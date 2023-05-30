﻿// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using Sanoid.Interop.Zfs.ZfsTypes;

namespace Sanoid.Interop.Zfs.ZfsCommandRunner;

internal class DummyZfsCommandRunner : ZfsCommandRunnerBase
{
    /// <inheritdoc />
    public override bool TakeSnapshot( string snapshotName )
    {
        return true;
    }

    /// <inheritdoc />
    public override Dictionary<string, ZfsProperty> GetZfsProperties( ZfsObjectKind kind, string zfsObjectName, bool sanoidOnly = true )
    {
        return new( );
    }

    /// <inheritdoc />
    public override bool SetZfsProperty( string zfsPath, params ZfsProperty[] properties )
    {
        return true;
    }
}