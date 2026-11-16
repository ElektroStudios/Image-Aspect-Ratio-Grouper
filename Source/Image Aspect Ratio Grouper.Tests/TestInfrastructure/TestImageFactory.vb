Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO

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
        ''' Creates a real image file with the specified dimensions. The format is chosen from the file extension.
        ''' </summary>
        ''' <param name="filePath">The full path of the image file to create. Its directory is created when needed.</param>
        ''' <param name="width">The image width, in pixels.</param>
        ''' <param name="height">The image height, in pixels.</param>
        ''' <returns><paramref name="filePath"/>, for fluent usage.</returns>
        ''' <exception cref="ArgumentException">Thrown when the extension is not a supported image format.</exception>
        Public Shared Function CreateImage(filePath As String, width As Integer, height As Integer) As String
            Directory.CreateDirectory(Path.GetDirectoryName(filePath))

            Using bitmap As New Bitmap(width, height, PixelFormat.Format24bppRgb)
                Using g As Graphics = Graphics.FromImage(bitmap)
                    g.Clear(Color.SteelBlue)
                End Using

                bitmap.Save(filePath, TestImageFactory.GetFormat(filePath))
            End Using

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
