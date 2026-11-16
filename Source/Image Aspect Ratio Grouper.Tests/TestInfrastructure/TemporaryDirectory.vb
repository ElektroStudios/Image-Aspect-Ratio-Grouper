Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.IO

#End Region

Namespace ImageAspectRatioGrouper.Tests.TestInfrastructure

    ''' <summary>
    ''' A unique, empty temporary directory that is deleted (with all its content) when disposed.
    ''' </summary>
    Public NotInheritable Class TemporaryDirectory : Implements IDisposable

#Region " Fields "

        ''' <summary>
        ''' Whether this instance has been disposed.
        ''' </summary>
        Private isDisposed As Boolean

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the full path of the temporary directory.
        ''' </summary>
        Public ReadOnly Property FullPath As String

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="TemporaryDirectory"/> class and creates the directory.
        ''' </summary>
        Public Sub New()
            Me.FullPath = Path.Combine(Path.GetTempPath(), $"IARG-Tests-{Guid.NewGuid():N}")
            Directory.CreateDirectory(Me.FullPath)
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Combines the temporary directory path with the specified relative path segments.
        ''' </summary>
        ''' <param name="segments">The relative path segments.</param>
        ''' <returns>The combined full path.</returns>
        Public Function Combine(ParamArray segments As String()) As String
            Dim allSegments As New List(Of String)()
            allSegments.Add(Me.FullPath)
            allSegments.AddRange(segments)
            Return Path.Combine(allSegments.ToArray())
        End Function

        ''' <summary>
        ''' Deletes the temporary directory and all its content.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose
            If Me.isDisposed Then
                Return
            End If

            Me.isDisposed = True
            If Directory.Exists(Me.FullPath) Then
                Directory.Delete(Me.FullPath, recursive:=True)
            End If
        End Sub

#End Region

    End Class

End Namespace
