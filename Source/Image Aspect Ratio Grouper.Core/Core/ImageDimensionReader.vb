Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.Drawing
Imports System.IO

Imports SkiaSharp

#End Region

Namespace ImageAspectRatioGrouper.Core

    ''' <summary>
    ''' Reads the dimensions of the image files.
    ''' <para></para>
    ''' JPEG, PNG, BMP and TIFF are read with GDI+; WebP, AVIF and ICO (formats that GDI+ cannot read) are read with
    ''' SkiaSharp. In both cases only the image header is read, not the pixels.
    ''' </summary>
    Public NotInheritable Class ImageDimensionReader

#Region " Constants and Read-only Fields "

        ''' <summary>
        ''' The supported (lower-case, dot-prefixed) file extensions.
        ''' </summary>
        Public Shared ReadOnly SupportedExtensions As ReadOnlyCollection(Of String) = Array.AsReadOnly(
            {".jpg", ".jpeg", ".png", ".bmp", ".tif", ".tiff", ".webp", ".avif", ".ico"})

        ''' <summary>
        ''' The file extensions read with SkiaSharp instead of GDI+.
        ''' </summary>
        Private Shared ReadOnly SkiaDecodedExtensions As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {
            ".webp", ".avif", ".ico"
        }

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="ImageDimensionReader"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Verifies that the native SkiaSharp library (<c>libSkiaSharp.dll</c>) can be loaded.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">
        ''' Thrown when the native library is missing or cannot be loaded, with a message meant for the user.
        ''' </exception>
        Public Shared Sub EnsureSkiaSharpIsAvailable()
            Try
                ' Any SkiaSharp object creation goes through the native library.
                Using New SKBitmap(1, 1)
                End Using

            Catch ex As Exception When TypeOf ex Is DllNotFoundException OrElse
                                       TypeOf ex Is BadImageFormatException OrElse
                                       TypeOf ex Is EntryPointNotFoundException OrElse
                                       TypeOf ex Is TypeInitializationException
                Dim architectureFolder As String = If(Environment.Is64BitProcess, "x64", "x86")
                Throw New InvalidOperationException(
                    $"The native SkiaSharp library cannot be loaded, so WebP, AVIF and ICO files cannot be read.{Environment.NewLine}" &
                    $"Make sure that '{architectureFolder}{Path.DirectorySeparatorChar}libSkiaSharp.dll' exists next to the executable.{Environment.NewLine}{Environment.NewLine}" &
                    $"{ex.GetType().Name}: {ex.Message}", ex)
            End Try
        End Sub

#End Region

#Region " Friend Methods "

        ''' <summary>
        ''' Reads the dimensions of an image file.
        ''' </summary>
        ''' <param name="filePath">The image file path.</param>
        ''' <returns>The width and height, in pixels.</returns>
        ''' <exception cref="ArgumentException">
        ''' Thrown when <paramref name="filePath"/> is empty, or GDI+ cannot read a JPEG, PNG, BMP or TIFF file.
        ''' </exception>
        ''' <exception cref="InvalidOperationException">Thrown when SkiaSharp cannot read a WebP, AVIF or ICO file.</exception>
        Friend Shared Function ReadSize(filePath As String) As Size
            If String.IsNullOrWhiteSpace(filePath) Then
                Throw New ArgumentException("The file path cannot be empty.", NameOf(filePath))
            End If

            Return If(ImageDimensionReader.IsSkiaDecoded(filePath),
                      ImageDimensionReader.ReadSizeWithSkia(filePath),
                      ImageDimensionReader.ReadSizeWithGdi(filePath))
        End Function

        ''' <summary>
        ''' Determines whether a file is read with SkiaSharp (WebP, AVIF and ICO) instead of GDI+.
        ''' </summary>
        ''' <param name="filePath">The file path (only its extension is evaluated).</param>
        ''' <returns><see langword="True"/> when the file is read with SkiaSharp; otherwise, <see langword="False"/>.</returns>
        ''' <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> is <see langword="Nothing"/>.</exception>
        Friend Shared Function IsSkiaDecoded(filePath As String) As Boolean
            If filePath Is Nothing Then
                Throw New ArgumentNullException(NameOf(filePath))
            End If

            Return ImageDimensionReader.SkiaDecodedExtensions.Contains(Path.GetExtension(filePath))
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Reads the dimensions of a JPEG, PNG, BMP or TIFF file with GDI+.
        ''' </summary>
        ''' <param name="filePath">The file path.</param>
        ''' <returns>The width and height, in pixels.</returns>
        Private Shared Function ReadSizeWithGdi(filePath As String) As Size
            Using stream As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                ' validateImageData:=False reads only the header (dimensions) instead of decoding the full bitmap.
                Using image As Image = Image.FromStream(stream, useEmbeddedColorManagement:=False, validateImageData:=False)
                    Return New Size(image.Width, image.Height)
                End Using
            End Using
        End Function

        ''' <summary>
        ''' Reads the dimensions of a WebP, AVIF or ICO file with SkiaSharp (for an ICO file, its largest image).
        ''' </summary>
        ''' <param name="filePath">The file path.</param>
        ''' <returns>The width and height, in pixels.</returns>
        ''' <exception cref="InvalidOperationException">Thrown when SkiaSharp cannot read the file.</exception>
        Private Shared Function ReadSizeWithSkia(filePath As String) As Size
            Using stream As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
                Dim codecResult As SKCodecResult
                ' The codec only parses the header: the pixels are never decoded.
                Using codec As SKCodec = SKCodec.Create(stream, codecResult)
                    If codec Is Nothing OrElse codecResult <> SKCodecResult.Success Then
                        Throw New InvalidOperationException($"SkiaSharp cannot read the file (codec result: {codecResult}). The file may be corrupt, or its extension may not match its format.")
                    End If

                    Return New Size(codec.Info.Width, codec.Info.Height)
                End Using
            End Using
        End Function

#End Region

    End Class

End Namespace
