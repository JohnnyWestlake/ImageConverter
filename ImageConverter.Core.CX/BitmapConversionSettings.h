#pragma once
#include "BitmapOption.h"

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
					_encoderId = encoderId;
					_fileExtension = fileExtension;
					_options = options;
				}

				property Platform::Guid EncoderId
				{
					Platform::Guid get() { return _encoderId; }
					void set(Platform::Guid value) { _encoderId = value; }
				}

				property Platform::String^ FileExtension
				{
					virtual Platform::String^ get() { return _fileExtension; }
					void set(Platform::String^ value) { _fileExtension = value; }
				}

				property IVectorView<BitmapOption^>^ Options
				{
					virtual IVectorView<BitmapOption^>^ get() { return _options; }
					void set(IVectorView<BitmapOption^>^ value) { _options = value; }
				}

				property CreationCollisionOption CollisionOption
				{
					virtual CreationCollisionOption get() { return _collisionOption; }
					void set(CreationCollisionOption value) { _collisionOption = value; }
				}

			private:
				Platform::Guid _encoderId;
				Platform::String^ _fileExtension;
				IVectorView<BitmapOption^>^ _options;
				CreationCollisionOption _collisionOption;

			};
		}
	}
}
