// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

//  <auto-generated>
//      This code was generated by:
//        TerminalGuiDesigner v1.0.24.0
//      You can make changes to this file and they will not be overwritten when saving.
//  </auto-generated>
// -----------------------------------------------------------------------------

using System.Text.Json;
using System.Text.Json.Serialization;
using Sanoid.Settings.Settings;

namespace Sanoid.ConfigConsole
{
    using Terminal.Gui;

    public partial class GlobalConfigurationWindow
    {
        public GlobalConfigurationWindow( )
        {
            Initialized += GlobalConfigurationWindowOnInitialized;
            InitializeComponent( );
            EnableEventHandlers( );
        }

        private void GlobalConfigurationWindowOnInitialized( object sender, EventArgs e )
        {
            SetFieldsFromSettingsObject( false );
        }

        private bool _eventsEnabled;
        [NotNull]
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

        private void DisableEventHandlers( )
        {
            if ( !_eventsEnabled )
            {
                return;
            }

            resetGlobalConfigButton.Clicked -= ResetButtonOnClicked;

            _eventsEnabled = false;
        }

        private void EnableEventHandlers( )
        {
            if ( _eventsEnabled )
            {
                return;
            }

            resetGlobalConfigButton.Clicked += ResetButtonOnClicked;
            _eventsEnabled = true;
        }

        private bool ValidateGlobalConfigValues( )
        {
            if ( pathToZfsTextField.Text.IsEmpty || pathToZpoolTextField.Text.IsEmpty )
            {
                return false;
            }

            if ( ( Environment.OSVersion.Platform == PlatformID.Unix && !File.Exists( pathToZfsTextField.Text.ToString( ) ) ) || !File.Exists( pathToZpoolTextField.Text.ToString( ) ) )
            {
                return false;
            }

            return true;
        }

        private void ResetButtonOnClicked( )
        {
            SetFieldsFromSettingsObject( );
        }

        private void SetFieldsFromSettingsObject( bool manageEventHandlers = true )
        {
            if ( manageEventHandlers )
            {
                DisableEventHandlers( );
            }

            Logger.Debug( "Setting global configuration fields to values in Settings" );

            dryRunRadioGroup.SelectedItem = Program.Settings!.DryRun ? 0 : 1;
            takeSnapshotsRadioGroup.SelectedItem = Program.Settings.TakeSnapshots ? 0 : 1;
            pruneSnapshotsRadioGroup.SelectedItem = Program.Settings.PruneSnapshots ? 0 : 1;
            pathToZfsTextField.Text = Program.Settings.ZfsPath;
            pathToZpoolTextField.Text = Program.Settings.ZpoolPath;

            Logger.Debug( "Finished setting global configuration fields to values in Settings" );

            if ( manageEventHandlers )
            {
                EnableEventHandlers( );
            }
        }

        private void ShowSaveDialog( )
        {
            if ( ValidateGlobalConfigValues( ) )
            {
                using ( SaveDialog globalConfigSaveDialog = new( "Save Global Configuration", "Select file to save global configuration", new( ) { ".json" } ) )
                {
                    globalConfigSaveDialog.AllowsOtherFileTypes = true;
                    globalConfigSaveDialog.CanCreateDirectories = true;
                    globalConfigSaveDialog.Modal = true;
                    Application.Run( globalConfigSaveDialog );
                    if ( globalConfigSaveDialog.Canceled )
                    {
                        return;
                    }

                    if ( globalConfigSaveDialog.FileName.IsEmpty )
                    {
                        return;
                    }

                    SanoidSettings settings = new( )
                    {
                        DryRun = dryRunRadioGroup.GetSelectedBooleanFromLabel(),
                        TakeSnapshots = takeSnapshotsRadioGroup.GetSelectedBooleanFromLabel(),
                        PruneSnapshots = pruneSnapshotsRadioGroup.GetSelectedBooleanFromLabel(),
                        ZfsPath = pathToZfsTextField.Text?.ToString( ) ?? string.Empty,
                        ZpoolPath = pathToZpoolTextField.Text?.ToString( ) ?? string.Empty,
                        Templates = Program.Settings!.Templates,
                        CacheDirectory = Program.Settings.CacheDirectory
                    };

                    File.WriteAllText( globalConfigSaveDialog.FileName.ToString( ) ?? throw new InvalidOperationException( "Null string provided for save file name" ), JsonSerializer.Serialize( settings, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.Never } ) );
                }
            }
        }
    }
}