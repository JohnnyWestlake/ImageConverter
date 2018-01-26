#pragma once

using namespace Windows::Graphics::Imaging;

namespace ImageConverter_Core_CX
{
    public ref class BitmapOption sealed
    {
		public:
			BitmapOption(Platform::String^ name, BitmapTypedValue^ bitmapValue);

			property BitmapTypedValue^ Value
			{
				virtual BitmapTypedValue^ get() { return bitmapvalue; }
				void set(BitmapTypedValue^ value) { bitmapvalue = value; }
			}

			property Platform::String^ Name
			{
				virtual Platform::String^ get() { return name; }
				void set(Platform::String^ value) { name = value; }
			}

	private:
		BitmapTypedValue^ bitmapvalue;
		Platform::String^ name;
    };
}
