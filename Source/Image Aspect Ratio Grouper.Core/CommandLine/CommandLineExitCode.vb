Option Strict On
Option Explicit On
Option Infer Off

Namespace ImageAspectRatioGrouper.CommandLine

    ''' <summary>
    ''' The process exit codes of the command-line mode.
    ''' <para></para>
    ''' The values are part of the public command-line contract (scripts depend on them): never renumber them.
    ''' </summary>
    Public Enum CommandLineExitCode As Integer

        ''' <summary>
        ''' The operation completed: every image was moved (or was already in its folder, or there was no image), or the help was shown.
        ''' </summary>
        Success = 0

        ''' <summary>
        ''' The operation completed, but some images failed (unreadable or locked files, or name conflicts) and stayed in place.
        ''' </summary>
        CompletedWithProblems = 1

        ''' <summary>
        ''' The command-line arguments are not valid.
        ''' </summary>
        InvalidArguments = 2

        ''' <summary>
        ''' The directory does not exist, or its path is not valid.
        ''' </summary>
        InvalidDirectory = 3

        ''' <summary>
        ''' The operation could not be completed (e.g. the directory is not accessible).
        ''' </summary>
        OperationFailed = 4

        ''' <summary>
        ''' The operation was cancelled by the user (Ctrl+C).
        ''' </summary>
        Cancelled = 5

        ''' <summary>
        ''' A required component is missing (the native SkiaSharp library).
        ''' </summary>
        MissingComponent = 6

    End Enum

End Namespace
