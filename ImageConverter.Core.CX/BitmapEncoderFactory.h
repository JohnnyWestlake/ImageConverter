#pragma once
#include "BitmapOption.h"
#include "BitmapConversionSettings.h"
#include "BitmapConversionResult.h"

using namespace Platform;
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

				static Platform::String^ GetIFDPath(Platform::Guid codecId);
				static Platform::String^ GetGPSPath(Platform::Guid codecId);
				static Platform::String^ GetXMPPath(Platform::Guid codecId);
				static Platform::String^ GetExifPath(Platform::Guid codecId);

				static IAsyncAction^ EncodeInternalAsync(
					BitmapDecoder^ decoder,
					IRandomAccessStream^ outputStream,
					BitmapConversionSettings^ settings);

				static IAsyncOperation<BitmapEncoder^>^ CreateEncoderAsync(
					IRandomAccessStream^ stream,
					BitmapConversionSettings^ settings);

				static IAsyncOperation<bool>^ TryCopyMetadataAsync(
					BitmapDecoder^ decoder,
					BitmapEncoder^ encoder);

				static IAsyncOperation<bool>^ TryCopyMetadataSetAsync(
					BitmapDecoder^ decoder,
					BitmapEncoder^ encoder,
					String^ decodeFramePath,
					String^ encodeFramePath);
			};
		}
	}
}
