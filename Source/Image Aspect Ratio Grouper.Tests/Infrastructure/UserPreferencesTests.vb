Option Strict On
Option Explicit On
Option Infer Off

#Region " Imports "

Imports System.ComponentModel

Imports ImageAspectRatioGrouper.Core
Imports ImageAspectRatioGrouper.Infrastructure
Imports ImageAspectRatioGrouper.Tests.TestInfrastructure
Imports ImageAspectRatioGrouper.Theming

#End Region

Namespace ImageAspectRatioGrouper.Tests.Infrastructure

    ''' <summary>
    ''' Tests for <see cref="UserPreferences"/> (and therefore for the application settings it wraps).
    ''' </summary>
    <Collection(SharedStateCollection.Name)>
    Public NotInheritable Class UserPreferencesTests

        ''' <summary>
        ''' The default values must match the UI defaults.
        ''' </summary>
        <Fact>
        Public Sub Defaults_MatchTheDesignDefaults()
            Dim settingsProperties As Configuration.SettingsPropertyCollection = My.Settings.Properties

            Assert.Equal(CStr(ImageGroupingOptions.DefaultTolerancePixels), CStr(settingsProperties("AspectRatioTolerance").DefaultValue))
            Assert.Equal(CStr(ImageGroupingOptions.DefaultMinimumHeight), CStr(settingsProperties("MinimumHeight").DefaultValue))
            Assert.Equal(AspectRatio.Default.Key, CStr(settingsProperties("AspectRatioKey").DefaultValue))
            Assert.Equal("True", CStr(settingsProperties("SearchRecursively").DefaultValue))
            Assert.Equal("True", CStr(settingsProperties("UpgradeRequired").DefaultValue))
        End Sub

        ''' <summary>
        ''' The target directory is intentionally never persisted: no setting may store a directory or path.
        ''' </summary>
        <Fact>
        Public Sub Settings_NeverPersistTheTargetDirectory()
            For Each settingProperty As Configuration.SettingsProperty In My.Settings.Properties
                Assert.DoesNotContain("Directory", settingProperty.Name, StringComparison.OrdinalIgnoreCase)
                Assert.DoesNotContain("Path", settingProperty.Name, StringComparison.OrdinalIgnoreCase)
            Next
        End Sub

        ''' <summary>
        ''' Every preference round-trips through the settings.
        ''' </summary>
        <Fact>
        Public Sub Preferences_RoundTrip()
            Using New SettingsSnapshot()
                UserPreferences.AspectRatioTolerance = 17
                UserPreferences.MinimumHeight = 999
                UserPreferences.AspectRatio = AspectRatio.Android9x20
                UserPreferences.SearchRecursively = False
                UserPreferences.Theme = AppTheme.Dark

                Assert.Equal(17, UserPreferences.AspectRatioTolerance)
                Assert.Equal(999, UserPreferences.MinimumHeight)
                Assert.Same(AspectRatio.Android9x20, UserPreferences.AspectRatio)
                Assert.False(UserPreferences.SearchRecursively)
                Assert.Equal(AppTheme.Dark, UserPreferences.Theme)

                UserPreferences.Theme = AppTheme.Light
                Assert.Equal(AppTheme.Light, UserPreferences.Theme)
                Assert.True(My.Settings.HasUserThemePreference)
            End Using
        End Sub

        ''' <summary>
        ''' Out-of-range assignments must fail fast and leave the stored value untouched.
        ''' </summary>
        <Fact>
        Public Sub InvalidAssignments_Throw()
            Using New SettingsSnapshot()
                UserPreferences.AspectRatioTolerance = 5

                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() UserPreferences.AspectRatioTolerance = UserPreferences.MaximumTolerancePixels + 1)
                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() UserPreferences.AspectRatioTolerance = UserPreferences.MinimumTolerancePixels - 1)
                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() UserPreferences.MinimumHeight = UserPreferences.MinimumHeightUpperBound + 1)
                Assert.Throws(Of ArgumentOutOfRangeException)(Sub() UserPreferences.MinimumHeight = UserPreferences.MinimumHeightLowerBound - 1)
                Assert.Throws(Of ArgumentNullException)(Sub() UserPreferences.AspectRatio = Nothing)
                Assert.Throws(Of InvalidEnumArgumentException)(Sub() UserPreferences.Theme = CType(42, AppTheme))

                Assert.Equal(5, UserPreferences.AspectRatioTolerance)
            End Using
        End Sub

        ''' <summary>
        ''' Corrupted persisted values must fail fast instead of being silently accepted.
        ''' </summary>
        <Fact>
        Public Sub CorruptedPersistedValues_Throw()
            Using New SettingsSnapshot()
                My.Settings.AspectRatioTolerance = 5000
                Assert.Throws(Of InvalidOperationException)(Function() UserPreferences.AspectRatioTolerance)

                My.Settings.MinimumHeight = -3
                Assert.Throws(Of InvalidOperationException)(Function() UserPreferences.MinimumHeight)

                My.Settings.AspectRatioKey = "not-a-ratio"
                Assert.Throws(Of InvalidOperationException)(Function() UserPreferences.AspectRatio)
            End Using
        End Sub

        ''' <summary>
        ''' Until the user explicitly picks a theme, the stored dark flag is ignored (the Windows theme is followed).
        ''' </summary>
        <Fact>
        Public Sub Theme_WithoutUserPreference_IgnoresTheStoredFlag()
            Using New SettingsSnapshot()
                My.Settings.HasUserThemePreference = False

                My.Settings.IsDarkTheme = True
                Dim themeWhenFlagIsDark As AppTheme = UserPreferences.Theme

                My.Settings.IsDarkTheme = False
                Dim themeWhenFlagIsLight As AppTheme = UserPreferences.Theme

                Assert.Equal(themeWhenFlagIsDark, themeWhenFlagIsLight)
            End Using
        End Sub

    End Class

End Namespace
