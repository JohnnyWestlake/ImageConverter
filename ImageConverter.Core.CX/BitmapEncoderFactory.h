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
					StorageFile^ sourceFile,
					IStorageFolder^ targetFolder,
					BitmapConversionSettings^ settings);

				static IAsyncOperation<BitmapConversionResult^>^ EncodeAsync(
					IRandomAccessStream^ inputStream,
					IRandomAccessStream^ outputStream,
					UINT inputFrameIndex,
					bool keepinputStreamAlive,
					BitmapConversionSettings^ settings);

			private:
				BitmapEncoderFactory();

				static Platform::String^ GetIFDPath(Platform::Guid codecId);
				static Platform::String^ GetGPSPath(Platform::Guid codecId);
				static Platform::String^ GetXMPPath(Platform::Guid codecId);
				static Platform::String^ GetExifPath(Platform::Guid codecId);

				static Platform::String^ GetFileName(Platform::String^ defaultName, BitmapConversionSettings^ settings)
				{
					if (settings->TargetFileName == nullptr || settings->TargetFileName->IsEmpty())
						return defaultName;

					return settings->TargetFileName;
				}

				static IAsyncAction^ EncodeInternalAsync(
					BitmapDecoder^ decoder,
					UINT frameIndex,
					IRandomAccessStream^ outputStream,
					BitmapConversionSettings^ settings);

				static IAsyncOperation<BitmapConversionResult^>^ EncodeFramesAsync(
					BitmapDecoder^ decoder,
					BitmapConversionResult^ result,
					Platform::String^ baseName,
					BitmapConversionSettings^ settings,
					IStorageFolder^ targetFolder,
					UINT frameIndex);


				static IAsyncOperation<BitmapEncoder^>^ CreateEncoderAsync(
					IRandomAccessStream^ stream,
					BitmapConversionSettings^ settings);

				static IAsyncOperation<bool>^ HandleMetadataAsync(
					BitmapDecoder^ decoder,
					BitmapEncoder^ encoder,
					bool copy);

				static IAsyncOperation<bool>^ TryCopyMetadataSetAsync(
					BitmapDecoder^ decoder,
					BitmapEncoder^ encoder,
					String^ decodeFramePath,
					String^ encodeFramePath);

				static IAsyncOperation<IBitmapFrame^>^ GetFrameAsync(
					BitmapDecoder^ decoder,
					UINT frameIndex);
			};
		}
	}
}
