// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Common.Zfs;

public static class ZfsListObjectTypesExtensions
{
    public static string ToStringForCommandLine( this ZfsListObjectTypes value )
    {
        return value.ToString( ).Replace( " ", "" ).ToLower( );
    }
}