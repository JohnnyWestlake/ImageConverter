#pragma once
#include "BitmapOption.h"

using namespace Windows::Graphics::Imaging;
using namespace Windows::Foundation;
using namespace Windows::Storage::Streams;
using namespace Windows::Foundation::Collections;


namespace ImageConverter{
	namespace Core {
		namespace CX {
			public ref class BitmapConversionResult sealed
			{
			public:
				property bool Success;

				property unsigned long long ResultFileSize;

				property Platform::String^ Status;

			internal:
				BitmapConversionResult() {}

				BitmapConversionResult(bool success, Platform::String^ status)
				{
					Success = success;
					Status = status;
				}
			};
		}
	}
}
