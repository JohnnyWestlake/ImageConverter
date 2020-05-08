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

IAsyncOperation<BitmapConversionResult^>^ BitmapEncoderFactory::EncodeAsync(
	StorageFile^ sourceFile,
	IStorageFolder^ targetFolder,
	BitmapConversionSettings^ settings)
{
	return create_async([sourceFile, targetFolder, settings]
		{
			return create_task(sourceFile->OpenAsync(FileAccessMode::Read))
				.then([sourceFile, settings, targetFolder](IRandomAccessStream^ inputStream)
					{
						BitmapConversionResult^ result = ref new BitmapConversionResult();
						try
						{
							// 2. Try to decode file as an image
							return create_task(BitmapDecoder::CreateAsync(inputStream))
								.then([sourceFile, settings, targetFolder, result, inputStream](BitmapDecoder^ decoder)
									{
										delete inputStream;
										try
										{
											return create_task(EncodeFramesAsync(decoder, result, sourceFile->DisplayName, settings, targetFolder, 0))
												.then([](BitmapConversionResult^ r)
													{
														return r;
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

IAsyncOperation<BitmapConversionResult^>^ BitmapEncoderFactory::EncodeAsync(
	IRandomAccessStream^ inputStream, 
	IRandomAccessStream^ outputStream, 
	UINT inputFrameIndex,
	bool keepinputStreamAlive, 
	BitmapConversionSettings^ settings)
{
	return create_async([inputStream, outputStream, settings, keepinputStreamAlive, inputFrameIndex]
		{
			BitmapConversionResult^ result = ref new BitmapConversionResult();
			try
			{
				return create_task(BitmapDecoder::CreateAsync(inputStream))
					.then([outputStream, settings, keepinputStreamAlive, result, inputStream, inputFrameIndex](BitmapDecoder^ decoder)
						{
							if (!keepinputStreamAlive)
							{
								inputStream->Dispose();
								delete inputStream;
							}

							try
							{
								return create_task(EncodeInternalAsync(decoder, inputFrameIndex, outputStream, settings))
									.then([decoder, result]
										{
											delete decoder;
											result->Success = true;
											return result;
										}, task_continuation_context::use_arbitrary());
							}
							catch (Exception^ ex)
							{
								result->Status = "Could not write to output stream";
								return task_from_result(result);
							}

						}, task_continuation_context::use_arbitrary());
			}
			catch (Exception^ ex)
			{
				result->Status = "Could not decode input stream";
				return task_from_result(result);
			}
		});
}

IAsyncOperation<BitmapEncoder^>^ BitmapEncoderFactory::CreateEncoderAsync(
	IRandomAccessStream^ stream,
	BitmapConversionSettings^ settings)
{
	auto map = ref new Map<Platform::String^, BitmapTypedValue^>();
	for each (BitmapOption ^ var in settings->Options)
	{
		map->Insert(var->Name, var->Value);
	}

	return BitmapEncoder::CreateAsync(settings->EncoderId, stream, map);
}

IAsyncAction^ BitmapEncoderFactory::EncodeInternalAsync(
	BitmapDecoder^ decoder,
	UINT frameIndex,
	IRandomAccessStream^ outputStream,
	BitmapConversionSettings^ settings)
{
	outputStream->Size = 0;
	return create_async([decoder, frameIndex, outputStream, settings]
		{
			return create_task(GetFrameAsync(decoder, frameIndex)).then([decoder, frameIndex, outputStream, settings](IBitmapFrame^ frame)
			{
				return create_task(CreateEncoderAsync(outputStream, settings)).then([decoder, frameIndex, frame, settings](BitmapEncoder^ encoder)
					{
						return create_task(frame->GetPixelDataAsync()).then([decoder, frameIndex, encoder, frame, settings](PixelDataProvider^ pixeldata)
							{
								encoder->SetPixelData(
									frame->BitmapPixelFormat,
									frame->BitmapAlphaMode,
									frame->OrientedPixelWidth,
									frame->OrientedPixelHeight,
									frame->DpiX,
									frame->DpiY,
									pixeldata->DetachPixelData());

								bool needsTransfrom =
									(settings->ScaledHeight > 0 && frame->OrientedPixelHeight > settings->ScaledHeight)
									|| (settings->ScaledWidth > 0 && frame->OrientedPixelWidth > settings->ScaledWidth);

								if (needsTransfrom)
								{
									double destWidth = settings->ScaledWidth > 0 ? (double)min(settings->ScaledWidth, decoder->OrientedPixelWidth) : frame->OrientedPixelWidth;
									double destHeight = settings->ScaledHeight > 0 ? (double)min(settings->ScaledHeight, decoder->OrientedPixelHeight) : frame->OrientedPixelHeight;

									double scale = min(destWidth / (double)decoder->OrientedPixelWidth, destHeight / (double)decoder->OrientedPixelHeight);

									encoder->BitmapTransform->ScaledHeight = (double)frame->OrientedPixelHeight * scale;
									encoder->BitmapTransform->ScaledWidth = (double)frame->OrientedPixelWidth * scale;

									encoder->BitmapTransform->InterpolationMode = BitmapInterpolationMode::Fant;
								}

								return create_task(HandleMetadataAsync(decoder, encoder, settings->CopyMetadata)).then([frame, frameIndex, encoder](bool success)
									{
										return create_task(encoder->FlushAsync()).then([frame, frameIndex, encoder]
											{
												delete encoder;

												if (frameIndex > 0)
													delete frame;

											}, task_continuation_context::use_arbitrary());
									}, task_continuation_context::use_arbitrary());
							}, task_continuation_context::use_arbitrary());
					}, task_continuation_context::use_arbitrary());
				}, task_continuation_context::use_arbitrary());
			});
}

IAsyncOperation<BitmapConversionResult^>^ BitmapEncoderFactory::EncodeFramesAsync(
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
				if (decoder->FrameCount > 1 && settings->FrameHandling == BitmapFrameHandling::ExtractAllSeperately)
					fileName = baseName + L" " + frameIndex + settings->FileExtension;

				// 2. Create output file
				return create_task(targetFolder->CreateFileAsync(fileName, settings->CollisionOption))
					.then([decoder, frameIndex, settings, result, baseName, targetFolder](StorageFile^ outputFile)
						{
							// 3. Open output stream
							return create_task(outputFile->OpenAsync(FileAccessMode::ReadWrite))
								.then([decoder, frameIndex, settings, result, baseName, targetFolder, outputFile](IRandomAccessStream^ outputStream)
									{
										// 4. Encode
										return create_task(EncodeInternalAsync(decoder, frameIndex, outputStream, settings))
											.then([decoder, frameIndex, settings, result, baseName, targetFolder, outputFile, outputStream]
												{
													delete outputStream;

													// 5. Get result file size
													return create_task(outputFile->GetBasicPropertiesAsync())
														.then([decoder, frameIndex, settings, result, baseName, targetFolder](FileProperties::BasicProperties^ properties)
															{
																result->ResultFileSize += properties->Size;
																result->Success = true;

																if (settings->FrameHandling == BitmapFrameHandling::FirstFrameOnly || decoder->FrameCount - 1 == frameIndex)
																{
																	delete decoder;
																	return task_from_result(result);
																}
																else
																	return create_task(EncodeFramesAsync(
																		decoder,
																		result,
																		baseName,
																		settings,
																		targetFolder,
																		frameIndex + 1)).then([](BitmapConversionResult^ r)
																			{
																				return r;
																			}, task_continuation_context::use_arbitrary());
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

IAsyncOperation<bool>^ BitmapEncoderFactory::HandleMetadataAsync(
	BitmapDecoder^ decoder,
	BitmapEncoder^ encoder,
	bool copy)
{
	return create_async([decoder, encoder, copy]
		{
			if (!copy)
				return task_from_result(false);

			return create_task(TryCopyMetadataSetAsync(decoder, encoder, GetIFDPath(decoder->DecoderInformation->CodecId), GetIFDPath(encoder->EncoderInformation->CodecId)))
				.then([decoder, encoder](bool ifd)
					{
						return create_task(TryCopyMetadataSetAsync(decoder, encoder, GetExifPath(decoder->DecoderInformation->CodecId), GetExifPath(encoder->EncoderInformation->CodecId)))
							.then([decoder, encoder, ifd](bool exif)
								{
									return create_task(TryCopyMetadataSetAsync(decoder, encoder, GetXMPPath(decoder->DecoderInformation->CodecId), GetXMPPath(encoder->EncoderInformation->CodecId)))
										.then([decoder, encoder, ifd, exif](bool xmp)
											{
												return create_task(TryCopyMetadataSetAsync(decoder, encoder, GetGPSPath(decoder->DecoderInformation->CodecId), GetGPSPath(encoder->EncoderInformation->CodecId)))
													.then([ifd, exif, xmp](bool gps)
														{
															return ifd || exif || xmp || gps;
														}, task_continuation_context::use_arbitrary());
											}, task_continuation_context::use_arbitrary());
								}, task_continuation_context::use_arbitrary());
					}, task_continuation_context::use_arbitrary());
		});
}

IAsyncOperation<bool>^ BitmapEncoderFactory::TryCopyMetadataSetAsync(
	BitmapDecoder^ decoder,
	BitmapEncoder^ encoder,
	String^ decodeFramePath,
	String^ encodeFramePath)
{
	return create_async([decoder, encoder, decodeFramePath, encodeFramePath]
		{
			try
			{
				if (decodeFramePath == nullptr || encodeFramePath == nullptr)
					return task_from_result(false);

				auto vec = ref new Vector<String^>();
				vec->Append(decodeFramePath);
				return create_task(decoder->BitmapProperties->GetPropertiesAsync(vec)).then([encoder, decodeFramePath, encodeFramePath](BitmapPropertySet^ props)
					{
						if (props->Size == 0 || !props->HasKey(decodeFramePath))
							return task_from_result(false);

						BitmapTypedValue^ exif = props->Lookup(decodeFramePath);

						auto map = ref new Map<String^, BitmapTypedValue^>();
						map->Insert(encodeFramePath, exif);

						return create_task(encoder->BitmapProperties->SetPropertiesAsync(map)).then([]
							{
								return task_from_result(true);
							}, task_continuation_context::use_arbitrary());
					});

			}
			catch (Exception^ ex)
			{
				return task_from_result(false);
			}
		});

}

IAsyncOperation<IBitmapFrame^>^ BitmapEncoderFactory::GetFrameAsync(BitmapDecoder^ decoder, UINT frameIndex)
{
	return create_async([decoder, frameIndex]
	{
		if (frameIndex == 0)
			return task_from_result((IBitmapFrame^)decoder);

		return create_task(decoder->GetFrameAsync(frameIndex)).then([](IBitmapFrame^ frame)
			{
				return frame;
			}, task_continuation_context::use_arbitrary());
	});
	
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