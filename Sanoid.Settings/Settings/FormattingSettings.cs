﻿// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

namespace Sanoid.Settings.Settings;

public sealed class FormattingSettings
{
    public required string ComponentSeparator { get; set; }
    public required string DailySuffix { get; set; }
    public required string FrequentSuffix { get; set; }
    public required string HourlySuffix { get; set; }
    public required string MonthlySuffix { get; set; }
    public required string Prefix { get; set; }
    public required string TimestampFormatString { get; set; }
    public required string WeeklySuffix { get; set; }
    public required string YearlySuffix { get; set; }

    public string GenerateFullSnapshotName( string datasetName, SnapshotPeriod period, DateTimeOffset timestamp )
    {
        return $"{datasetName}@{GenerateShortSnapshotName( period, timestamp )}";
    }

    public string GenerateShortSnapshotName( SnapshotPeriod period, DateTimeOffset timestamp )
    {
        return $"{Prefix}{ComponentSeparator}{timestamp.ToString( TimestampFormatString )}{ComponentSeparator}{period switch
        {
            SnapshotPeriod.Temporary => "temporary",
            SnapshotPeriod.Frequent => FrequentSuffix,
            SnapshotPeriod.Hourly => HourlySuffix,
            SnapshotPeriod.Daily => DailySuffix,
            SnapshotPeriod.Weekly => WeeklySuffix,
            SnapshotPeriod.Monthly => MonthlySuffix,
            SnapshotPeriod.Yearly => YearlySuffix,
            SnapshotPeriod.Manual => "manual",
            _ => throw new ArgumentOutOfRangeException( nameof( period ), period, null )
        }}";
    }
}
