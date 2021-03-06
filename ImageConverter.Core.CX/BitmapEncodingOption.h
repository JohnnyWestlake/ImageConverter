#pragma once
#include "pch.h"

using namespace Windows::Graphics::Imaging;
using namespace Windows::Foundation;
using namespace Windows::Storage::Streams;
using namespace Windows::Foundation::Collections;
using namespace Platform;

namespace ImageConverter { namespace Core { namespace CX {

	public ref class JpegQualityOption sealed : IBitmapEncodingOption
	{
	public:
		static property IVectorView<Format>^ SupportedFormats
		{
			IVectorView<Format>^ get() { return _supportedFormats; }
		}

		property float ImageQuality
		{
			float get() { return _value; }
			void set(float v) { _value = v; }
		}

		static property String^ Description
		{
			String^ get() { return L"Higher values indicate higher quality at the cost of larger file sizes."; }
		}

		virtual BitmapOption^ GetValue()
		{
			auto value = ref new BitmapTypedValue(_value, PropertyType::Single);
			return ref new BitmapOption("ImageQuality", value);
		}

	private:
		static IVectorView<Format>^ GetFormats();
		static IVectorView<Format>^ _supportedFormats;
		float _value = 0.9f;
	};
	
	public ref class JpegChromaSubsamplingOption sealed : IBitmapEncodingOption
	{
	public:
		static property IVectorView<Format>^ SupportedFormats
		{
			IVectorView<Format>^ get() { return _supportedFormats; }
		}

		static property String^ Description
		{
			String^ get() { return L"Specifies which chroma subsampling mode will be used for image compression in JPEG images."; }
		}

		property JpegSubsamplingMode JpegYCrCbSubsampling
		{
			JpegSubsamplingMode get() { return _value; }
			void set(JpegSubsamplingMode v) { _value = v; }
		}

		virtual BitmapOption^ GetValue()
		{
			auto value = ref new BitmapTypedValue((byte)_value, PropertyType::UInt8);
			return ref new BitmapOption("JpegYCrCbSubsampling", value);
		}

	private:
		static IVectorView<Format>^ GetFormats();
		static IVectorView<Format>^ _supportedFormats;
		JpegSubsamplingMode _value = JpegSubsamplingMode::Default;
	};
	
	public ref class PngFilterModeOption sealed : IBitmapEncodingOption
	{
	public:
		static property IVectorView<Format>^ SupportedFormats
		{
			IVectorView<Format>^ get() { return _supportedFormats; }
		}

		static property String^ Description
		{
			String^ get()
			{
				return L"Specifies the filter used to optimize the image prior to image compression.\n" +
					"None does not perform any filtering and is typically the fastest but consumes the most space. " +
					"Sub, Up, Average and Paeth filtering perform differently across various images. " +
					"Adaptive filtering attempts to select the most efficient of the previous filter modes for each scanline in the image. This typically performs the slowest but consumes the least space.";
			}
		}

		property PngFilterMode FilterOption
		{
			PngFilterMode get() { return _value; }
			void set(PngFilterMode v) { _value = v; }
		}

		virtual BitmapOption^ GetValue()
		{
			auto value = ref new BitmapTypedValue((byte)_value, PropertyType::UInt8);
			return ref new BitmapOption("FilterOption", value);
		}

	private:
		static IVectorView<Format>^ GetFormats();
		static IVectorView<Format>^ _supportedFormats;
		PngFilterMode _value = PngFilterMode::Automatic;
	};
	
	public ref class InterlaceOptionOption sealed : IBitmapEncodingOption
	{
	public:
		static property IVectorView<Format>^ SupportedFormats
		{
			IVectorView<Format>^ get() { return _supportedFormats; }
		}

		static property String^ Description
		{
			String^ get()
			{
				return L"Specifies whether to encode the image data as interlaced.";
			}
		}

		property bool InterlaceOption
		{
			bool get() { return _value; }
			void set(bool v) { _value = v; }
		}

		virtual BitmapOption^ GetValue()
		{
			auto value = ref new BitmapTypedValue(_value, PropertyType::Boolean);
			return ref new BitmapOption("InterlaceOption", value);
		}

	private:
		static IVectorView<Format>^ GetFormats();
		static IVectorView<Format>^ _supportedFormats;
		bool _value = false;
	};
	
