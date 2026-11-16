Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel

Imports ImageAspectRatioGrouper.Core
Imports ImageAspectRatioGrouper.Theming

#End Region

Namespace ImageAspectRatioGrouper.Infrastructure

    ''' <summary>
    ''' Strongly-typed, validated facade over <c>My.Settings</c> that persists the user's preferences across restarts.
    ''' </summary>
    Friend NotInheritable Class UserPreferences

#Region " Constants "

        ''' <summary>
        ''' The minimum allowed aspect ratio tolerance, in pixels.
        ''' </summary>
        Friend Const MinimumTolerancePixels As Integer = 0

        ''' <summary>
        ''' The maximum allowed aspect ratio tolerance, in pixels.
        ''' </summary>
        Friend Const MaximumTolerancePixels As Integer = 100

        ''' <summary>
        ''' The lowest allowed value for the minimum result height, in pixels.
        ''' </summary>
        Friend Const MinimumHeightLowerBound As Integer = 0

        ''' <summary>
        ''' The highest allowed value for the minimum result height, in pixels.
        ''' </summary>
        Friend Const MinimumHeightUpperBound As Integer = 10000

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="UserPreferences"/> class from being created.
        ''' </summary>
        Private Sub New()
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets or sets the preferred theme.
        ''' <para></para>
        ''' Until the user explicitly picks a theme, the current Windows apps theme is used.
        ''' </summary>
        ''' <exception cref="InvalidEnumArgumentException">Thrown when the assigned value is not a defined <see cref="AppTheme"/>.</exception>
        Friend Shared Property Theme As AppTheme
            Get
                If Not My.Settings.HasUserThemePreference Then
                    Return If(WindowChrome.IsWindowsDarkModeEnabled(), AppTheme.Dark, AppTheme.Light)
                End If

                Return If(My.Settings.IsDarkTheme, AppTheme.Dark, AppTheme.Light)
            End Get
            Set(value As AppTheme)
                If Not [Enum].IsDefined(GetType(AppTheme), value) Then
                    Throw New InvalidEnumArgumentException(NameOf(value), CInt(value), GetType(AppTheme))
                End If

                My.Settings.IsDarkTheme = (value = AppTheme.Dark)
                My.Settings.HasUserThemePreference = True
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the aspect ratio tolerance, in pixels.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when the persisted value is out of range (corrupted settings).</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when the assigned value is out of range.</exception>
        Friend Shared Property AspectRatioTolerance As Integer
            Get
                Dim value As Integer = My.Settings.AspectRatioTolerance
                If value < UserPreferences.MinimumTolerancePixels OrElse value > UserPreferences.MaximumTolerancePixels Then
                    Throw New InvalidOperationException($"The persisted tolerance value ({value}) is outside the valid range [{UserPreferences.MinimumTolerancePixels}-{UserPreferences.MaximumTolerancePixels}]. The user.config file may be corrupted.")
                End If

                Return value
            End Get
            Set(value As Integer)
                If value < UserPreferences.MinimumTolerancePixels OrElse value > UserPreferences.MaximumTolerancePixels Then
                    Throw New ArgumentOutOfRangeException(NameOf(value), value, $"Tolerance must be between {UserPreferences.MinimumTolerancePixels} and {UserPreferences.MaximumTolerancePixels}.")
                End If

                My.Settings.AspectRatioTolerance = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the target aspect ratio.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when the persisted key is unknown (corrupted settings).</exception>
        ''' <exception cref="ArgumentNullException">Thrown when the assigned value is <see langword="Nothing"/>.</exception>
        Friend Shared Property AspectRatio As AspectRatio
            Get
                Dim key As String = My.Settings.AspectRatioKey
                Try
                    Return AspectRatio.FromKey(key)
                Catch ex As ArgumentException
                    Throw New InvalidOperationException($"The persisted aspect ratio key ('{key}') is not a known standard ratio. The user.config file may be corrupted.", ex)
                End Try
            End Get
            Set(value As AspectRatio)
                If value Is Nothing Then
                    Throw New ArgumentNullException(NameOf(value))
                End If

                My.Settings.AspectRatioKey = value.Key
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the minimum acceptable result height, in pixels.
        ''' </summary>
        ''' <exception cref="InvalidOperationException">Thrown when the persisted value is out of range (corrupted settings).</exception>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when the assigned value is out of range.</exception>
        Friend Shared Property MinimumHeight As Integer
            Get
                Dim value As Integer = My.Settings.MinimumHeight
                If value < UserPreferences.MinimumHeightLowerBound OrElse value > UserPreferences.MinimumHeightUpperBound Then
                    Throw New InvalidOperationException($"The persisted minimum height ({value}) is outside the valid range [{UserPreferences.MinimumHeightLowerBound}-{UserPreferences.MinimumHeightUpperBound}]. The user.config file may be corrupted.")
                End If

                Return value
            End Get
            Set(value As Integer)
                If value < UserPreferences.MinimumHeightLowerBound OrElse value > UserPreferences.MinimumHeightUpperBound Then
                    Throw New ArgumentOutOfRangeException(NameOf(value), value, $"Minimum height must be between {UserPreferences.MinimumHeightLowerBound} and {UserPreferences.MinimumHeightUpperBound}.")
                End If

                My.Settings.MinimumHeight = value
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets a value indicating whether the directory search must be recursive.
        ''' </summary>
        Friend Shared Property SearchRecursively As Boolean
            Get
                Return My.Settings.SearchRecursively
            End Get
            Set(value As Boolean)
                My.Settings.SearchRecursively = value
            End Set
        End Property

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Migrates the settings of a previous application version (if any) to the current version.
        ''' <para></para>
        ''' <c>user.config</c> is stored per assembly version, so without this step every version bump would reset the preferences.
        ''' </summary>
        Friend Shared Sub UpgradeIfRequired()
            If Not My.Settings.UpgradeRequired Then
                Return
            End If

            My.Settings.Upgrade()
            My.Settings.UpgradeRequired = False
            My.Settings.Save()
        End Sub

        ''' <summary>
        ''' Persists the current preferences to disk.
        ''' </summary>
        Friend Shared Sub Save()
            My.Settings.Save()
        End Sub

#End Region

    End Class

End Namespace
