#pragma once

using namespace Windows::Graphics::Imaging;

namespace ImageConverter {
	namespace Core {
		namespace CX {
			public ref class BitmapOption sealed
			{
			public:
				BitmapOption(Platform::String^ name, BitmapTypedValue^ bitmapValue);

				property BitmapTypedValue^ Value;

				property Platform::String^ Name;
			};
		}
	}
}
