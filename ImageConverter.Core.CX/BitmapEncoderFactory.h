#pragma once
#include "BitmapOption.h"
#include "BitmapConversionSettings.h"
#include "BitmapConversionResult.h"
#include "pplawait.h"

using namespace Platform;
using namespace Windows::Graphics::Imaging;
using namespace Windows::Foundation;
using namespace Windows::Storage;
using namespace Windows::Storage::Streams;
using namespace Windows::Foundation::Collections;
using namespace concurrency;


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

				static IAsyncOperation<BitmapConversionResult^>^ EncodeFrameAsync(
					BitmapDecoder^ decoder,
					BitmapConversionResult^ result,
					Platform::String^ baseName,
					BitmapConversionSettings^ settings,
					IStorageFolder^ targetFolder,
					UINT frameIndex);

				static IAsyncOperation<BitmapEncoder^>^ CreateEncoderAsync(
					IRandomAccessStream^ stream,
					BitmapConversionSettings^ settings);

				static task<BitmapConversionResult^> EncodeInternalAsync(
					StorageFile^ file,
					IStorageFolder^ targetFolder,
					BitmapConversionSettings^ settings);

				static task<bool> HandleEncodeAsync(
					BitmapDecoder^ decoder,
					UINT frameIndex,
					IRandomAccessStream^ outputStream,
					BitmapConversionSettings^ settings);

				static task<bool> HandleMetadataAsync(
					BitmapDecoder^ decoder,
					BitmapEncoder^ encoder);

				static task<bool> TryCopyMetadataSetAsync(
					BitmapDecoder^ decoder,
					BitmapEncoder^ encoder,
					String^ decodeFramePath,
					String^ encodeFramePath);
			};
		}
	}
}
