#include "pch.h"
#include "BitmapEncoderFactory.h"
#include "BitmapConversionSettings.h"
#include <concurrent_vector.h>
#include "pplawait.h"

using namespace ImageConverter::Core::CX;
using namespace Platform;
using namespace Platform::Collections;
using namespace concurrency;
using namespace Windows::Graphics::Imaging;
using namespace Windows::Storage;

BitmapEncoderFactory::BitmapEncoderFactory()
{

}

IAsyncOperation<BitmapConversionResult^>^ BitmapEncoderFactory::EncodeAsync(
	StorageFile^ file,
	IStorageFolder^ targetFolder,
	BitmapConversionSettings^ settings)
{
	return create_async([file, targetFolder, settings]
		{
			return create_task(EncodeInternalAsync(file, targetFolder, settings));
		});
}

task<BitmapConversionResult^> BitmapEncoderFactory::EncodeInternalAsync(
	StorageFile^ file,
	IStorageFolder^ targetFolder,
	BitmapConversionSettings^ settings)
{
	BitmapConversionResult^ result = ref new BitmapConversionResult();
	result->Success = true;

	BitmapDecoder^ decoder;

	try
	{
		IRandomAccessStream^ inputStream = co_await file->OpenAsync(FileAccessMode::Read);
		decoder = co_await BitmapDecoder::CreateAsync(inputStream);
	}
	catch (Exception^ ex)
	{
		result->Status = "Could not open / decode file";
		result->Success = false;
	}

	if (decoder != nullptr)
	{
		for (UINT i = 0; i < decoder->FrameCount; i++)
		{
			if (result->Success)
				co_await EncodeFrameAsync(decoder, result, file->DisplayName, settings, targetFolder, i);
		}
	}

	co_return result;
}

/* This is left as IAsyncOperation as the task version dies when HEIC encoding */
IAsyncOperation<BitmapConversionResult^>^ BitmapEncoderFactory::EncodeFrameAsync(
	BitmapDecoder^ decoder,
	BitmapConversionResult^ result,
	Platform::String^ baseName,
	BitmapConversionSettings^ settings,
	IStorageFolder^ targetFolder,
	UINT frameIndex)
{
	return create_async([&]
		{
			try
			{
				// 1. Create file name
				String^ fileName = baseName + settings->FileExtension;
				if (decoder->FrameCount > 1)
					fileName = baseName + L" " + frameIndex + settings->FileExtension;

				// 2. Create output file
				return create_task(targetFolder->CreateFileAsync(fileName, settings->CollisionOption))
					.then([settings, decoder, result, frameIndex](StorageFile^ outputFile)
						{
							// 3. Open output stream
							return create_task(outputFile->OpenAsync(FileAccessMode::ReadWrite))
								.then([settings, decoder, outputFile, frameIndex, result](IRandomAccessStream^ outputStream)
									{
										// 4. Encode
										return HandleEncodeAsync(decoder, frameIndex, outputStream, settings)
											.then([outputStream, outputFile, decoder, result](bool r)
												{
													delete decoder;
													delete outputStream;

													// 5. Get result file size
													return create_task(outputFile->GetBasicPropertiesAsync())
														.then([result](FileProperties::BasicProperties^ properties)
															{
																result->ResultFileSize += properties->Size;
																result->Success = true;
																return result;
															}, task_continuation_context::use_arbitrary());
												}, task_continuation_context::use_arbitrary());
									}, task_continuation_context::use_arbitrary());
						}, task_continuation_context::use_arbitrary());
			}
			catch (Exception^ ex)
			{
				result->Status = "Could not write to output file";
				result->Success = false;
				return task_from_result(result);
			}
		});
}


IAsyncOperation<BitmapEncoder^>^ BitmapEncoderFactory::CreateEncoderAsync(
	IRandomAccessStream^ stream,
	BitmapConversionSettings^ settings)
{
	auto map = ref new Map<Platform::String^, BitmapTypedValue^>();
	for each (BitmapOption^ var in settings->Options)
	{
		map->Insert(var->Name, var->Value);
	}

	return BitmapEncoder::CreateAsync(settings->EncoderId, stream, map);
}

task<bool> BitmapEncoderFactory::HandleEncodeAsync(
	BitmapDecoder^ decoder,
	UINT frameIndex,
	IRandomAccessStream^ outputStream,
	BitmapConversionSettings^ settings)
{
	outputStream->Size = 0;

	// 1. Get pixel data for the correct frame
	IBitmapFrame^ frame = decoder;
	if (frameIndex > 0)
		frame = co_await decoder->GetFrameAsync(frameIndex);

	PixelDataProvider^ pixeldata = co_await frame->GetPixelDataAsync();

	// 2. Create the encoder with the pixel data
	BitmapEncoder^ encoder = co_await CreateEncoderAsync(outputStream, settings);
	encoder->SetPixelData(
		frame->BitmapPixelFormat,
		frame->BitmapAlphaMode,
		frame->OrientedPixelWidth,
		frame->OrientedPixelHeight,
		frame->DpiX,
		frame->DpiY,
		pixeldata->DetachPixelData());

	// 3. Apply scaling
	bool needsTransfrom =
		(settings->ScaledHeight > 0 && frame->OrientedPixelHeight > settings->ScaledHeight)
		|| (settings->ScaledWidth > 0 && frame->OrientedPixelWidth > settings->ScaledWidth);

	if (needsTransfrom)
	{
		double destWidth = settings->ScaledWidth > 0 ? (double)min(settings->ScaledWidth, decoder->OrientedPixelWidth) : decoder->OrientedPixelWidth;
		double destHeight = settings->ScaledHeight > 0 ? (double)min(settings->ScaledHeight, decoder->OrientedPixelHeight) : decoder->OrientedPixelHeight;

		double scale = min(destWidth / (double)decoder->OrientedPixelWidth, destHeight / (double)decoder->OrientedPixelHeight);

		encoder->BitmapTransform->ScaledHeight = (double)decoder->OrientedPixelHeight * scale;
		encoder->BitmapTransform->ScaledWidth = (double)decoder->OrientedPixelWidth * scale;

		encoder->BitmapTransform->InterpolationMode = BitmapInterpolationMode::Fant;
	}

	// 4. Copy metadata
	if (settings->CopyMetadata)
		co_await HandleMetadataAsync(decoder, encoder);

	// 5. Commit
	co_await encoder->FlushAsync();
	delete encoder;

	co_return true;
}

