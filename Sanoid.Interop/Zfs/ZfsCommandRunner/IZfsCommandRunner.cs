// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Text.Json;
using NLog;
using Sanoid.Interop.Libc.Enums;
using Sanoid.Interop.Zfs.ZfsTypes;
using Sanoid.Settings.Settings;

namespace Sanoid.Interop.Zfs.ZfsCommandRunner;

/// <summary>
///     Base class for classes that call native ZFS utilities from the system.
/// </summary>
/// <remarks>
///     Default implementations of command functions return mocked values.
/// </remarks>
public interface IZfsCommandRunner
{
    /// <summary>
    ///     Gets a list of ZFS datasets (filesystems and volumes)
    /// </summary>
    /// <returns>
    ///     An <see cref="ImmutableSortedSet{T}" /> of <see langword="string" />s, each representing the ZFS path of a dataset
    ///     on the system.
    /// </returns>
    ImmutableSortedSet<string> ZfsListAll( )
    {
        ImmutableSortedSet<string> dataSets = ImmutableSortedSet<string>.Empty.Union( new[] { "pool1", "pool1/dataset1", "pool1/dataset1/leaf", "pool1/dataset2", "pool1/dataset3", "pool1/zvol1" } );
        LogManager.GetCurrentClassLogger( ).Warn( "Running on windows. Returning fake datasets: {0}", JsonSerializer.Serialize( dataSets ) );
        return dataSets;
    }

    /// <summary>
    ///     Creates a zfs snapshot
    /// </summary>
    /// <returns>
    ///     A boolean value indicating whether the operation succeeded (ie no exceptions were thrown).
    /// </returns>
    public bool TakeSnapshot( Dataset ds, SnapshotPeriod snapshotPeriod, DateTimeOffset timestamp, SanoidSettings settings, out Snapshot snapshot );
    
    /// <summary>
    ///     Destroys a zfs snapshot
    /// </summary>
    /// <returns>
    ///     A boolean value indicating whether the operation succeeded (ie no exceptions were thrown).
    /// </returns>
    public bool DestroySnapshot( Dataset ds, Snapshot snapshot, SanoidSettings settings );

    /// <summary>
    ///     Sets the provided <see cref="ZfsProperty" /> values for <paramref name="zfsPath" />
    /// </summary>
    /// <param name="zfsPath">The fully-qualified path to operate on</param>
    /// <param name="properties">A parameterized array of <see cref="ZfsProperty" /> objects to set</param>
    /// <returns>
    ///     A <see langword="bool" /> indicating success or failure of the operation.
    /// </returns>
    public bool SetZfsProperties( bool dryRun, string zfsPath, params ZfsProperty[] properties );

    /// <summary>
    ///     Gets all dataset configuration from zfs
    /// </summary>
    /// <returns>
    ///     A <see cref="Dictionary{TKey,TValue}" /> of <see langword="string" /> to <see cref="Dataset" /> of all datasets in
    ///     zfs, with sanoid.net properties populated
    /// </returns>
    public Dictionary<string, Dataset> GetZfsDatasetConfiguration( string args = " -r" );

    /// <summary>
    ///     Gets configuration defined at all pool roots
    /// </summary>
    /// <returns>
    ///     A <see cref="Dictionary{TKey,TValue}" /> of <see langword="string" /> to <see cref="Dataset" /> of pool root
    ///     datasets in
    ///     zfs, with sanoid.net properties populated
    /// </returns>
    public ConcurrentDictionary<string, Dataset> GetPoolRootsWithAllRequiredSanoidProperties( );

    /// <summary>
    ///     Gets all snapshots from zfs
    /// </summary>
    /// <param name="datasets"></param>
    /// <returns>
    ///     A <see cref="Dictionary{TKey,TValue}" /> of <see langword="string" /> to <see cref="Snapshot" /> of all snapshots
    ///     in all pools that were taken by Sanoid.net
    /// </returns>
    public Dictionary<string, Snapshot> GetZfsSanoidSnapshots( ref Dictionary<string, Dataset> datasets );

    /// <summary>
    /// Gets everything Sanoid.net cares about from ZFS, via a call to zfs list with all known properties
    /// </summary>
    /// <param name="settings"></param>
    /// <returns>A tuple containing an exit status and a dictionary of all datasets found, along with their properties and dependent snapshots that Sanoid.net created</returns>
    public (Errno status, ConcurrentDictionary<string, Dataset> datasets) GetFullDatasetConfiguration( SanoidSettings settings );
}
