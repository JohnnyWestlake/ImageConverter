#include "pch.h"
#include "BitmapEncoderFactory.h"
#include <concurrent_vector.h>

using namespace ImageConverter_Core_CX;
using namespace Platform;
using namespace Platform::Collections;
using namespace concurrency;

BitmapEncoderFactory::BitmapEncoderFactory()
{

}

IAsyncOperation<BitmapEncoder^>^ BitmapEncoderFactory::CreateEncoderAsync(
	Platform::Guid id, 
	IRandomAccessStream^ stream,
	IVectorView<BitmapOption^>^ options)
{
	auto map = ref new Map<Platform::String^, BitmapTypedValue^>();
	for each (BitmapOption^ var in options)
	{
		map->Insert(var->Name, var->Value);
	}

	return BitmapEncoder::CreateAsync(id, stream, map);
}

IAsyncAction^ BitmapEncoderFactory::EncodeAsync(
	BitmapDecoder^ decoder,
	Platform::Guid encoderId,
	IRandomAccessStream^ outputStream,
	IVectorView<BitmapOption^>^ options)
{
	outputStream->Size = 0;
	return create_async([decoder, encoderId, outputStream, options]
	{
		return create_task(CreateEncoderAsync(encoderId, outputStream, options)).then([decoder](BitmapEncoder^ encoder)
		{
			return create_task(decoder->GetPixelDataAsync()).then([encoder, decoder](PixelDataProvider^ pixeldata)
			{
				encoder->SetPixelData(
					decoder->BitmapPixelFormat,
					decoder->BitmapAlphaMode,
					decoder->OrientedPixelWidth,
					decoder->OrientedPixelHeight,
					decoder->DpiX,
					decoder->DpiY,
					pixeldata->DetachPixelData());
				return create_task(encoder->FlushAsync());
			}, task_continuation_context::use_arbitrary());
		}, task_continuation_context::use_arbitrary());
	});
}