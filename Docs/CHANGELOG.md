# Image Aspect Ratio Grouper Change Log 📋

## v1.1.0 🔄
- Added command-line support, through the new console application `Image Aspect Ratio Grouper.CLI.exe`:
  - `"Image Aspect Ratio Grouper.CLI.exe" <directory> [options]` groups the images, with the options `/recursive`, `/ratio:<ratio>`, `/tolerance:<n>` and `/minheight:<n>`.
  - Every image and the final summary are written to the console, and Ctrl+C cancels the operation.
  - Documented exit codes, so it can be used in scripts.
  - `/?` (or no arguments) shows the help, which also explains how the program works.
- The main window accepts a directory as its command-line argument, and selects it on startup. An invalid argument is reported in its console.
- Added the Help > Command-Line Reference menu item.
- Added support for WebP, AVIF and ICO files, read with SkiaSharp (the native library is verified on startup).
- The shared logic now lives in the new `Image Aspect Ratio Grouper.Core.dll` library, used by both applications.
- The final summary of the main window and of the command line share the same format.

## v1.0.1 🔄
- Fixed: the captions of the toggle switches ("Include subdirectories" and the "Dark mode" / "Light mode" menu bar toggle) were not vertically centered on the switch.
- Improved: the toggle switches now draw their own caption and scale with the font and the DPI. Clicking the caption toggles the switch, as before.

## v1.0 🔄
Initial Release.
