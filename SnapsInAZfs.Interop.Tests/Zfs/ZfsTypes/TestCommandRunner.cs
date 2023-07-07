﻿// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using NLog;
using SnapsInAZfs.Interop.Zfs.ZfsCommandRunner;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Settings.Settings;
using Terminal.Gui.Trees;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes;

public class TestCommandRunner : ZfsCommandRunnerBase
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    /// <inheritdoc />
    public override async Task<bool> DestroySnapshotAsync( Snapshot snapshot, SnapsInAZfsSettings settings )
    {
        throw new NotImplementedException( );
    }

    public override async Task GetDatasetsAndSnapshotsFromZfsAsync( SnapsInAZfsSettings settings, ConcurrentDictionary<string, ZfsRecord> datasets, ConcurrentDictionary<string, Snapshot> snapshots )
    {
        string propertiesString = IZfsProperty.KnownDatasetProperties.Union( IZfsProperty.KnownSnapshotProperties ).ToCommaSeparatedSingleLineString( );
        Logger.Debug( "Pretending to run zfs get type,{0},available,used -H -p -r -t filesystem,volume,snapshot", propertiesString );
        ConfiguredCancelableAsyncEnumerable<string> lineProvider = ZfsExecEnumeratorAsync( "get", "testData-WithSnapshotsToPrune.txt" ).ConfigureAwait( true );
        SortedDictionary<string, RawZfsObject> rawObjects = new( );
        await GetRawZfsObjectsAsync( lineProvider, rawObjects ).ConfigureAwait( true );
        ProcessRawObjects( rawObjects, datasets, snapshots );
        CheckAndUpdateLastSnapshotTimesForDatasets( settings, datasets );
    }

    /// <inheritdoc />
    public override async Task<ConcurrentDictionary<string, ConcurrentDictionary<string, bool>>> GetPoolRootsAndPropertyValiditiesAsync( )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override async Task<List<ITreeNode>> GetZfsObjectsForConfigConsoleTreeAsync( ConcurrentDictionary<string, ZfsRecord> baseDatasets, ConcurrentDictionary<string, ZfsRecord> treeDatasets )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override bool SetDefaultValuesForMissingZfsPropertiesOnPoolAsync( bool dryRun, string poolName, string[] propertyArray )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override bool SetZfsProperties( bool dryRun, string zfsPath, params IZfsProperty[] properties )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override bool SetZfsProperties( bool dryRun, string zfsPath, List<IZfsProperty> properties )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override bool TakeSnapshot( ZfsRecord ds, SnapshotPeriod period, DateTimeOffset timestamp, SnapsInAZfsSettings snapsInAZfsSettings, TemplateSettings datasetTemplate, out Snapshot? snapshot )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override IAsyncEnumerable<string> ZfsExecEnumeratorAsync( string verb, string args )
    {
        throw new NotImplementedException( );
    }

    /// <inheritdoc />
    public override IAsyncEnumerable<string> ZpoolExecEnumerator( string verb, string args )
    {
        throw new NotImplementedException( );
    }
}
