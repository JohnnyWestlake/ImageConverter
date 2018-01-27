#pragma once

using namespace Windows::Graphics::Imaging;

namespace ImageConverter {
	namespace Core {
		namespace CX {
			public ref class BitmapOption sealed
			{
			public:
				BitmapOption(Platform::String^ name, BitmapTypedValue^ bitmapValue);

				property BitmapTypedValue^ Value
				{
					BitmapTypedValue^ get() { return bitmapvalue; }
					void set(BitmapTypedValue^ value) { bitmapvalue = value; }
				}

				property Platform::String^ Name
				{
					Platform::String^ get() { return name; }
					void set(Platform::String^ value) { name = value; }
				}

			private:
				BitmapTypedValue ^ bitmapvalue;
				Platform::String^ name;
			};
		}
	}
}
