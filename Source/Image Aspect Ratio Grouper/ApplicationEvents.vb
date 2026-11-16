Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.Windows.Forms

Imports Microsoft.VisualBasic.ApplicationServices

Imports ImageAspectRatioGrouper.Infrastructure
Imports ImageAspectRatioGrouper.Theming

#End Region

Namespace My

    ''' <summary>
    ''' Application-level events (Visual Basic Application Framework).
    ''' </summary>
    Partial Friend Class MyApplication

        ''' <summary>
        ''' Runs before the main form is created: migrates settings and activates the persisted theme,
        ''' so that the very first window handle is already created with the right title bar color.
        ''' </summary>
        Private Sub MyApplication_Startup(sender As Object, e As StartupEventArgs) Handles Me.Startup
            UserPreferences.UpgradeIfRequired()
            ThemeManager.SetTheme(UserPreferences.Theme)
        End Sub

        ''' <summary>
        ''' Last-chance handler: shows the full exception details instead of failing silently, then terminates.
        ''' </summary>
        Private Sub MyApplication_UnhandledException(sender As Object, e As UnhandledExceptionEventArgs) Handles Me.UnhandledException
            MessageBox.Show(e.Exception.ToString(),
                            $"{My.Application.Info.Title} - Unhandled Exception",
                            MessageBoxButtons.OK, MessageBoxIcon.Error)
            e.ExitApplication = True
        End Sub

    End Class

End Namespace
