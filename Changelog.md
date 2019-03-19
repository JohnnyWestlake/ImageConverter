# Version 1.2.0.0

### New Features:
- Support multiple instances of application
- Allow application to continue converting when application is minimised (if attached to power source or allowed by Windows)

### Updates:
- UI Improvements


# Version 1.1.2.0

### Bug Fixes:
- Fixed startup crash on Windows 10 1709 & 1803 builds (due to missing UI API's)


# Version 1.1.0.0

### New Features:
- You can now choose image quality whilst encoding to HEIC/HEIF!
- You can now apply maximum size contraints to resize the output images whilst maintaining the input aspect ratio.
- You can now open the output folder in windows explorer using the options button next to the folder
- You can now drag files from Windows Explorer and other apps into the app to add them to the conversion list.

### Updates:
- Prevent setting un-writeable system folders as the export folder (like "This PC", "Network", etc).
- Prevent adding un-readable, online-only files - such online-only OneDrive files - to the file list. A warning will be shown if you attempt this.


# Version 1.0.1.0

### New Features:
- Added support for decoding more formats:
    - .ico
    - .icon
    - .cur

For devices on the Windows 10 Fall 2018 Update:
- Added support for decoding even more additional formats:
    - .avci
    - .heic
    - .heif
    - .avcs
    - .webp