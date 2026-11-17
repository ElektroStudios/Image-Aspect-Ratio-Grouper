Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports ImageAspectRatioGrouper.CommandLine
Imports ImageAspectRatioGrouper.Core

#End Region

Namespace ImageAspectRatioGrouper.Tests.CommandLine

    ''' <summary>
    ''' Tests for the command-line parser (<see cref="CommandLineParser"/>) and its syntax rules (<see cref="CommandLineSyntax"/>).
    ''' </summary>
    Public NotInheritable Class CommandLineParserTests

#Region " Helpers "

        ''' <summary>
        ''' Parses the arguments.
        ''' </summary>
        ''' <param name="args">The arguments.</param>
        ''' <returns>The parsed arguments.</returns>
        Private Shared Function Parse(ParamArray args As String()) As CommandLineArguments
            Return CommandLineParser.Parse(args)
        End Function

        ''' <summary>
        ''' Asserts that the arguments are rejected as invalid arguments, and returns the error message.
        ''' </summary>
        ''' <param name="args">The arguments.</param>
        ''' <returns>The error message.</returns>
        Private Shared Function AssertRejected(ParamArray args As String()) As String
            Dim exception As CommandLineException = Assert.Throws(Of CommandLineException)(Function() CommandLineParser.Parse(args))
            Assert.Equal(CommandLineExitCode.InvalidArguments, exception.ExitCode)
            Assert.False(String.IsNullOrWhiteSpace(exception.Message))
            Return exception.Message
        End Function

#End Region

#Region " Help "

        ''' <summary>
        ''' No arguments shows the help.
        ''' </summary>
        <Fact>
        Public Sub Parse_NoArguments_ShowsHelp()
            Dim arguments As CommandLineArguments = Parse()

            Assert.True(arguments.ShowHelp)
            Assert.Null(arguments.Directory)
        End Sub

        ''' <summary>
        ''' Every help switch, with any prefix and case, shows the help, even with other (even invalid) arguments.
        ''' </summary>
        ''' <param name="helpSwitch">
        ''' The help switch.
        ''' </param>
        <Theory>
        <InlineData("/?")>
        <InlineData("-?")>
        <InlineData("/h")>
        <InlineData("-H")>
        <InlineData("/help")>
        <InlineData("-help")>
        <InlineData("--help")>
        <InlineData("/HELP")>
        Public Sub Parse_HelpSwitch_ShowsHelp(helpSwitch As String)
            Assert.True(Parse(helpSwitch).ShowHelp)
            Assert.True(Parse("C:\Images", "/tolerance:999", helpSwitch).ShowHelp)
            Assert.True(Parse("/unknown", helpSwitch).ShowHelp)
            Assert.True(Parse("C:\Images", Nothing, helpSwitch).ShowHelp)
        End Sub

#End Region

