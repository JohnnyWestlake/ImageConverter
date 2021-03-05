#pragma once
#include "pch.h"

using namespace Windows::Graphics::Imaging;
using namespace Windows::Foundation;
using namespace Windows::Storage;
using namespace Windows::Foundation::Collections;


namespace ImageConverter {
	namespace Core {
		namespace CX {
			public ref class BitmapConversionSettings sealed
			{
			public:
				BitmapConversionSettings() {}

				BitmapConversionSettings(Platform::Guid encoderId, Platform::String^ fileExtension, IVectorView<BitmapOption^>^ options)
				{
					EncoderId = encoderId;
					FileExtension = fileExtension;
					Options = options;
					Interpolation = BitmapInterpolationMode::Fant;
				}

				property Platform::Guid EncoderId;

				/// <summary>
				/// File extension to use when creating the converted file.
				/// </summary>
				property Platform::String^ FileExtension;

				/// <summary>
				/// File name not including file extension to use when creating the converted file.
				/// If not set, uses the source file's DisplayName
				/// </summary>
				property Platform::String^ TargetFileName;

				property int ScaledWidth;

				property int ScaledHeight;

				property bool CopyMetadata;

				property BitmapFrameHandling FrameHandling;

				property BitmapInterpolationMode Interpolation;

				property IVectorView<BitmapOption^>^ Options;

				property CreationCollisionOption CollisionOption;
			};
		}
	}
}
