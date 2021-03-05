#include "pch.h"
#include "BitmapEncodingOption.h"
#include <concurrent_vector.h>

using namespace ImageConverter::Core::CX;
using namespace Platform;
using namespace Platform::Collections;
using namespace concurrency;
using namespace Windows::Graphics::Imaging;
using namespace Windows::Storage;

IVectorView<Format>^ JpegQualityOption::_supportedFormats = JpegQualityOption::GetFormats();
IVectorView<Format>^ JpegQualityOption::GetFormats()
{
	auto vec = ref new Vector<Format>();
	vec->Append(Format::Jpeg);
	vec->Append(Format::JpegXR);
	vec->Append(Format::Heif);
	return vec->GetView();
}

IVectorView<Format>^ JpegChromaSubsamplingOption::_supportedFormats = JpegChromaSubsamplingOption::GetFormats();
IVectorView<Format>^ JpegChromaSubsamplingOption::GetFormats()
{
	auto vec = ref new Vector<Format>();
	vec->Append(Format::Jpeg);
	return vec->GetView();
}

IVectorView<Format>^ PngFilterModeOption::_supportedFormats = PngFilterModeOption::GetFormats();
IVectorView<Format>^ PngFilterModeOption::GetFormats()
{
	auto vec = ref new Vector<Format>();
	vec->Append(Format::Png);
	return vec->GetView();
}

IVectorView<Format>^ InterlaceOptionOption::_supportedFormats = InterlaceOptionOption::GetFormats();
IVectorView<Format>^ InterlaceOptionOption::GetFormats()
{
	auto vec = ref new Vector<Format>();
	vec->Append(Format::Png);
	return vec->GetView();
}

IVectorView<Format>^ LosslessOption::_supportedFormats = LosslessOption::GetFormats();
IVectorView<Format>^ LosslessOption::GetFormats()
{
	auto vec = ref new Vector<Format>();
	vec->Append(Format::JpegXR);
	return vec->GetView();
}

IVectorView<Format>^ ProgressiveModeOption::_supportedFormats = ProgressiveModeOption::GetFormats();
IVectorView<Format>^ ProgressiveModeOption::GetFormats()
{
	auto vec = ref new Vector<Format>();
	vec->Append(Format::JpegXR);
	return vec->GetView();
}

IVectorView<Format>^ BitmapV5BGRAOption::_supportedFormats = BitmapV5BGRAOption::GetFormats();
IVectorView<Format>^ BitmapV5BGRAOption::GetFormats()
{
	auto vec = ref new Vector<Format>();
	vec->Append(Format::Bmp);
	return vec->GetView();
}

IVectorView<Format>^ TiffCompressionQualityOption::_supportedFormats = TiffCompressionQualityOption::GetFormats();
IVectorView<Format>^ TiffCompressionQualityOption::GetFormats()
{
	auto vec = ref new Vector<Format>();
	vec->Append(Format::Tiff);
	return vec->GetView();
}