	public ref class LosslessOption sealed : IBitmapEncodingOption
	{
	public:
		static property IVectorView<Format>^ SupportedFormats
		{
			IVectorView<Format>^ get() { return _supportedFormats; }
		}

		static property String^ Description
		{
			String^ get() { return L"If enabled, the Image Quality option is ignored."; }
		}

		property bool Lossless
		{
			bool get() { return _value; }
			void set(bool v) { _value = v; }
		}

		virtual BitmapOption^ GetValue()
		{
			auto value = ref new BitmapTypedValue(_value, PropertyType::Boolean);
			return ref new BitmapOption("Lossless", value);
		}

	private:
		static IVectorView<Format>^ GetFormats();
		static IVectorView<Format>^ _supportedFormats;
		bool _value = false;
	};
	
	public ref class ProgressiveModeOption sealed : IBitmapEncodingOption
	{
	public:
		static property IVectorView<Format>^ SupportedFormats
		{
			IVectorView<Format>^ get() { return _supportedFormats; }
		}

		static property String^ Description
		{
			String^ get() { return L"Switch between sequential or progressive encoding"; }
		}

		property bool ProgressiveMode
		{
			bool get() { return _value; }
			void set(bool v) { _value = v; }
		}

		virtual BitmapOption^ GetValue()
		{
			auto value = ref new BitmapTypedValue(_value, PropertyType::Boolean);
			return ref new BitmapOption("ProgressiveMode", value);
		}

	private:
		static IVectorView<Format>^ GetFormats();
		static IVectorView<Format>^ _supportedFormats;
		bool _value = true;
	};
	
	public ref class BitmapV5BGRAOption sealed : IBitmapEncodingOption
	{
	public:
		static property IVectorView<Format>^ SupportedFormats
		{
			IVectorView<Format>^ get() { return _supportedFormats; }
		}

		static property String^ Description
		{
			String^ get() {
				return L"Specifies whether to allow encoding data in the 32bpp BGRA pixel format, allowing an alpha channel to be encoded correctly. If this option is set to true, the BMP will be written with a BITMAPV5HEADER header." +
					"\n\nNote for 16-bit and 32-bit Windows BMP files, the BMP codec ignores any alpha channel, as many legacy image files contain invalid data in this extra channel. Starting with Windows 8, 32-bit Windows BMP files written using the BITMAPV5HEADER with valid alpha channel content are read as 32bpp BGRA";
			}
		}

		property bool EnableAlpha
		{
			bool get() { return _value; }
			void set(bool v) { _value = v; }
		}

		virtual BitmapOption^ GetValue()
		{
			auto value = ref new BitmapTypedValue(_value, PropertyType::Boolean);
			return ref new BitmapOption("EnableV5Header32bppBGRA", value);
		}

	private:
		static IVectorView<Format>^ GetFormats();
		static IVectorView<Format>^ _supportedFormats;
		bool _value = false;
	};
	
	public ref class TiffCompressionQualityOption sealed : IBitmapEncodingOption
	{
	public:
		static property IVectorView<Format>^ SupportedFormats
		{
			IVectorView<Format>^ get() { return _supportedFormats; }
		}

		property float CompressionQuality
		{
			float get() { return _value; }
			void set(float v) { _value = v; }
		}

		static property String^ Description
		{
			String^ get() { return L"Higher values indicate a more efficient and slower compression scheme. \nCompression quality does not affect image quality, only the time taken to save the files and the resulting file size."; }
		}

		virtual BitmapOption^ GetValue()
		{
			auto value = ref new BitmapTypedValue(_value, PropertyType::Single);
			return ref new BitmapOption("CompressionQuality", value);
		}

	private:
		static IVectorView<Format>^ GetFormats();
		static IVectorView<Format>^ _supportedFormats;
		float _value = 0.9f;
	};

}}}
