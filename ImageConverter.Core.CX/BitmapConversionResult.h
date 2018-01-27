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
				property bool Success
				{
					bool get() { return _success; }
					void set(bool value) { _success = value; }
				}

				property unsigned long long ResultFileSize
				{
					unsigned long long get() { return _resultFileSize; }
					void set(unsigned long long value) { _resultFileSize = value; }
				}

				property Platform::String^ Status
				{
					virtual Platform::String^ get() { return _status; }
					void set(Platform::String^ value) { _status = value; }
				}

			internal:
				BitmapConversionResult() {}

				BitmapConversionResult(bool success, Platform::String^ status)
				{
					_success = success;
					_status = status;
				}

			private:
				Platform::String^ _status;
				bool _success;
				unsigned long long _resultFileSize;

			};
		}
	}
}
