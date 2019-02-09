#include "pch.h"
#include "BitmapEncoderFactory.h"
#include "BitmapConversionSettings.h"
#include <concurrent_vector.h>

using namespace ImageConverter::Core::CX;
using namespace Platform;
using namespace Platform::Collections;
using namespace concurrency;
using namespace Windows::Graphics::Imaging;
using namespace Windows::Storage;

BitmapEncoderFactory::BitmapEncoderFactory()
{

}

IAsyncOperation<BitmapConversionResult^>^  BitmapEncoderFactory::EncodeAsync(
	StorageFile^ file,
	IStorageFolder^ targetFolder,
	BitmapConversionSettings^ settings)
{
	return create_async([file, targetFolder, settings]
		{
			return create_task(file->OpenAsync(FileAccessMode::Read))
			.then([file, settings, targetFolder](IRandomAccessStream^ inputStream)
			{
				BitmapConversionResult^ result = ref new BitmapConversionResult();
				try
				{
					// 2. Try to decode file as an image
					return create_task(BitmapDecoder::CreateAsync(inputStream))
						.then([file, settings, targetFolder, result, inputStream](BitmapDecoder^ decoder)
					{
						delete inputStream;
						try
						{
							// 3. Create output file
							return create_task(targetFolder->CreateFileAsync(file->DisplayName + settings->FileExtension, settings->CollisionOption))
								.then([settings, result, decoder](StorageFile^ outputFile)
							{
								// 4. Open output stream
								return create_task(outputFile->OpenAsync(FileAccessMode::ReadWrite))
									.then([settings, result, decoder, outputFile](IRandomAccessStream^ outputStream)
								{
									// 5. Encode
									return create_task(EncodeInternalAsync(decoder, outputStream, settings))
										.then([result, outputStream, outputFile, decoder]
									{
										delete decoder;
										delete outputStream;

										// 6. Get result file size
										return create_task(outputFile->GetBasicPropertiesAsync())
											.then([result](FileProperties::BasicProperties^ properties)
										{
											result->ResultFileSize = properties->Size;
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
							return task_from_result(result);
						}

					}, task_continuation_context::use_arbitrary());
				}
				catch (Exception^ ex)
				{
					result->Status = "Could not decode file";
					return task_from_result(result);
				}

			}, task_continuation_context::use_arbitrary());
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

IAsyncAction^ BitmapEncoderFactory::EncodeInternalAsync(
	BitmapDecoder^ decoder,
	IRandomAccessStream^ outputStream,
	BitmapConversionSettings^ settings)
{
	outputStream->Size = 0;
	return create_async([decoder, outputStream, settings]
	{
		return create_task(CreateEncoderAsync(outputStream, settings)).then([decoder, settings](BitmapEncoder^ encoder)
		{
			return create_task(decoder->GetPixelDataAsync()).then([encoder, decoder, settings](PixelDataProvider^ pixeldata)
			{
				encoder->SetPixelData(
					decoder->BitmapPixelFormat,
					decoder->BitmapAlphaMode,
					decoder->OrientedPixelWidth,
					decoder->OrientedPixelHeight,
					decoder->DpiX,
					decoder->DpiY,
					pixeldata->DetachPixelData());

				bool needsTransfrom =
					(settings->ScaledHeight > 0 && decoder->OrientedPixelHeight > settings->ScaledHeight)
					|| (settings->ScaledWidth > 0 && decoder->OrientedPixelWidth > settings->ScaledWidth);

				if (needsTransfrom)
				{
					double destWidth = settings->ScaledWidth > 0 ? (double)min(settings->ScaledWidth, decoder->OrientedPixelWidth) : decoder->OrientedPixelWidth;
					double destHeight = settings->ScaledHeight > 0 ? (double)min(settings->ScaledHeight, decoder->OrientedPixelHeight) : decoder->OrientedPixelHeight;

					double scale = min(destWidth / (double)decoder->OrientedPixelWidth, destHeight / (double)decoder->OrientedPixelHeight);

					encoder->BitmapTransform->ScaledHeight = (double)decoder->OrientedPixelHeight * scale;
					encoder->BitmapTransform->ScaledWidth = (double)decoder->OrientedPixelWidth * scale;

					encoder->BitmapTransform->InterpolationMode = BitmapInterpolationMode::Fant;
				}

				return create_task(encoder->FlushAsync()).then([encoder]
				{
					delete encoder;
				}, task_continuation_context::use_arbitrary());
			}, task_continuation_context::use_arbitrary());
		}, task_continuation_context::use_arbitrary());
	});
}