#Region " Grouping "

        ''' <summary>
        ''' A directory alone groups with the documented defaults (never the preferences of the main window),
        ''' and the directory is kept exactly as written (it is validated later).
        ''' </summary>
        ''' <param name="directory">
        ''' The directory, as written in the command line.
        ''' </param>
        <Theory>
        <InlineData("C:\Images")>
        <InlineData("Relative Folder")>
        <InlineData("C:\Images\""")>
        <InlineData("/data/images")>
        <InlineData("\\server\share\images")>
        Public Sub Parse_Directory_UsesTheDefaults(directory As String)
            Dim arguments As CommandLineArguments = Parse(directory)

            Assert.False(arguments.ShowHelp)
            Assert.Equal(directory, arguments.Directory)
            Assert.False(arguments.SearchRecursively)
            Assert.Same(AspectRatio.Default, arguments.AspectRatio)
            Assert.Equal(ImageGroupingOptions.DefaultTolerancePixels, arguments.TolerancePixels)
            Assert.Equal(ImageGroupingOptions.DefaultMinimumHeight, arguments.MinimumHeight)
        End Sub

        ''' <summary>
        ''' Every option, in any order, before or after the directory, with any prefix, separator and case.
        ''' </summary>
        ''' <param name="a1">The first argument.</param>
        ''' <param name="a2">The second argument.</param>
        ''' <param name="a3">The third argument.</param>
        ''' <param name="a4">The fourth argument.</param>
        ''' <param name="a5">The fifth argument.</param>
        <Theory>
        <InlineData("C:\Images", "/recursive", "/ratio:21:9", "/tolerance:5", "/minheight:720")>
        <InlineData("/tolerance:5", "/minheight:720", "C:\Images", "/ratio:21:9", "/recursive")>
        <InlineData("C:\Images", "-r", "-a:21:9", "-t:5", "-m:720")>
        <InlineData("C:\Images", "--recursive", "--ratio=21:9", "--tolerance=5", "--minheight=720")>
        <InlineData("C:\Images", "/Recursive", "/RATIO: 21:9 ", "/Tolerance=5", "/M: 720 ")>
        Public Sub Parse_AllOptions(a1 As String, a2 As String, a3 As String, a4 As String, a5 As String)
            Dim arguments As CommandLineArguments = Parse(a1, a2, a3, a4, a5)

            Assert.False(arguments.ShowHelp)
            Assert.Equal("C:\Images", arguments.Directory)
            Assert.True(arguments.SearchRecursively)
            Assert.Same(AspectRatio.Ultrawide21x9, arguments.AspectRatio)
            Assert.Equal(5, arguments.TolerancePixels)
            Assert.Equal(720, arguments.MinimumHeight)
        End Sub

        ''' <summary>
        ''' Every standard aspect ratio is accepted by its key.
        ''' </summary>
        <Fact>
        Public Sub Parse_AcceptsEveryStandardRatio()
            For Each ratio As AspectRatio In AspectRatio.StandardRatios
                Assert.Same(ratio, Parse("x", $"/ratio:{ratio.Key}").AspectRatio)
            Next
        End Sub

        ''' <summary>
        ''' The limits of every range are accepted.
        ''' </summary>
        <Fact>
        Public Sub Parse_AcceptsTheLimits()
            Dim lowest As CommandLineArguments = Parse("x", $"/tolerance:{ImageGroupingOptions.MinimumTolerancePixels}", $"/minheight:{ImageGroupingOptions.MinimumHeightLowerBound}")
            Dim highest As CommandLineArguments = Parse("x", $"/tolerance:{ImageGroupingOptions.MaximumTolerancePixels}", $"/minheight:{ImageGroupingOptions.MinimumHeightUpperBound}")

            Assert.Equal(ImageGroupingOptions.MinimumTolerancePixels, lowest.TolerancePixels)
            Assert.Equal(ImageGroupingOptions.MinimumHeightLowerBound, lowest.MinimumHeight)
            Assert.Equal(ImageGroupingOptions.MaximumTolerancePixels, highest.TolerancePixels)
            Assert.Equal(ImageGroupingOptions.MinimumHeightUpperBound, highest.MinimumHeight)
        End Sub

#End Region

#Region " Invalid arguments "

        ''' <summary>
        ''' Unknown switches are rejected and named in the message.
        ''' </summary>
        ''' <param name="unknownSwitch">
        ''' The unknown switch.
        ''' </param>
        <Theory>
        <InlineData("/unknown")>
        <InlineData("/ratios:16:9")>
        <InlineData("/group")>
        <InlineData("--")>
        <InlineData("-x")>
        Public Sub Parse_UnknownSwitch_IsRejected(unknownSwitch As String)
            Dim message As String = AssertRejected("C:\Images", unknownSwitch)
            Assert.Contains(unknownSwitch, message, StringComparison.Ordinal)
        End Sub

        ''' <summary>
        ''' Regression: a directory glued to a switch (<c>/r"C:\Images"</c> arrives as <c>/rC:\Images</c>) is an unknown
        ''' switch with a hint, never a directory (which would fail later with a confusing message).
        ''' </summary>
        ''' <param name="argument">
        ''' The switch glued to a directory.
        ''' </param>
        <Theory>
        <InlineData("/rC:\Images")>
        <InlineData("/recursiveC:\Users\Administrador\Images")>
        <InlineData("-mC:/Images")>
        Public Sub Parse_DirectoryGluedToSwitch_IsRejectedWithAHint(argument As String)
            Dim message As String = AssertRejected(argument)
            Assert.Contains(argument, message, StringComparison.Ordinal)
            Assert.Contains("separate", message, StringComparison.Ordinal)
        End Sub

        ''' <summary>
        ''' Missing, unknown, non-integer and out-of-range values are rejected.
        ''' </summary>
        ''' <param name="invalidSwitch">
        ''' The switch with an invalid value.
        ''' </param>
        <Theory>
        <InlineData("/ratio")>
        <InlineData("/ratio:")>
        <InlineData("/ratio=")>
        <InlineData("/ratio:16x9")>
        <InlineData("/ratio:16/9")>
        <InlineData("/ratio:17:9")>
        <InlineData("/ratio:16:9:1")>
        <InlineData("/tolerance")>
        <InlineData("/tolerance: ")>
        <InlineData("/tolerance:abc")>
        <InlineData("/tolerance:2.5")>
        <InlineData("/tolerance:-1")>
        <InlineData("/tolerance:101")>
        <InlineData("/tolerance:0x5")>
        <InlineData("/tolerance:99999999999999999999")>
        <InlineData("/minheight:-1")>
        <InlineData("/minheight:10001")>
        <InlineData("/minheight:1e3")>
        Public Sub Parse_InvalidValue_IsRejected(invalidSwitch As String)
            AssertRejected("C:\Images", invalidSwitch)
        End Sub

        ''' <summary>
        ''' The error messages show the allowed range, or the valid aspect ratios.
        ''' </summary>
        <Fact>
        Public Sub Parse_InvalidValue_MessageShowsTheValidValues()
            Dim toleranceMessage As String = AssertRejected("C:\Images", "/tolerance:101")
            Assert.Contains($"{ImageGroupingOptions.MinimumTolerancePixels} to {ImageGroupingOptions.MaximumTolerancePixels}", toleranceMessage, StringComparison.Ordinal)

            Dim ratioMessage As String = AssertRejected("C:\Images", "/ratio:16x9")
            Assert.All(AspectRatio.StandardRatios, Sub(ratio As AspectRatio) Assert.Contains(ratio.Key, ratioMessage, StringComparison.Ordinal))
        End Sub

        ''' <summary>
        ''' Switches that do not take a value reject one.
        ''' </summary>
        ''' <param name="switchWithValue">
        ''' The switch with an unexpected value.
        ''' </param>
        <Theory>
        <InlineData("/recursive:yes")>
        <InlineData("/r=1")>
        Public Sub Parse_UnexpectedValue_IsRejected(switchWithValue As String)
            AssertRejected("C:\Images", switchWithValue)
        End Sub

        ''' <summary>
        ''' A repeated switch is rejected, even when written with an alias or another case.
        ''' </summary>
        ''' <param name="first">The first switch.</param>
        ''' <param name="second">The repeated switch.</param>
        <Theory>
        <InlineData("/tolerance:5", "/tolerance:5")>
        <InlineData("/ratio:16:9", "/a:4:3")>
        <InlineData("/recursive", "-R")>
        Public Sub Parse_RepeatedSwitch_IsRejected(first As String, second As String)
            AssertRejected("C:\Images", first, second)
        End Sub

        ''' <summary>
        ''' Options without a directory are rejected.
        ''' </summary>
        <Fact>
        Public Sub Parse_WithoutDirectory_IsRejected()
            Assert.Contains("missing", AssertRejected("/r", "/tolerance:5"), StringComparison.Ordinal)
        End Sub

        ''' <summary>
        ''' Several directories are rejected, with a hint about quoting paths that contain spaces.
        ''' </summary>
        <Fact>
        Public Sub Parse_SeveralDirectories_AreRejected()
            Assert.Contains("quotes", AssertRejected("C:\My", "Images"), StringComparison.Ordinal)
        End Sub

        ''' <summary>
        ''' A null argument array fails fast; a null argument is rejected.
        ''' </summary>
        <Fact>
        Public Sub Parse_NullArguments()
            Assert.Throws(Of ArgumentNullException)(Function() CommandLineParser.Parse(Nothing))
            AssertRejected("C:\Images", Nothing)
        End Sub

#End Region

#Region " Syntax "

        ''' <summary>
        ''' Switches are told apart from paths.
        ''' </summary>
        ''' <param name="argument">The argument.</param>
        ''' <param name="expected">Whether the argument is a switch.</param>
        <Theory>
        <InlineData("/r", True)>
        <InlineData("-r", True)>
        <InlineData("--recursive", True)>
        <InlineData("/tolerance:5", True)>
        <InlineData("/ratio:16:9", True)>
        <InlineData("/?", True)>
        <InlineData("/rC:\Images", True)>
        <InlineData("C:\Images", False)>
        <InlineData("/data/images", False)>
        <InlineData("/tmp", True)>
        <InlineData("-", False)>
        <InlineData("/", False)>
        <InlineData("", False)>
        <InlineData(Nothing, False)>
        <InlineData("Images", False)>
        Public Sub IsSwitch(argument As String, expected As Boolean)
            Assert.Equal(expected, CommandLineSyntax.IsSwitch(argument))
        End Sub

        ''' <summary>
        ''' Every canonical switch has a one-letter alias, and every alias maps to a canonical switch.
        ''' </summary>
        <Fact>
        Public Sub SwitchAliases_AreConsistent()
            Dim canonicalNames As String() = {CommandLineSyntax.RecursiveSwitch, CommandLineSyntax.RatioSwitch,
                                              CommandLineSyntax.ToleranceSwitch, CommandLineSyntax.MinimumHeightSwitch}

            For Each name As String In canonicalNames
                Assert.Equal(name, CommandLineSyntax.SwitchAliases(name))
                Assert.Single(CommandLineSyntax.SwitchAliases, Function(pair As KeyValuePair(Of String, String)) pair.Value = name AndAlso pair.Key.Length = 1)
            Next

            Assert.Equal(canonicalNames.Length * 2, CommandLineSyntax.SwitchAliases.Count)
            Assert.All(CommandLineSyntax.SwitchAliases.Values, Sub(value As String) Assert.Contains(value, canonicalNames))
            Assert.Throws(Of ArgumentException)(Function() CommandLineSyntax.RemovePrefix("C:\Images"))
        End Sub

#End Region

    End Class

End Namespace
