#pragma once
#include "BitmapOption.h"

using namespace Windows::Graphics::Imaging;
using namespace Windows::Foundation;
using namespace Windows::Storage::Streams;
using namespace Windows::Foundation::Collections;


namespace ImageConverter_Core_CX
{
	public ref class BitmapEncoderFactory sealed
	{
	public:
		static IAsyncAction^ EncodeAsync(
			BitmapDecoder^ decoder,
			Platform::Guid encoderId,
			IRandomAccessStream^ outputStream,
			IVectorView<BitmapOption^>^ options);

		//static IAsyncAction^ EncodeAsync(
		//	BitmapEncoder^ encoder,
		//	BitmapDecoder^ decoder);

	private:
		BitmapEncoderFactory();
		
		static IAsyncOperation<BitmapEncoder^>^ CreateEncoderAsync(
			Platform::Guid id, 
			IRandomAccessStream^ stream,
			IVectorView<BitmapOption^>^ options);
	};
}
