#pragma once

using namespace Windows::Graphics::Imaging;
using namespace Windows::Foundation;
using namespace Windows::Storage;
using namespace Windows::Foundation::Collections;


namespace ImageConverter {
	namespace Core {
		namespace CX {
			public enum class BitmapFrameHandling
			{
				/// <summary>
				/// Only the first frame of the bitmap is converted
				/// </summary>
				FirstFrameOnly = 0,

				/// <summary>
				/// All frames of the bitmap are converted to separate files
				/// </summary>
				ExtractAllSeperately = 1
				
			};
		}
	}
}