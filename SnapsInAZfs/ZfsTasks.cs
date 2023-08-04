﻿// LICENSE:
// 
// Copyright 2023 Brandon Thetford
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using SnapsInAZfs.Interop.Libc.Enums;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs;

internal static class ZfsTasks
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );
    private static readonly AutoResetEvent SnapshotAutoResetEvent = new( true );

    /// <exception cref="InvalidOperationException">If an invalid value is returned when getting the mutex</exception>
    internal static async Task TakeAllConfiguredSnapshotsAsync( IZfsCommandRunner commandRunner, SnapsInAZfsSettings settings, DateTimeOffset timestamp, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        if ( !SnapshotAutoResetEvent.WaitOne( 30000 ) )
        {
            Logger.Error( "Timed out waiting to take snapshots. Another operation is in progress" );
            return;
        }

        Logger.Info( "Begin taking snapshots for all configured datasets" );

        //Need to operate on a sorted collection
        ImmutableSortedDictionary<string, ZfsRecord> sortedDatasets = datasets.ToImmutableSortedDictionary( );
        foreach ( ( string _, ZfsRecord ds ) in sortedDatasets )
        {
            //OK to disable this warning here. We don't use it if the result is false, and we don't put null in the collection in the first place
#pragma warning disable CS8600
            if ( !settings.Templates.TryGetValue( ds.Template.Value, out TemplateSettings template ) )
            {
                Logger.Error( "Template {0} specified for {1} not found in configuration - skipping snapshots for {1}", ds.Template.Name, ds.Name );
                continue;
            }
#pragma warning restore CS8600

            // The MaxBy function will fail if the sort key is a value type (it is - DateTimeOffset) and the collection is null
            // ReSharper disable SimplifyLinqExpressionUseMinByAndMaxBy
            List<IZfsProperty> propsToSet = new( );
            if ( ds is not { TakeSnapshots.Value: true } )
            {
                Logger.Debug( "Dataset {0} not configured to take snapshots - skipping", ds.Name );
                continue;
            }

            if ( ds is not { Enabled.Value: true } )
            {
                Logger.Debug( "Dataset {0} is disabled - skipping", ds.Name );
                continue;
            }

            Logger.Debug( "Checking for and taking needed snapshots for dataset {0}", ds.Name );

            if ( ds.IsFrequentSnapshotNeeded( template.SnapshotTiming, in timestamp ) )
            {
                Logger.Debug( "Frequent snapshot needed for dataset {0}", ds.Name );
                ( bool success, Snapshot? snapshot ) = TakeSnapshotKind( ds, SnapshotPeriod.Frequent, propsToSet );
                if ( success && snapshot is not null )
                {
                    snapshots[ snapshot.Name ] = snapshot;
                }
            }

            if ( ds.IsHourlySnapshotNeeded( in timestamp ) )
            {
                Logger.Debug( "Hourly snapshot needed for dataset {0}", ds.Name );
                ( bool success, Snapshot? snapshot ) = TakeSnapshotKind( ds, SnapshotPeriod.Hourly, propsToSet );
                if ( success && snapshot is not null )
                {
                    snapshots[ snapshot.Name ] = snapshot;
                }
            }

            if ( ds.IsDailySnapshotNeeded( in timestamp ) )
            {
                Logger.Debug( "Daily snapshot needed for dataset {0}", ds.Name );
                ( bool success, Snapshot? snapshot ) = TakeSnapshotKind( ds, SnapshotPeriod.Daily, propsToSet );
                if ( success && snapshot is not null )
                {
                    snapshots[ snapshot.Name ] = snapshot;
                }
            }

            if ( ds.IsWeeklySnapshotNeeded( template.SnapshotTiming, in timestamp ) )
            {
                Logger.Debug( "Weekly snapshot needed for dataset {0}", ds.Name );
                ( bool success, Snapshot? snapshot ) = TakeSnapshotKind( ds, SnapshotPeriod.Weekly, propsToSet );
                if ( success && snapshot is not null )
                {
                    snapshots[ snapshot.Name ] = snapshot;
                }
            }

            if ( ds.IsMonthlySnapshotNeeded( in timestamp ) )
            {
                Logger.Debug( "Monthly snapshot needed for dataset {0}", ds.Name );
                ( bool success, Snapshot? snapshot ) = TakeSnapshotKind( ds, SnapshotPeriod.Monthly, propsToSet );
                if ( success && snapshot is not null )
                {
                    snapshots[ snapshot.Name ] = snapshot;
                }
            }

            if ( ds.IsYearlySnapshotNeeded( in timestamp ) )
            {
                Logger.Debug( "Yearly snapshot needed for dataset {0}", ds.Name );
                ( bool success, Snapshot? snapshot ) = TakeSnapshotKind( ds, SnapshotPeriod.Yearly, propsToSet );
                if ( success && snapshot is not null )
                {
                    snapshots[ snapshot.Name ] = snapshot;
                }
            }

            if ( propsToSet.Any( ) )
            {
                Logger.Debug( "Took snapshots of {0}. Need to set properties: {1}", ds.Name, propsToSet.Select( p => $"{p.Name}: {p.ValueString}" ).ToCommaSeparatedSingleLineString( ) );
                ZfsCommandRunnerOperationStatus setPropertiesResult = await commandRunner.SetZfsPropertiesAsync( settings.DryRun, ds.Name, propsToSet.ToArray( ) ).ConfigureAwait( true );
                switch ( setPropertiesResult )
                {
                    case ZfsCommandRunnerOperationStatus.Success:
                        Logger.Debug( "Property set successful" );
                    continue;
                    case ZfsCommandRunnerOperationStatus.DryRun:
                        Logger.Info( "DRY RUN: No properties were set on actual datasets" );
                    continue;
                    case ZfsCommandRunnerOperationStatus.ZeroLengthRequest:
                        Logger.Warn( "Set property request contained 0 elements for {0}", ds.Name );
                        continue;
                    default:
                        Logger.Error( "Error setting properties for dataset {0}", ds.Name );
                    continue;
                }

            }

            Logger.Debug( "No snapshots needed for dataset {0}", ds.Name );
        }

        Logger.Info( "Finished taking snapshots" );

        SnapshotAutoResetEvent.Set( );

        return;

        (bool success, Snapshot? snapshot) TakeSnapshotKind( ZfsRecord ds, SnapshotPeriod period, List<IZfsProperty> propsToSet )
        {
            Logger.Trace( "Requested to take {0} snapshot of {1}", period, ds.Name );
            bool snapshotTaken = TakeSnapshot( commandRunner, settings, ds, period, timestamp, out Snapshot? snapshot );
            switch ( snapshotTaken )
            {
                case true:
                    Logger.Trace( "{0} snapshot {1} taken successfully", period, snapshot?.Name ?? $"of {ds.Name}" );
                    propsToSet.Add( ds.UpdateProperty( period.GetMostRecentSnapshotZfsPropertyName( ), in timestamp ) );
                    return ( true, snapshot );
                case false when settings.DryRun:
                    propsToSet.Add( ds.UpdateProperty( period.GetMostRecentSnapshotZfsPropertyName( ), in timestamp ) );
                    return ( true, null );
                default:
                    return ( false, null );
            }
        }
    }

    internal static async Task<Errno> PruneAllConfiguredSnapshotsAsync( IZfsCommandRunner commandRunner, SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets )
    {
        if ( !SnapshotAutoResetEvent.WaitOne( 30000 ) )
        {
            Logger.Error( "Timed out waiting to prune snapshots. Another operation is in progress." );
            return Errno.EBUSY;
        }

        Logger.Info( "Begin pruning snapshots for all configured datasets" );
        await Parallel.ForEachAsync( datasets.Values, new ParallelOptions { MaxDegreeOfParallelism = 4 }, async ( ds, _ ) => await PruneSnapshotsForDatasetAsync( ds ).ConfigureAwait( false ) ).ConfigureAwait( false );

        Logger.Info( "Finished pruning snapshots" );
        SnapshotAutoResetEvent.Set( );

        return Errno.EOK;

        async Task PruneSnapshotsForDatasetAsync( ZfsRecord ds )
        {
            if ( ds is not { Enabled.Value: true } )
            {
                Logger.Debug( "Dataset {0} is disabled - skipping prune", ds.Name );
                return;
            }

            if ( ds is not { PruneSnapshots.Value: true } )
            {
                Logger.Debug( "Dataset {0} not configured to prune snapshots - skipping", ds.Name );
                return;
            }

            List<Snapshot> snapshotsToPruneForDataset = ds.GetSnapshotsToPrune( );

            Logger.Debug( "Need to prune the following snapshots from {0}: {1}", ds.Name, snapshotsToPruneForDataset.ToCommaSeparatedSingleLineString( true ) );

            foreach ( Snapshot snapshot in snapshotsToPruneForDataset )
            {
                ZfsCommandRunnerOperationStatus destroyStatus = await commandRunner.DestroySnapshotAsync( snapshot, settings ).ConfigureAwait( false );
                switch ( destroyStatus )
                {
                    case ZfsCommandRunnerOperationStatus.DryRun:
                        Logger.Info( "DRY RUN: Snapshot not destroyed, but pretending it was for simulation" );
                        goto Remove;
                    case ZfsCommandRunnerOperationStatus.Success:
                        Logger.Info( "Destroyed snapshot {0}", snapshot.Name );
                    Remove:
                        if ( !ds.RemoveSnapshot( snapshot ) )
                        {
                            Logger.Debug( "Unable to remove snapshot {0} from {1} {2} object", snapshot.Name, ds.Kind, ds.Name );
                        }
                        continue;
                    case ZfsCommandRunnerOperationStatus.ZfsProcessFailure:
                    case ZfsCommandRunnerOperationStatus.Failure:
                    case ZfsCommandRunnerOperationStatus.NameValidationFailed:
                    default:
                        Logger.Error( "Failed to destroy snapshot {0}", snapshot.Name );
                        continue;
                }
            }
        }
    }

    internal static bool TakeSnapshot( IZfsCommandRunner commandRunner, SnapsInAZfsSettings settings, ZfsRecord ds, SnapshotPeriod period, DateTimeOffset timestamp, out Snapshot? snapshot )
    {
        Logger.Trace( "TakeSnapshot called for {0} with period {1}", ds.Name, period );
        snapshot = null;

        switch ( ds )
        {
            case { Enabled.Value: false }:
                Logger.Trace( "Dataset {0} is not enabled. Skipping", ds.Name );
                return false;
            case { TakeSnapshots.Value: false }:
                Logger.Trace( "Dataset {0} is not configured to take snapshots. Skipping", ds.Name );
                return false;
            case { IsPoolRoot: false, Recursion.Value: ZfsPropertyValueConstants.ZfsRecursion, ParentDataset.Recursion.Value: ZfsPropertyValueConstants.ZfsRecursion }:
                Logger.Trace( "Ancestor {1} of dataset {0} is already configured for zfs native recursion. Skipping", ds.Name, ds.ParentDataset.Name );
                return false;
            case { IsPoolRoot: false, Recursion.Value: ZfsPropertyValueConstants.SnapsInAZfs, ParentDataset.Recursion.Value: ZfsPropertyValueConstants.ZfsRecursion }:
                Logger.Warn( "Ancestor {1} of dataset {0} is configured for zfs native recursion and local recursion is set. No new snapshot will be taken of {0} to avoid name collision. Check ZFS configuration", ds.Name, ds.ParentDataset.Name );
                return false;
        }

        Logger.Trace( "Looking up template {0} for {1} snapshot of {2}", ds.Template.Value, period, ds.Name );

        if ( !settings.Templates.TryGetValue( ds.Template.Value, out TemplateSettings? template ) )
        {
            Logger.Error( "Template {0} for dataset {1} not found in configuration. Skipping", ds.Template.Value, ds.Name );
            return false;
        }

        switch ( period.Kind )
        {
            case SnapshotPeriodKind.Frequent:
                if ( ds.SnapshotRetentionFrequent.IsNotWanted( ) )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", period, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.Hourly:
                if ( ds.SnapshotRetentionHourly.IsNotWanted( ) )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", period, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.Daily:
                if ( ds.SnapshotRetentionDaily.IsNotWanted( ) )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", period, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.Weekly:
                if ( ds.SnapshotRetentionWeekly.IsNotWanted( ) )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", period, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.Monthly:
                if ( ds.SnapshotRetentionMonthly.IsNotWanted( ) )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", period, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.Yearly:
                if ( ds.SnapshotRetentionYearly.IsNotWanted( ) )
                {
                    Logger.Trace( "Requested {0} snapshot, but dataset {1} does not want them. Skipping", period, ds.Name );
                    return false;
                }

                break;
            case SnapshotPeriodKind.NotSet:
            default:
                throw new ArgumentOutOfRangeException( nameof( period ), period, $"Unexpected value received for Period for dataset {ds.Name}. Snapshot not taken." );
        }

        Logger.Trace( "{0} {1} will have a {2} snapshot taken with these settings: {3}", ds.Kind, ds.Name, period, JsonSerializer.Serialize( new { ds.Template, ds.Recursion } ) );

        ZfsCommandRunnerOperationStatus zfsCommandRunnerStatus = commandRunner.TakeSnapshot( ds, period, timestamp, settings, template, out snapshot );
        switch ( zfsCommandRunnerStatus )
        {
            case ZfsCommandRunnerOperationStatus.DryRun:
                ds.AddSnapshot( snapshot! );
                Logger.Info( "DRY RUN: Pretending snapshot {0} was successfully taken", snapshot!.Name );
                return false;
            case ZfsCommandRunnerOperationStatus.Success:
                ds.AddSnapshot( snapshot! );
                Logger.Info( "Snapshot {0} successfully taken", snapshot!.Name );
                return true;
            default:
                Logger.Error( "{0} snapshot for {1} {2} not taken", period, ds.Kind, ds.Name );
                return false;
        }
    }

    internal static bool UpdateZfsDatasetSchema( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> poolRootsWithPropertyValidities, IZfsCommandRunner zfsCommandRunner )
    {
        bool errorsEncountered = false;
        Logger.Debug( "Requested update of zfs properties schema" );
        foreach ( ( string poolName, ConcurrentDictionary<string, bool> propertiesToAdd ) in poolRootsWithPropertyValidities )
        {
            Logger.Info( "Updating properties for pool {0}", poolName );

            // It's not a nullable type...
            // ReSharper disable once ExceptionNotDocumentedOptional
            string[] propertyArray = propertiesToAdd.Where( kvp => !kvp.Value ).Select( kvp => kvp.Key ).ToArray( );

            if ( propertyArray.Length == 0 )
            {
                Logger.Info( "No missing properties to set for {0} - Skipping", poolName );
                continue;
            }

            // Attempt to set the missing properties for the pool.
            // Log an error if unsuccessful
            if ( zfsCommandRunner.SetDefaultValuesForMissingZfsPropertiesOnPoolAsync( settings, poolName, propertyArray ) )
            {
                Logger.Info( "Finished updating properties for pool {0}", poolName );
            }
            else
            {
                if ( settings.DryRun )
                {
                    Logger.Info( "DRY RUN: Properties intentionally not set for {0}: {1}", poolName, JsonSerializer.Serialize( propertyArray ) );
                }
                else
                {
                    errorsEncountered = true;
                    Logger.Error( "Failed updating properties for pool {0}. Unset properties: {1}", poolName, JsonSerializer.Serialize( propertyArray ) );
                }
            }
        }

        Logger.Debug( "Finished updating zfs properties schema for all pool roots" );
        if ( errorsEncountered )
        {
            Logger.Error( "Some operations failed. See previous log output." );
        }

        return !errorsEncountered;
    }

    public record CheckZfsPropertiesSchemaResult( ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> PoolRootsWithPropertyValidities, bool MissingPropertiesFound );

    public static async Task<CheckZfsPropertiesSchemaResult> CheckZfsPoolRootPropertiesSchemaAsync( IZfsCommandRunner zfsCommandRunner, CommandLineArguments args )
    {
        Logger.Debug( "Checking zfs properties schema" );

        ConcurrentDictionary<string, ConcurrentDictionary<string, bool>> poolRootsWithPropertyValidities = await zfsCommandRunner.GetPoolRootsAndPropertyValiditiesAsync( ).ConfigureAwait( false );
        bool missingPropertiesFound = false;
        foreach ( ( string poolName, ConcurrentDictionary<string, bool>? propertyValidities ) in poolRootsWithPropertyValidities )
        {
            Logger.Debug( "Checking property validities for pool root {0}", poolName );
            bool missingPropertiesFoundForPool = false;
            foreach ( ( string propName, bool propValue ) in propertyValidities )
            {
                if ( !IZfsProperty.DefaultDatasetProperties.ContainsKey( propName ) )
                {
                    Logger.Trace( "Not interested in property {0} for pool root schema check", propName );
                    continue;
                }

                Logger.Trace( "Checking validity of property {0} in pool root {1}", propName, poolName );
                if ( propValue )
                {
                    Logger.Trace( "Pool root {0} has property {1} with a valid value", poolName, propName );
                    continue;
                }

                Logger.Debug( "Pool root {0} has missing or invalid property {1}", poolName, propName );
                if ( !missingPropertiesFoundForPool )
                {
                    missingPropertiesFound = missingPropertiesFoundForPool = true;
                }
            }

            foreach ( ( string propName, _ ) in IZfsProperty.DefaultDatasetProperties )
            {
                if ( propName.StartsWith( ZfsPropertyNames.SiazNamespace ) && !propertyValidities.ContainsKey( propName ) )
                {
                    propertyValidities.TryAdd( propName, false );
                    missingPropertiesFound = missingPropertiesFoundForPool = true;
                }
            }


            Logger.Debug( "Finished checking property validities for pool root {0}", poolName );

            switch ( args )
            {
                case { CheckZfsProperties: true } when missingPropertiesFoundForPool:
                    Logger.Warn( "Pool {0} is missing the following properties: {1}", poolName, propertyValidities.Where( kvp => !kvp.Value ).Select( kvp => kvp.Key ).ToCommaSeparatedSingleLineString( true ) );
                    continue;
                case { CheckZfsProperties: true } when !missingPropertiesFoundForPool:
                    Logger.Info( "No missing properties in pool {0}", poolName );
                    continue;
                case { PrepareZfsProperties: true } when missingPropertiesFoundForPool:
                    Logger.Info( "Pool {0} is missing the following properties: {1}", poolName, propertyValidities.Where( kvp => !kvp.Value ).Select( kvp => kvp.Key ).ToCommaSeparatedSingleLineString( true ) );
                    continue;
                case { PrepareZfsProperties: true } when !missingPropertiesFoundForPool:
                    Logger.Info( "No missing properties in pool {0}", poolName );
                    continue;
                case { PrepareZfsProperties: false, CheckZfsProperties: false } when missingPropertiesFoundForPool:
                    Logger.Fatal( "Pool {0} is missing the following properties: {1}", poolName, propertyValidities.Where( kvp => !kvp.Value ).Select( kvp => kvp.Key ).ToCommaSeparatedSingleLineString( true ) );
                    continue;
                case { PrepareZfsProperties: false, CheckZfsProperties: false } when !missingPropertiesFoundForPool:
                    Logger.Debug( "No missing properties in pool {0}", poolName );
                    continue;
            }
        }

        return new( poolRootsWithPropertyValidities, missingPropertiesFound );
    }

    public static Task GetDatasetsAndSnapshotsFromZfsAsync( SnapsInAZfsSettings settings, IZfsCommandRunner zfsCommandRunner, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        return zfsCommandRunner.GetDatasetsAndSnapshotsFromZfsAsync( settings, datasets, snapshots );
    }
}
