#include "pch.h"
#include "BitmapOption.h"

using namespace ImageConverter_Core_CX;
using namespace Platform;

BitmapOption::BitmapOption(Platform::String^ name, BitmapTypedValue^ bitmapValue)
{
	this->Value = bitmapValue;
	this->Name = name;
}