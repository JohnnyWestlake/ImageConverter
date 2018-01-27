#include "pch.h"
#include "BitmapOption.h"

using namespace Platform;
using namespace ImageConverter::Core::CX;

BitmapOption::BitmapOption(Platform::String^ name, BitmapTypedValue^ bitmapValue)
{
	this->Value = bitmapValue;
	this->Name = name;
}