task<bool> BitmapEncoderFactory::HandleMetadataAsync(
	BitmapDecoder^ decoder,
	BitmapEncoder^ encoder)
{
	auto d = decoder->DecoderInformation->CodecId;
	auto e = encoder->EncoderInformation->CodecId;

	auto ifd = co_await TryCopyMetadataSetAsync(decoder, encoder, GetIFDPath(d), GetIFDPath(e));
	auto exif = co_await TryCopyMetadataSetAsync(decoder, encoder, GetExifPath(d), GetExifPath(e));
	auto xmp = co_await TryCopyMetadataSetAsync(decoder, encoder, GetXMPPath(d), GetXMPPath(e));
	auto gps = co_await TryCopyMetadataSetAsync(decoder, encoder, GetGPSPath(d), GetGPSPath(e));

	co_return ifd || exif || xmp || gps;
}

task<bool> BitmapEncoderFactory::TryCopyMetadataSetAsync(
	BitmapDecoder^ decoder, 
	BitmapEncoder^ encoder,
	String^ decodeFramePath,
	String^ encodeFramePath)
{
	try
	{
		if (decodeFramePath == nullptr || encodeFramePath == nullptr)
			co_return false;

		auto vec = ref new Vector<String^>();
		vec->Append(decodeFramePath);

		BitmapPropertySet^ props = co_await decoder->BitmapProperties->GetPropertiesAsync(vec);
		if (props->Size == 0 || !props->HasKey(decodeFramePath))
			co_return false;

		BitmapTypedValue^ val = props->Lookup(decodeFramePath);

		auto map = ref new Map<String^, BitmapTypedValue^>();
		map->Insert(encodeFramePath, val);

		co_await encoder->BitmapProperties->SetPropertiesAsync(map);
		co_return true;
	}
	catch (Exception^ ex)
	{
		co_return false;
	}
}




Platform::String^ BitmapEncoderFactory::GetExifPath(Platform::Guid id)
{
	if (id == BitmapDecoder::JpegDecoderId || id == BitmapEncoder::JpegEncoderId)
		return ref new Platform::String(L"/app1/ifd/exif");
	else if (id == BitmapDecoder::TiffDecoderId || id == BitmapDecoder::JpegXRDecoderId
		|| id == BitmapEncoder::TiffEncoderId || id == BitmapEncoder::JpegXREncoderId
		|| id == BitmapEncoder::HeifEncoderId || id == BitmapDecoder::HeifDecoderId
		|| id == BitmapDecoder::WebpDecoderId)
		return ref new Platform::String(L"/ifd/exif");
	else
		return nullptr;
}

Platform::String^ BitmapEncoderFactory::GetXMPPath(Platform::Guid id)
{
	if (id == BitmapDecoder::JpegDecoderId || id == BitmapEncoder::JpegEncoderId)
		return ref new Platform::String(L"/xmp");
	else if (id == BitmapDecoder::TiffDecoderId || id == BitmapDecoder::JpegXRDecoderId
		|| id == BitmapEncoder::TiffEncoderId || id == BitmapEncoder::JpegXREncoderId
		|| id == BitmapEncoder::HeifEncoderId || id == BitmapDecoder::HeifDecoderId
		|| id == BitmapDecoder::WebpDecoderId)
		return ref new Platform::String(L"/ifd/xmp");
	else
		return nullptr;
}

Platform::String^ BitmapEncoderFactory::GetGPSPath(Platform::Guid id)
{
	if (id == BitmapDecoder::JpegDecoderId || id == BitmapEncoder::JpegEncoderId)
		return ref new Platform::String(L"/app1/ifd/gps");
	else if (id == BitmapDecoder::TiffDecoderId || id == BitmapDecoder::JpegXRDecoderId
		|| id == BitmapEncoder::TiffEncoderId || id == BitmapEncoder::JpegXREncoderId
		|| id == BitmapEncoder::HeifEncoderId || id == BitmapDecoder::HeifDecoderId
		|| id == BitmapDecoder::WebpDecoderId)
		return ref new Platform::String(L"/ifd/gps");
	else
		return nullptr;
}

Platform::String^ BitmapEncoderFactory::GetIFDPath(Platform::Guid id)
{
	if (id == BitmapDecoder::JpegDecoderId || id == BitmapEncoder::JpegEncoderId)
		return ref new Platform::String(L"/app1/ifd");
	else if (id == BitmapDecoder::TiffDecoderId || id == BitmapDecoder::JpegXRDecoderId
		|| id == BitmapEncoder::TiffEncoderId || id == BitmapEncoder::JpegXREncoderId
		|| id == BitmapEncoder::HeifEncoderId || id == BitmapDecoder::HeifDecoderId
		|| id == BitmapDecoder::WebpDecoderId)
		return ref new Platform::String(L"/ifd");
	else
		return nullptr;
}