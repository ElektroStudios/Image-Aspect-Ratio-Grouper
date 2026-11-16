Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageAspectRatioGrouper.Tests.TestInfrastructure

    ''' <summary>
    ''' Describes the environment the tests run in, so that tests which depend on the native
    ''' Windows Forms implementation of .NET Framework (e.g. internal child controls of <c>NumericUpDown</c>,
    ''' <c>uxtheme.dll</c>, <c>dwmapi.dll</c>) are skipped elsewhere (e.g. under Mono) instead of failing.
    ''' </summary>
    Public NotInheritable Class TestEnvironment

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="TestEnvironment"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets a value indicating whether the tests run on the Mono runtime.
        ''' </summary>
        Public Shared ReadOnly Property IsMono As Boolean
            Get
                Return Type.GetType("Mono.Runtime") IsNot Nothing
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether the tests run on .NET Framework on Windows
        ''' (the real target of the application).
        ''' </summary>
        Public Shared ReadOnly Property IsWindowsDesktopFramework As Boolean
            Get
                Return Environment.OSVersion.Platform = PlatformID.Win32NT AndAlso Not TestEnvironment.IsMono
            End Get
        End Property

        ''' <summary>
        ''' Gets a value indicating whether the tests run on a Unix platform (e.g. Linux under Mono).
        ''' </summary>
        Public Shared ReadOnly Property IsUnix As Boolean
            Get
                Return Environment.OSVersion.Platform = PlatformID.Unix
            End Get
        End Property

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Creates a directory symbolic link (Unix only), using the system <c>ln</c> tool.
        ''' </summary>
        ''' <param name="linkPath">The path of the link to create.</param>
        ''' <param name="targetPath">The directory the link points to.</param>
        ''' <exception cref="PlatformNotSupportedException">Thrown when not running on Unix.</exception>
        ''' <exception cref="InvalidOperationException">Thrown when <c>ln</c> fails.</exception>
        Public Shared Sub CreateDirectorySymbolicLink(linkPath As String, targetPath As String)
            If Not TestEnvironment.IsUnix Then
                Throw New PlatformNotSupportedException("Directory symbolic links are only created on Unix by the tests.")
            End If

            Dim startInfo As New Diagnostics.ProcessStartInfo("ln", $"-s ""{targetPath}"" ""{linkPath}""") With {
                .UseShellExecute = False,
                .CreateNoWindow = True
            }

            Using process As Diagnostics.Process = Diagnostics.Process.Start(startInfo)
                process.WaitForExit()
                If process.ExitCode <> 0 Then
                    Throw New InvalidOperationException($"'ln' failed with exit code {process.ExitCode}.")
                End If
            End Using
        End Sub

#End Region

#Region " Constants "

        ''' <summary>
        ''' The skip reason used by the tests that require <see cref="IsWindowsDesktopFramework"/>.
        ''' </summary>
        Public Const WindowsOnlyReason As String = "Requires the native Windows Forms implementation of .NET Framework on Windows."

#End Region

    End Class

End Namespace
