#pragma once
#include <iostream>

using namespace Windows::Graphics::Imaging;
using namespace Windows::Foundation;
using namespace Windows::Storage::Streams;
using namespace Windows::Foundation::Collections;
using namespace Platform;

namespace ImageConverter {
	namespace Core {
		namespace CX {

			/// <summary>
			/// Retrieves information related to installed Microsoft Store codec
			/// extension packages.
			/// </summary>
			public ref class CodecSupport sealed
			{
			public:
				property bool HasHEIFImageExtensions;

				property bool HasWebPImageExtensions;

				property bool HasRAWImageExtensions;

				/// <summary>
				/// Codec ID for Microsoft RAW Image Extension package.
				/// </summary>
				static property Guid RAWImageExtensionsDecoderID { Guid get() { return _rawImageExtensionsDecoderID; } }

				static property Guid DDSDecoderID { Guid get() { return _ddsDecoderID; } }

				static property Guid DDSEncoderID { Guid get() { return _ddsEncoderID; } }



				static property Uri^ HEIFImageExtensionsStoreLink { Uri^ get() { return _heifStoreLink; } }

				static property Uri^ WebPImageExtensionsStoreLink { Uri^ get() { return _webpStoreLink; } }

				static property Uri^ RAWImageExtensionsStoreLink  { Uri^ get() { return _rawStoreLink; } }

				static property Uri^ HEIFImageExtensionsWebLink   { Uri^ get() { return _heifWebLink; } }

				static property Uri^ WebPImageExtensionsWebLink   { Uri^ get() { return _webpWebLink; } }

				static property Uri^ RAWImageExtensionsWebLink    { Uri^ get() { return _rawWebLink; } }

				static CodecSupport^ CreateSummary()
				{
					auto summary = ref new CodecSupport();

					IVectorView<BitmapCodecInformation^>^ items = BitmapDecoder::GetDecoderInformationEnumerator();

					for (int i = 0; i < items->Size; i++)
					{
						auto info = items->GetAt(i);

						if (info->CodecId == BitmapDecoder::HeifDecoderId)
							summary->HasHEIFImageExtensions = true;
						else if (info->CodecId == BitmapDecoder::WebpDecoderId)
							summary->HasWebPImageExtensions = true;
						else if (info->CodecId == RAWImageExtensionsDecoderID)
							summary->HasRAWImageExtensions = true;
					}

					return summary;
				}

			internal:
				CodecSupport() { };

				static Guid _ddsDecoderID;
				static Guid _ddsEncoderID;
				static Guid _rawImageExtensionsDecoderID;


				static Uri^ _heifWebLink;
				static Uri^ _webpWebLink;
				static Uri^ _rawWebLink;

				static Uri^ _heifStoreLink;
				static Uri^ _webpStoreLink;
				static Uri^ _rawStoreLink;
			};

			Uri^ CodecSupport::_heifWebLink = ref new Uri(L"https://www.microsoft.com/store/apps/9pmmsr1cgpwg");
			Uri^ CodecSupport::_heifStoreLink = ref new Uri(L"ms-windows-store://pdp/?ProductId=9pmmsr1cgpwg");

			Uri^ CodecSupport::_webpWebLink = ref new Uri(L"https://www.microsoft.com/store/apps/9pg2dk419drg");
			Uri^ CodecSupport::_webpStoreLink = ref new Uri(L"ms-windows-store://pdp/?ProductId=9pg2dk419drg");

			Uri^ CodecSupport::_rawWebLink = ref new Uri(L"https://www.microsoft.com/store/apps/9nctdw2w1bh8");
			Uri^ CodecSupport::_rawStoreLink = ref new Uri(L"ms-windows-store://pdp/?ProductId=9nctdw2w1bh8"); 

			Guid CodecSupport::_rawImageExtensionsDecoderID = Guid(0x41945702, 0x8302, 0x44a6, 0x94, 0x45, 0xac, 0x98, 0xe8, 0xaf, 0xa0, 0x86);
			Guid CodecSupport::_ddsDecoderID = Guid(0x9053699f, 0xa341, 0x429d, 0x9e, 0x90, 0xee, 0x43, 0x7c, 0xf8, 0x0c, 0x73);
			Guid CodecSupport::_ddsEncoderID = Guid(0xa61dde94, 0x66ce, 0x4ac1, 0x88, 0x1b, 0x71, 0x68, 0x05, 0x88, 0x89, 0x5e);
		}
	}
}

