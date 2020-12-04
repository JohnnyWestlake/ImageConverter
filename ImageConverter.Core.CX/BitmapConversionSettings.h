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

				property Platform::String^ FileExtension;

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
