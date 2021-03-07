# Image Converter 

[Download from the Microsoft Store](https://www.microsoft.com/en-us/p/image-converter/9pgn31qtzq26)

A quick, simple and lightweight batch image converter powered by Windows, supporting convertering from BMP, JPEG, JPEG-XR, GIF, TIFF, DDS, PNG, WEBP, HEIF, RAW, ICO, CUR and more to BMP, JPEG, JPEG-XR, GIF, TIFF, DDS, PNG, HEIF.

Image Converter also supports converting a large number of digital camera RAW formats into any of the other supported formats. You can also extend the amount of formats that can be read by installing additional WIC Codecs on your Windows 10 machine.

The Windows 10 Fall 2018 Update adds experiemental support for HEIF encoding and decoding. Lots of photos from your new iPhone? No problem.

With the encoding engine written in C++, the converter opens fast, uses a minimal amount of RAM, and processes quickly, using Microsoft's native Windows Imaging Component encoders. And with zero network access everything works offline and there are no privacy worries.

Simple choose your output folder, add your files, choose your output format (and options), then go ahead and convert.

You can now also specify output height & width constraints to downsize your encoded images whilst maintaining their original aspect ratios.

Full supported decoding format list*:
.bmp
.dib
.rle
.gif
.ico
.icon
.cur
.jpeg
.jpe
.jpg
.jfif
.exif
.png
.tiff
.tif
.dng
.wdp
.jxr
.dds
.heic
.heif
.avci
.heics
.heifs
.avcs
.webp
.ARW
.CR2
.CRW
.ERF
.KDC
.MRW
.NEF
.NRW
.ORF
.PEF
.RAF
.RAW
.RW2
.RWL
.SR2
.SRW
.DNG

(*some formats may only be supported on the most recent Windows 10 Update)


## Using as a library

The `ImageConverter.Core.CX` project contains all of the conversion logic as a Windows Runtime Component that can be used in UWP applications of any language as a library. Below is C#  example of converting a file to JPEG using the library, using the file name `MyConvertedFile.jpg` for the output file.

````
Task<BitmapConversionResult> ConvertAsync(StorageFile input, StorageFolder output)
{
    /* Converts an image to jpg 95% quality with 4:4:4 chroma subsampling */
    var settings = new BitmapConversionSettings
    {
        CollisionOption = CreationCollisionOption.ReplaceExisting,
        EncoderId = BitmapEncoder.JpegEncoderId,
        FileExtension = ".jpg",
        TargetFileName = "MyConvertedFile",
        Options = new List<BitmapOption>
        {
            new BitmapOption("ImageQuality", new BitmapTypedValue(0.95f, PropertyType.Single )),
            new JpegChromaSubsamplingOption { JpegYCrCbSubsampling = JpegSubsamplingMode.Y4Cb4Cr4 }.GetValue()
        }
    };

    return BitmapEncoderFactory.EncodeAsync(input, output, settings).AsTask();
}
````

Leaving `TargetFileName` as null will create the output file with the same file name (minus file extension) as the input file. 

Example bitmap options can be found in `ImageConverter.Core.CX.BitmapEncodingOption.h`. These map to WIC properties, more of which can be found in offical Microsoft documentation. This file contains strongly typed BitmapOptions so you don't need to know the correct WIC property names and formats. For example,

```
/*
 * Two ways to create a 95% quality JPEG option in C# 
 */

// Strongly-typed class from BitmapEncodingOption.h
BitmapOption op1 = new JpegQualityOption { ImageQuality = 0.95f }.GetValue(); 

// Loosely-typed class using native WinRT BitmapOption class directly
BitmapOption op2 = new BitmapOption("ImageQuality", new BitmapTypedValue(0.95f, PropertyType.Single ));
```

Both techniques create the same `BitmapOption` class.