#pragma once
#include "BitmapOption.h"
#include "BitmapConversionSettings.h"
#include "BitmapConversionResult.h"

using namespace Windows::Graphics::Imaging;
using namespace Windows::Foundation;
using namespace Windows::Storage;
using namespace Windows::Storage::Streams;
using namespace Windows::Foundation::Collections;


namespace ImageConverter {
	namespace Core {
		namespace CX {
			public ref class BitmapEncoderFactory sealed
			{
			public:
				static IAsyncOperation<BitmapConversionResult^>^ EncodeAsync(
					StorageFile^ file,
					IStorageFolder^ targetFolder,
					BitmapConversionSettings^ settings);

			private:
				BitmapEncoderFactory();

				static IAsyncAction^ EncodeInternalAsync(
					BitmapDecoder^ decoder,
					Platform::Guid encoderId,
					IRandomAccessStream^ outputStream,
					IVectorView<BitmapOption^>^ options);

				static IAsyncOperation<BitmapEncoder^>^ CreateEncoderAsync(
					Platform::Guid id,
					IRandomAccessStream^ stream,
					IVectorView<BitmapOption^>^ options);
			};
		}
	}
}
