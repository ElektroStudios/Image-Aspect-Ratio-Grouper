Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Diagnostics
Imports System.IO
Imports System.Text
Imports System.Threading.Tasks

Imports ImageAspectRatioGrouper.CommandLine
Imports ImageAspectRatioGrouper.Core
Imports ImageAspectRatioGrouper.Infrastructure
Imports ImageAspectRatioGrouper.Tests.TestInfrastructure

#End Region

Namespace ImageAspectRatioGrouper.Tests.CommandLine

    ''' <summary>
    ''' Runs the real command-line executable in its own process, and checks what a user sees in the console:
    ''' the standard output, the standard error output and the exit code.
    ''' </summary>
    Public NotInheritable Class CommandLineProcessTests

#Region " Helpers "

        ''' <summary>
        ''' The result of running the executable.
        ''' </summary>
        Private NotInheritable Class ProcessResult

            ''' <summary>
            ''' Gets or sets the exit code.
            ''' </summary>
            Public Property ExitCode As Integer

            ''' <summary>
            ''' Gets or sets the standard output lines.
            ''' </summary>
            Public Property Output As String()

            ''' <summary>
            ''' Gets or sets the standard error output.
            ''' </summary>
            Public Property [Error] As String

        End Class

        ''' <summary>
        ''' Runs the command-line executable (copied next to the test assembly).
        ''' </summary>
        ''' <param name="workingDirectory">The current directory of the process.</param>
        ''' <param name="args">The arguments.</param>
        ''' <returns>The result.</returns>
        Private Shared Function RunExecutable(workingDirectory As String, ParamArray args As String()) As ProcessResult
            Return RunExecutable(AppDomain.CurrentDomain.BaseDirectory, workingDirectory, args)
        End Function

        ''' <summary>
        ''' Runs the command-line executable of the specified directory.
        ''' </summary>
        ''' <param name="executableDirectory">The directory that contains the executable.</param>
        ''' <param name="workingDirectory">The current directory of the process.</param>
        ''' <param name="args">The arguments.</param>
        ''' <returns>The result.</returns>
        Private Shared Function RunExecutable(executableDirectory As String, workingDirectory As String, args As String()) As ProcessResult
            Dim executablePath As String = Path.Combine(executableDirectory, ProductInfo.CliExecutableName)
            Assert.True(File.Exists(executablePath), $"The executable was not found: {executablePath}")

            Dim arguments As String = String.Join(" ", args.Select(Function(argument As String) If(argument.Contains(" "), $"""{argument}""", argument)))
            Dim startInfo As ProcessStartInfo =
                If(TestEnvironment.IsMono,
                   New ProcessStartInfo("mono", $"""{executablePath}"" {arguments}"),
                   New ProcessStartInfo(executablePath, arguments))

            startInfo.UseShellExecute = False
            startInfo.CreateNoWindow = True
            startInfo.RedirectStandardOutput = True
            startInfo.RedirectStandardError = True
            startInfo.StandardOutputEncoding = Encoding.UTF8
            startInfo.StandardErrorEncoding = Encoding.UTF8
            startInfo.WorkingDirectory = workingDirectory

            Using process As Process = Process.Start(startInfo)
                Dim errorTask As Task(Of String) = process.StandardError.ReadToEndAsync()
                Dim output As String = process.StandardOutput.ReadToEnd()
                Assert.True(process.WaitForExit(120000), "The executable did not finish in time.")

                Return New ProcessResult() With {
                    .ExitCode = process.ExitCode,
                    .Output = output.Split({Environment.NewLine}, StringSplitOptions.None),
                    .Error = errorTask.GetAwaiter().GetResult()
                }
            End Using
        End Function

        ''' <summary>
        ''' Asserts that the output starts with the title, version and architecture, followed by the repository address.
        ''' </summary>
        ''' <param name="result">The result.</param>
        Private Shared Sub AssertHeader(result As ProcessResult)
            Dim version As Version = GetType(CommandLineApplication).Assembly.GetName().Version
            Assert.Equal($"Image Aspect Ratio Grouper CLI v{version} ({UtilApplication.ProcessArchitecture.ToString().ToUpperInvariant()})", result.Output(0))
            Assert.Equal(ProductInfo.RepositoryUrl, result.Output(1))
        End Sub

#End Region

#Region " Tests "

        ''' <summary>
        ''' The executable has the documented file name, and the main window too.
        ''' </summary>
        <Fact>
        Public Sub ExecutableNames_MatchTheDocumentation()
            Assert.Equal(ProductInfo.CliExecutableName, Path.GetFileName(GetType(CommandLineApplication).Assembly.Location))
            Assert.Equal(ProductInfo.GuiExecutableName, Path.GetFileName(GetType(Global.ImageAspectRatioGrouper.Forms.MainForm).Assembly.Location))
        End Sub

        ''' <summary>
        ''' Without arguments, the header and the help are written to the standard output, with exit code 0.
        ''' </summary>
        <Fact>
        Public Sub NoArguments_WritesTheHeaderAndTheHelp()
            Dim result As ProcessResult = RunExecutable(Path.GetTempPath())

            Assert.Equal(CInt(CommandLineExitCode.Success), result.ExitCode)
            AssertHeader(result)
            Assert.Contains("USAGE:", result.Output)
            Assert.Contains("EXIT CODES:", result.Output)
            Assert.Equal(String.Empty, result.Error)
        End Sub

        ''' <summary>
        ''' Invalid arguments: the header on the standard output, the error on the standard error output, exit code 2.
        ''' </summary>
        <Fact>
        Public Sub InvalidArguments_WritesTheErrorAndExitCode2()
            Dim result As ProcessResult = RunExecutable(Path.GetTempPath(), "/bogus")

            Assert.Equal(CInt(CommandLineExitCode.InvalidArguments), result.ExitCode)
            AssertHeader(result)
            Assert.Contains("[ERROR] Unknown switch: /bogus", result.Error, StringComparison.Ordinal)
        End Sub

        ''' <summary>
        ''' A missing directory: the error on the standard error output, exit code 3.
        ''' </summary>
        <Fact>
        Public Sub MissingDirectory_WritesTheErrorAndExitCode3()
            Using temp As New TemporaryDirectory()
                Dim result As ProcessResult = RunExecutable(temp.FullPath, "missing folder")

                Assert.Equal(CInt(CommandLineExitCode.InvalidDirectory), result.ExitCode)
                AssertHeader(result)
                Assert.Contains("[ERROR] The directory does not exist:", result.Error, StringComparison.Ordinal)
            End Using
        End Sub

        ''' <summary>
        ''' Grouping: every image and the summary are written to the standard output, and the images are moved.
        ''' </summary>
        <Fact>
        Public Sub Group_WritesTheProgressAndTheSummary()
            Using temp As New TemporaryDirectory()
                Dim directoryPath As String = temp.Combine("My Images")
                Dim alreadyFolder As String = ImageGroupingOptions.GetAlreadyConformingFolderName(AspectRatio.Default)
                TestImageFactory.CreateImage(Path.Combine(directoryPath, "already.png"), 1920, 1080)
                TestImageFactory.CreateImage(Path.Combine(directoryPath, "tall.png"), 1990, 1323)

                Dim result As ProcessResult = RunExecutable(temp.FullPath, "My Images", "/ratio:16:9")

                Assert.Equal(CInt(CommandLineExitCode.Success), result.ExitCode)
                AssertHeader(result)
                Assert.Contains(result.Output, Function(line As String) line.StartsWith("[START] ", StringComparison.Ordinal))
                Assert.Contains(result.Output, Function(line As String) line.Contains("'tall.png' (1990x1323) -> 1118"))
                ' Regression: the folder names use U+2215 (and "±" in the options line): the output is UTF-8, never "?".
                Assert.Contains(result.Output, Function(line As String) line.Contains($"'already.png' (1920x1080) -> {alreadyFolder}"))
                Assert.Contains(result.Output, Function(line As String) line.Contains("Tolerance: ±3 px"))
                Assert.Contains("[SUCCESS] Operation completed. Processed 2 image(s).", result.Output)
                Assert.Equal(String.Empty, result.Error)
                Assert.True(File.Exists(CommandLineApplicationTests.GroupedPath(directoryPath, alreadyFolder, "already.png")))
                Assert.True(File.Exists(CommandLineApplicationTests.GroupedPath(directoryPath, "1118", "tall.png")))
            End Using
        End Sub

        ''' <summary>
        ''' Without the native SkiaSharp library next to the executable, the grouping stops before moving anything,
        ''' with the error on the standard error output and exit code 6.
        ''' </summary>
        <Fact>
        Public Sub MissingNativeLibrary_WritesTheErrorAndExitCode6()
            Using temp As New TemporaryDirectory()
                ' A copy of the application without the native library (libSkiaSharp.*, also in the architecture subfolders).
                Dim applicationDirectory As String = temp.Combine("app")
                Directory.CreateDirectory(applicationDirectory)
                For Each filePath As String In Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory)
                    Dim fileName As String = Path.GetFileName(filePath)
                    Dim extension As String = Path.GetExtension(fileName).ToLowerInvariant()
                    If Not fileName.StartsWith("libSkiaSharp", StringComparison.OrdinalIgnoreCase) AndAlso
                       (extension = ".dll" OrElse extension = ".exe" OrElse extension = ".config") Then
                        File.Copy(filePath, Path.Combine(applicationDirectory, fileName))
                    End If
                Next

                Dim image As String = TestImageFactory.CreateImage(temp.Combine("images", "already.png"), 1920, 1080)

                Dim result As ProcessResult = RunExecutable(applicationDirectory, temp.FullPath, {"images"})

                Assert.Equal(CInt(CommandLineExitCode.MissingComponent), result.ExitCode)
                AssertHeader(result)
                Assert.Contains("[ERROR] The native SkiaSharp library cannot be loaded", result.Error, StringComparison.Ordinal)
                Assert.True(File.Exists(image))
            End Using
        End Sub

#End Region

    End Class

End Namespace
