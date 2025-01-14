// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the �Software�), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED �AS IS�, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

//  <auto-generated>
//      This code was generated by:
//        TerminalGuiDesigner v1.0.24.0
//      You can make changes to this file and they will not be overwritten when saving.
//  </auto-generated>
// -----------------------------------------------------------------------------

#nullable enable

namespace SnapsInAZfs.ConfigConsole;

public sealed partial class GlobalConfigurationWindow
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public GlobalConfigurationWindow( )
    {
        // ReSharper disable once HeapView.ObjectAllocation.Possible
        // ReSharper disable once HeapView.DelegateAllocation
        Initialized += GlobalConfigurationWindowOnInitialized;
        InitializeComponent( );
        EnableEventHandlers( );
    }

    private bool _eventsEnabled;

    internal bool IsConfigurationChanged =>
        ValidateGlobalConfigValues( )
        && ( Program.Settings!.DryRun != dryRunRadioGroup.GetSelectedBooleanFromLabel( )
             || Program.Settings.TakeSnapshots != takeSnapshotsRadioGroup.GetSelectedBooleanFromLabel( )
             || Program.Settings.PruneSnapshots != pruneSnapshotsRadioGroup.GetSelectedBooleanFromLabel( )
             || Program.Settings.LocalSystemName != localSystemNameTextBox.Text.ToString( )!
             || Program.Settings.ZfsPath != pathToZfsTextField.Text.ToString( )!
             || Program.Settings.ZpoolPath != pathToZpoolTextField.Text.ToString( )! );

    internal bool ValidateGlobalConfigValues( )
    {
        if ( pathToZfsTextField.Text.IsEmpty || pathToZpoolTextField.Text.IsEmpty || localSystemNameTextBox.Text.IsEmpty )
        {
            return false;
        }

        return Environment.OSVersion.Platform != PlatformID.Unix
               || ( File.Exists( pathToZfsTextField.Text.ToString( ) ) && File.Exists( pathToZpoolTextField.Text.ToString( ) ) );
    }

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

    private void GlobalConfigurationWindowOnInitialized( object? sender, EventArgs e )
    {
        SetFieldsFromSettingsObject( false );
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
        localSystemNameTextBox.Text = Program.Settings.LocalSystemName;
        pathToZfsTextField.Text = Program.Settings.ZfsPath;
        pathToZpoolTextField.Text = Program.Settings.ZpoolPath;

        Logger.Debug( "Finished setting global configuration fields to values in Settings" );

        if ( manageEventHandlers )
        {
            EnableEventHandlers( );
        }
    }
}
