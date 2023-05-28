// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Collections.Immutable;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using NLog;

namespace Sanoid.Interop.Zfs;

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
    ///     An <see cref="ImmutableSortedSet{T}" /> of <see langword="string" />s, each representing the ZFS path of a dataset on the system.
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
    bool ZfsSnapshot( string snapshotName )
    {
        if ( string.IsNullOrWhiteSpace( snapshotName ) )
        {
            return false;
        }

        MatchCollection matches = ZfsIdentifierRegexes.SnapshotNameRegex( ).Matches( snapshotName );

        if ( matches.Count == 0 )
        {
            return false;
        }

        foreach ( Match match in matches )
        {
            Console.WriteLine(match.Value ?? "null");
        }

        return true;
    }

    /// <summary>
    ///     Gets a <see cref="Dictionary{TKey,TValue}"/> of &lt;<see langword="string"/>,<see cref="ZfsProperty"/>&gt; for <paramref name="zfsObjectName"/>
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If an invalid or uninitialized value is provided for paramref name="kind" />.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    ///     If <paramref name="zfsObjectName" /> is null, empty, or only whitespace
    /// </exception>
    /// <exception cref="InvalidOperationException">If the process execution threw this exception.</exception>
    /// <exception cref="ArgumentException">If name validation fails for <paramref name="zfsObjectName" /></exception>
    public Dictionary<string, ZfsProperty> GetZfsProperties( ZfsObjectKind kind, string zfsObjectName, bool sanoidOnly = true );
}
