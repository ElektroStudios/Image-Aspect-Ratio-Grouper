Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO

Imports SkiaSharp

#End Region

Namespace ImageAspectRatioGrouper.Tests.TestInfrastructure

    ''' <summary>
    ''' Creates real image files (and deliberately invalid ones) for the integration tests.
    ''' </summary>
    Public NotInheritable Class TestImageFactory

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="TestImageFactory"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Creates a real image file with the specified dimensions. The format is chosen from the file extension
        ''' (PNG, BMP, TIFF and JPEG through GDI+; lossless WebP through SkiaSharp; ICO with an embedded PNG).
        ''' </summary>
        ''' <param name="filePath">The full path of the image file to create. Its directory is created when needed.</param>
        ''' <param name="width">The image width, in pixels.</param>
        ''' <param name="height">The image height, in pixels.</param>
        ''' <returns><paramref name="filePath"/>, for fluent usage.</returns>
        ''' <exception cref="ArgumentException">Thrown when the extension is not a supported image format.</exception>
        Public Shared Function CreateImage(filePath As String, width As Integer, height As Integer) As String
            Directory.CreateDirectory(Path.GetDirectoryName(filePath))

            Select Case Path.GetExtension(filePath).ToLowerInvariant()
                Case ".webp"
                    File.WriteAllBytes(filePath, TestImageFactory.EncodeLosslessWebP(width, height))
                Case ".ico"
                    File.WriteAllBytes(filePath, TestImageFactory.EncodeIcoWithPng(width, height))
                Case Else
                    Using bitmap As Bitmap = TestImageFactory.CreateBitmap(width, height)
                        bitmap.Save(filePath, TestImageFactory.GetFormat(filePath))
                    End Using
            End Select

            Return filePath
        End Function

        ''' <summary>
        ''' Creates a file that has an image extension but whose content is not a valid image.
        ''' </summary>
        ''' <param name="filePath">The full path of the file to create. Its directory is created when needed.</param>
        ''' <returns><paramref name="filePath"/>, for fluent usage.</returns>
        Public Shared Function CreateCorruptImage(filePath As String) As String
            Directory.CreateDirectory(Path.GetDirectoryName(filePath))
            File.WriteAllText(filePath, "This is definitely not an image.")
            Return filePath
        End Function

        ''' <summary>
        ''' Creates a non-image file (with any content).
        ''' </summary>
        ''' <param name="filePath">The full path of the file to create. Its directory is created when needed.</param>
        ''' <returns><paramref name="filePath"/>, for fluent usage.</returns>
        Public Shared Function CreateTextFile(filePath As String) As String
            Directory.CreateDirectory(Path.GetDirectoryName(filePath))
            File.WriteAllText(filePath, "text")
            Return filePath
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Creates a solid bitmap.
        ''' </summary>
        ''' <param name="width">The width.</param>
        ''' <param name="height">The height.</param>
        ''' <returns>The bitmap. The caller must dispose it.</returns>
        Private Shared Function CreateBitmap(width As Integer, height As Integer) As Bitmap
            Dim bitmap As New Bitmap(width, height, PixelFormat.Format24bppRgb)
            Try
                Using g As Graphics = Graphics.FromImage(bitmap)
                    g.Clear(Color.SteelBlue)
                End Using

                Return bitmap

            Catch
                bitmap.Dispose()
                Throw
            End Try
        End Function

        ''' <summary>
        ''' Encodes a solid lossless WebP image with SkiaSharp (GDI+ cannot write WebP).
        ''' </summary>
        ''' <param name="width">The width.</param>
        ''' <param name="height">The height.</param>
        ''' <returns>The WebP file bytes.</returns>
        Private Shared Function EncodeLosslessWebP(width As Integer, height As Integer) As Byte()
            Dim info As New SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul)
            Using bitmap As New SKBitmap(info)
                bitmap.Erase(New SKColor(Color.SteelBlue.R, Color.SteelBlue.G, Color.SteelBlue.B))

                Using pixmap As SKPixmap = bitmap.PeekPixels(),
                      data As SKData = pixmap.Encode(New SKWebpEncoderOptions(SKWebpEncoderCompression.Lossless, 100.0F))

                    If data Is Nothing Then
                        Throw New InvalidOperationException("SkiaSharp could not encode the WebP test image.")
                    End If

                    Return data.ToArray()
                End Using
            End Using
        End Function

        ''' <summary>
        ''' Encodes an ICO file with a single embedded PNG image (the format of the modern, large icons).
        ''' </summary>
        ''' <param name="width">The width.</param>
        ''' <param name="height">The height.</param>
        ''' <returns>The ICO file bytes.</returns>
        Private Shared Function EncodeIcoWithPng(width As Integer, height As Integer) As Byte()
            Dim pngBytes As Byte()
            Using bitmap As Bitmap = TestImageFactory.CreateBitmap(width, height),
                  pngStream As New MemoryStream()

                bitmap.Save(pngStream, ImageFormat.Png)
                pngBytes = pngStream.ToArray()
            End Using

            Using stream As New MemoryStream(),
                  writer As New BinaryWriter(stream)

                ' ICONDIR.
                writer.Write(CUShort(0))     ' Reserved.
                writer.Write(CUShort(1))     ' Type: icon.
                writer.Write(CUShort(1))     ' Image count.

                ' ICONDIRENTRY (0 means 256 or more).
                writer.Write(CByte(If(width >= 256, 0, width)))
                writer.Write(CByte(If(height >= 256, 0, height)))
                writer.Write(CByte(0))       ' Palette colors.
                writer.Write(CByte(0))       ' Reserved.
                writer.Write(CUShort(1))     ' Color planes.
                writer.Write(CUShort(32))    ' Bits per pixel.
                writer.Write(CUInt(pngBytes.Length))
                writer.Write(CUInt(6 + 16))  ' Offset of the image data.

                writer.Write(pngBytes)
                writer.Flush()
                Return stream.ToArray()
            End Using
        End Function

        ''' <summary>
        ''' Gets the GDI+ image format that matches the extension of the specified file.
        ''' </summary>
        ''' <param name="filePath">The file path.</param>
        ''' <returns>The image format.</returns>
        ''' <exception cref="ArgumentException">Thrown when the extension is not a supported image format.</exception>
        Private Shared Function GetFormat(filePath As String) As ImageFormat
            Select Case Path.GetExtension(filePath).ToLowerInvariant()
                Case ".png"
                    Return ImageFormat.Png
                Case ".jpg", ".jpeg"
                    Return ImageFormat.Jpeg
                Case ".bmp"
                    Return ImageFormat.Bmp
                Case ".tif", ".tiff"
                    Return ImageFormat.Tiff
                Case Else
                    Throw New ArgumentException($"Unsupported test image extension: {filePath}", NameOf(filePath))
            End Select
        End Function

#End Region

    End Class

End Namespace
