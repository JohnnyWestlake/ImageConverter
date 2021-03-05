#pragma once
#include "pch.h"

using namespace Windows::Graphics::Imaging;
using namespace Windows::Foundation;
using namespace Windows::Storage::Streams;
using namespace Windows::Foundation::Collections;


namespace ImageConverter {
	namespace Core {
		namespace CX {
			public interface class IBitmapEncodingOption
			{
				BitmapOption^ GetValue();
			};
		}
	}
}
