// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using NLog;
using SnapsInAZfs.Interop.Zfs.ZfsTypes.Validation;
using SnapsInAZfs.Settings.Settings;

namespace SnapsInAZfs.Interop.Zfs.ZfsTypes;

public record ZfsRecord
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger( );

    public ZfsRecord( string Name, string Kind, ZfsRecord? PoolRoot = null )
    {
        this.Name = Name;
        IsPoolRoot = PoolRoot is null;
        PoolRoot ??= this;
        this.Kind = Kind;
        this.PoolRoot = PoolRoot;
        NameValidatorRegex = Kind switch
        {
            ZfsPropertyValueConstants.FileSystem => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsPropertyValueConstants.Volume => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsPropertyValueConstants.Snapshot => ZfsIdentifierRegexes.SnapshotNameRegex( ),
            _ => throw new InvalidOperationException( "Unknown type of object specified for ZfsIdentifierValidator." )
        };
    }

    public ZfsProperty<bool> Enabled { get; private set; } = new( ZfsPropertyNames.EnabledPropertyName, false, "local" );
    public bool IsPoolRoot { get; }

    public IZfsProperty this[ string propName ]
    {
        get
        {
            ArgumentException.ThrowIfNullOrEmpty( propName );
            return propName switch
            {
                ZfsPropertyNames.EnabledPropertyName => Enabled,
                ZfsPropertyNames.TakeSnapshotsPropertyName => TakeSnapshots,
                ZfsPropertyNames.PruneSnapshotsPropertyName => PruneSnapshots,
                ZfsPropertyNames.RecursionPropertyName => Recursion,
                ZfsPropertyNames.TemplatePropertyName => Template,
                ZfsPropertyNames.SnapshotRetentionFrequentPropertyName => SnapshotRetentionFrequent,
                ZfsPropertyNames.SnapshotRetentionHourlyPropertyName => SnapshotRetentionHourly,
                ZfsPropertyNames.SnapshotRetentionDailyPropertyName => SnapshotRetentionDaily,
                ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName => SnapshotRetentionWeekly,
                ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName => SnapshotRetentionMonthly,
                ZfsPropertyNames.SnapshotRetentionYearlyPropertyName => SnapshotRetentionYearly,
                ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName => SnapshotRetentionPruneDeferral,
                ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName => LastFrequentSnapshotTimestamp,
                ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName => LastHourlySnapshotTimestamp,
                ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName => LastDailySnapshotTimestamp,
                ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName => LastWeeklySnapshotTimestamp,
                ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName => LastMonthlySnapshotTimestamp,
                ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName => LastYearlySnapshotTimestamp,
                _ => throw new ArgumentOutOfRangeException( nameof( propName ) )
            };
        }
    }

    public string Kind { get; }

    public ZfsProperty<DateTimeOffset> LastDailySnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastFrequentSnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastHourlySnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastMonthlySnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastWeeklySnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );
    public ZfsProperty<DateTimeOffset> LastYearlySnapshotTimestamp { get; private set; } = new( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, "local" );

    public string Name { get; }

    [JsonIgnore]
    public ZfsRecord PoolRoot { get; init; }

    public int PoolUsedCapacity { get; set; }
    public ZfsProperty<bool> PruneSnapshots { get; protected set; } = new( ZfsPropertyNames.PruneSnapshotsPropertyName, false, ZfsPropertySourceConstants.Local );
    public ZfsProperty<string> Recursion { get; protected set; } = new( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local );
    public ZfsProperty<int> SnapshotRetentionDaily { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, -1, ZfsPropertySourceConstants.Local );
    public ZfsProperty<int> SnapshotRetentionFrequent { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, -1, ZfsPropertySourceConstants.Local );
    public ZfsProperty<int> SnapshotRetentionHourly { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, -1, ZfsPropertySourceConstants.Local );
    public ZfsProperty<int> SnapshotRetentionMonthly { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, -1, ZfsPropertySourceConstants.Local );
    public ZfsProperty<int> SnapshotRetentionPruneDeferral { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0, ZfsPropertySourceConstants.Local );
    public ZfsProperty<int> SnapshotRetentionWeekly { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, -1, ZfsPropertySourceConstants.Local );
    public ZfsProperty<int> SnapshotRetentionYearly { get; private set; } = new( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, -1, ZfsPropertySourceConstants.Local );

    public ConcurrentDictionary<string, SnapshotRecord> Snapshots { get; } = new( );
    public ZfsProperty<bool> TakeSnapshots { get; private set; } = new( ZfsPropertyNames.TakeSnapshotsPropertyName, false, ZfsPropertySourceConstants.Local );
    public ZfsProperty<string> Template { get; private set; } = new( ZfsPropertyNames.TemplatePropertyName, "default", ZfsPropertySourceConstants.Local );

    [JsonIgnore]
    internal Regex NameValidatorRegex { get; }

    public SnapshotRecord AddSnapshot( SnapshotRecord snap )
    {
        Logger.Trace( "Adding snapshot {0} to {1} {2}", snap.Name, Kind, Name );
        Snapshots[ snap.Name ] = snap;
        return snap;
    }

    public void Deconstruct( out string name, out string kind, out bool isPoolRoot, out ZfsRecord poolRoot )
    {
        name = Name;
        kind = Kind;
        isPoolRoot = IsPoolRoot;
        poolRoot = PoolRoot;
    }

    public List<SnapshotRecord> GetSnapshotsToPrune( )
    {
        Logger.Debug( "Getting list of snapshots to prune for dataset {0}", Name );
        if ( !Enabled.Value )
        {
            Logger.Debug( "Dataset {0} is disabled. Skipping pruning", Name );
            return new( );
        }

        Logger.Debug( "Checking prune deferral setting for dataset {0}", Name );
        if ( SnapshotRetentionPruneDeferral.Value != 0 && PoolUsedCapacity < SnapshotRetentionPruneDeferral.Value )
        {
            Logger.Info( "Pool used capacity for {0} ({1}%) is below prune deferral threshold of {2}%. Skipping pruning of {0}", Name, PoolUsedCapacity, SnapshotRetentionPruneDeferral.Value );
            return new( );
        }

        if ( SnapshotRetentionPruneDeferral.Value == 0 )
        {
            Logger.Debug( "Prune deferral not enabled for {0}", Name );
        }

        List<SnapshotRecord> snapshotsToPrune = new( );
        List<SnapshotRecord> snapshotsSetForPruning = Snapshots.Where( kvp => kvp.Value.PruneSnapshots.Value && kvp.Value.Period.Value.Equals( SnapshotPeriod.Frequent ) ).Select( kvp => kvp.Value ).ToList( );
        Logger.Debug( "Frequent snapshots of {0} available for pruning: {1}", Name, string.Join( ',', snapshotsSetForPruning.Select( s => s.Name ) ) );
        int numberToPrune;
        if ( snapshotsSetForPruning.Count > SnapshotRetentionFrequent.Value )
        {
            numberToPrune = snapshotsSetForPruning.Count - SnapshotRetentionFrequent.Value;
            Logger.Debug( "Need to prune oldest {0} frequent snapshots from dataset {1}", numberToPrune, Name );
            snapshotsSetForPruning.Sort( );
            for ( int i = 0; i < numberToPrune; i++ )
            {
                SnapshotRecord frequentSnapshot = snapshotsSetForPruning[ i ];
                Logger.Debug( "Adding snapshot {0} to prune list", frequentSnapshot.Name );
                snapshotsToPrune.Add( frequentSnapshot );
            }
        }

        snapshotsSetForPruning.Clear( );
        snapshotsSetForPruning = Snapshots.Where( kvp => kvp.Value.PruneSnapshots.Value && kvp.Value.Period.Value.Equals( SnapshotPeriod.Hourly ) ).Select( kvp => kvp.Value ).ToList( );
        Logger.Debug( "Hourly snapshots of {0} available for pruning: {1}", Name, snapshotsSetForPruning.Select( s => s.Name ).ToCommaSeparatedSingleLineString( ) );
        if ( snapshotsSetForPruning.Count > SnapshotRetentionHourly.Value )
        {
            numberToPrune = snapshotsSetForPruning.Count - SnapshotRetentionHourly.Value;
            Logger.Debug( "Need to prune oldest {0} hourly snapshots from dataset {1}", numberToPrune, Name );
            snapshotsSetForPruning.Sort( );
            for ( int i = 0; i < numberToPrune; i++ )
            {
                SnapshotRecord hourlySnapshot = snapshotsSetForPruning[ i ];
                Logger.Debug( "Adding snapshot {0} to prune list", hourlySnapshot.Name );
                snapshotsToPrune.Add( hourlySnapshot );
            }
        }

        snapshotsSetForPruning.Clear( );
        snapshotsSetForPruning = Snapshots.Where( kvp => kvp.Value.PruneSnapshots.Value && kvp.Value.Period.Value.Equals( SnapshotPeriod.Daily ) ).Select( kvp => kvp.Value ).ToList( );
        Logger.Debug( "Daily snapshots of {0} available for pruning: {1}", Name, snapshotsSetForPruning.Select( s => s.Name ).ToCommaSeparatedSingleLineString( ) );
        if ( snapshotsSetForPruning.Count > SnapshotRetentionDaily.Value )
        {
            numberToPrune = snapshotsSetForPruning.Count - SnapshotRetentionDaily.Value;
            Logger.Debug( "Need to prune oldest {0} daily snapshots from dataset {1}", numberToPrune, Name );
            snapshotsSetForPruning.Sort( );
            for ( int i = 0; i < numberToPrune; i++ )
            {
                SnapshotRecord dailySnapshot = snapshotsSetForPruning[ i ];
                Logger.Debug( "Adding snapshot {0} to prune list", dailySnapshot.Name );
                snapshotsToPrune.Add( dailySnapshot );
            }
        }

        snapshotsSetForPruning.Clear( );
        snapshotsSetForPruning = Snapshots.Where( kvp => kvp.Value.PruneSnapshots.Value && kvp.Value.Period.Value.Equals( SnapshotPeriod.Weekly ) ).Select( kvp => kvp.Value ).ToList( );
        Logger.Debug( "Weekly snapshots of {0} available for pruning: {1}", Name, snapshotsSetForPruning.Select( s => s.Name ).ToCommaSeparatedSingleLineString( ) );
        if ( snapshotsSetForPruning.Count > SnapshotRetentionWeekly.Value )
        {
            numberToPrune = snapshotsSetForPruning.Count - SnapshotRetentionWeekly.Value;
            Logger.Debug( "Need to prune oldest {0} weekly snapshots from dataset {1}", numberToPrune, Name );
            snapshotsSetForPruning.Sort( );
            for ( int i = 0; i < numberToPrune; i++ )
            {
                SnapshotRecord weeklySnapshot = snapshotsSetForPruning[ i ];
                Logger.Debug( "Adding snapshot {0} to prune list", weeklySnapshot.Name );
                snapshotsToPrune.Add( weeklySnapshot );
            }
        }

        snapshotsSetForPruning.Clear( );
        snapshotsSetForPruning = Snapshots.Where( kvp => kvp.Value.PruneSnapshots.Value && kvp.Value.Period.Value.Equals( SnapshotPeriod.Monthly ) ).Select( kvp => kvp.Value ).ToList( );
        Logger.Debug( "Monthly snapshots of {0} available for pruning: {1}", Name, snapshotsSetForPruning.Select( s => s.Name ).ToCommaSeparatedSingleLineString( ) );
        if ( snapshotsSetForPruning.Count > SnapshotRetentionMonthly.Value )
        {
            numberToPrune = snapshotsSetForPruning.Count - SnapshotRetentionMonthly.Value;
            Logger.Debug( "Need to prune oldest {0} monthly snapshots from dataset {1}", numberToPrune, Name );
            snapshotsSetForPruning.Sort( );
            for ( int i = 0; i < numberToPrune; i++ )
            {
                SnapshotRecord monthlySnapshot = snapshotsSetForPruning[ i ];
                Logger.Debug( "Adding snapshot {0} to prune list", monthlySnapshot.Name );
                snapshotsToPrune.Add( monthlySnapshot );
            }
        }

        snapshotsSetForPruning.Clear( );
        snapshotsSetForPruning = Snapshots.Where( kvp => kvp.Value.PruneSnapshots.Value && kvp.Value.Period.Value.Equals( SnapshotPeriod.Yearly ) ).Select( kvp => kvp.Value ).ToList( );
        Logger.Debug( "Yearly snapshots of {0} available for pruning: {1}", Name, snapshotsSetForPruning.Select( s => s.Name ).ToCommaSeparatedSingleLineString( ) );
        // Don't do this, so these all look the same
        // ReSharper disable once InvertIf
        if ( snapshotsSetForPruning.Count > SnapshotRetentionYearly.Value )
        {
            numberToPrune = snapshotsSetForPruning.Count - SnapshotRetentionYearly.Value;
            Logger.Debug( "Need to prune oldest {0} yearly snapshots from dataset {1}", numberToPrune, Name );
            snapshotsSetForPruning.Sort( );
            for ( int i = 0; i < numberToPrune; i++ )
            {
                SnapshotRecord yearlySnapshot = snapshotsSetForPruning[ i ];
                Logger.Debug( "Adding snapshot {0} to prune list", yearlySnapshot.Name );
                snapshotsToPrune.Add( yearlySnapshot );
            }
        }

        return snapshotsToPrune;
    }

    /// <summary>
    ///     Gets whether a daily snapshot is needed, according to the <paramref name="timestamp" /> and the properties defined
    ///     on the object
    /// </summary>
    /// <param name="timestamp">The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type</param>
    /// <returns>
    ///     A <see langword="bool" /> indicating whether ALL of the following conditions are met:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Snapshot retention settings define daily greater than 0</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="timestamp" /> is either more than one day ahead of the last daily
    ///                 snapshot OR the last daily snapshot is not in the same day of the year
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public bool IsDailySnapshotNeeded( DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no dailies
        if ( !SnapshotRetentionDaily.IsWanted( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if daily snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        TimeSpan timeSinceLastDailySnapshot = timestamp - LastDailySnapshotTimestamp.Value;
        bool atLeastOneDaySinceLastDailySnapshot = timeSinceLastDailySnapshot.TotalDays >= 1d;
        // Check if more than a day ago or if a different day of the year
        bool lastDailyOnDifferentDayOfYear = LastDailySnapshotTimestamp.Value.LocalDateTime.DayOfYear != timestamp.LocalDateTime.DayOfYear;
        bool dailySnapshotNeeded = atLeastOneDaySinceLastDailySnapshot || lastDailyOnDifferentDayOfYear;
        Logger.Debug( "Daily snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, dailySnapshotNeeded ? "" : "not " );
        return dailySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a frequent snapshot is needed, according to the provided <see cref="SnapshotTimingSettings" /> and
    ///     <paramref name="timestamp" />
    /// </summary>
    /// <param name="template">
    ///     The <see cref="SnapshotTimingSettings" /> object to check status against.
    /// </param>
    /// <param name="timestamp">The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type</param>
    /// <returns>
    ///     A <see langword="bool" /> indicating whether ALL of the following conditions are met:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Template snapshot retention settings define frequent greater than 0</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="timestamp" /> is either more than FrequentPeriod minutes ahead of the last frequent
    ///                 snapshot OR the last frequent snapshot is not in the same period of the hour
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public bool IsFrequentSnapshotNeeded( SnapshotTimingSettings template, DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no frequent
        if ( !SnapshotRetentionFrequent.IsWanted( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if frequent snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        int currentFrequentPeriodOfHour = template.GetPeriodOfHour( timestamp );
        int lastFrequentSnapshotPeriodOfHour = template.GetPeriodOfHour( LastFrequentSnapshotTimestamp.Value );
        double minutesSinceLastFrequentSnapshot = ( timestamp - LastFrequentSnapshotTimestamp.Value ).TotalMinutes;
        // Check if more than FrequentPeriod ago or if the period of the hour is different.
        bool frequentSnapshotNeeded = minutesSinceLastFrequentSnapshot >= template.FrequentPeriod || lastFrequentSnapshotPeriodOfHour != currentFrequentPeriodOfHour;
        Logger.Debug( "Frequent snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, frequentSnapshotNeeded ? "" : "not " );
        return frequentSnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether an hourly snapshot is needed, according to the provided <paramref name="timestamp" />
    /// </summary>
    /// <param name="timestamp">The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type</param>
    /// <returns>
    ///     A <see langword="bool" /> indicating whether ALL of the following conditions are met:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Snapshot retention settings define hourly greater than 0</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="timestamp" /> is either more than one hour ahead of the last hourly
    ///                 snapshot OR the last hourly snapshot is not in the same hour
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    public bool IsHourlySnapshotNeeded( DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no hourlies
        if ( !SnapshotRetentionHourly.IsWanted( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if hourly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        TimeSpan timeSinceLastHourlySnapshot = timestamp - LastHourlySnapshotTimestamp.Value;
        bool atLeastOneHourSinceLastHourlySnapshot = timeSinceLastHourlySnapshot.TotalHours >= 1d;
        // Check if more than an hour ago or if hour is different
        bool hourlySnapshotNeeded = atLeastOneHourSinceLastHourlySnapshot
                                    || LastHourlySnapshotTimestamp.Value.LocalDateTime.Hour != timestamp.LocalDateTime.Hour;
        Logger.Debug( "Hourly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, hourlySnapshotNeeded ? "" : "not " );
        return hourlySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a monthly snapshot is needed
    /// </summary>
    /// <param name="timestamp">The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type</param>
    /// <returns>
    ///     A <see langword="bool" /> indicating whether ALL of the following conditions are met:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Snapshot retention settings define monthly greater than 0</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="timestamp" /> is either in a different month than the last monthly snapshot OR the last
    ///                 monthly snapshot is in a different year
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     Uses culture-aware definitions of months, using the executing user's culture.
    /// </remarks>
    public bool IsMonthlySnapshotNeeded( DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no monthlies
        if ( !SnapshotRetentionMonthly.IsWanted( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if monthly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        int lastMonthlySnapshotMonth = CultureInfo.CurrentCulture.Calendar.GetMonth( LastMonthlySnapshotTimestamp.Value.LocalDateTime );
        int currentMonth = CultureInfo.CurrentCulture.Calendar.GetMonth( timestamp.LocalDateTime );
        int lastMonthlySnapshotYear = CultureInfo.CurrentCulture.Calendar.GetYear( LastMonthlySnapshotTimestamp.Value.LocalDateTime );
        int currentYear = CultureInfo.CurrentCulture.Calendar.GetYear( timestamp.LocalDateTime );
        // Check if the last monthly snapshot was in a different month or if same month but different year
        bool lastMonthlySnapshotInDifferentMonth = lastMonthlySnapshotMonth != currentMonth;
        bool lastMonthlySnapshotInDifferentYear = currentYear != lastMonthlySnapshotYear;
        bool monthlySnapshotNeeded = lastMonthlySnapshotInDifferentMonth || lastMonthlySnapshotInDifferentYear;
        Logger.Debug( "Monthly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, monthlySnapshotNeeded ? "" : "not " );
        return monthlySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a weekly snapshot is needed, according to the provided <see cref="SnapshotTimingSettings" /> and
    ///     <paramref name="timestamp" />
    /// </summary>
    /// <param name="template">
    ///     The <see cref="SnapshotTimingSettings" /> object to check status against.
    /// </param>
    /// <param name="timestamp">The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type</param>
    /// <returns>
    ///     A <see langword="bool" /> indicating whether ALL of the following conditions are met:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>Snapshot retention settings define weekly greater than 0</description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <paramref name="timestamp" /> is either more than 7 days ahead of the last weekly
    ///                 snapshot OR the last weekly snapshot is not in the same week of the year
    ///             </description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     Uses culture-aware definitions of week numbers, using the executing user's culture, and treating the day of the
    ///     week specified in settings for weekly snapshots as the "first" day of the week, for week numbering purposes
    /// </remarks>
    public bool IsWeeklySnapshotNeeded( SnapshotTimingSettings template, DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no weeklies
        if ( !SnapshotRetentionWeekly.IsWanted( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if weekly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        TimeSpan timeSinceLastWeeklySnapshot = timestamp - LastWeeklySnapshotTimestamp.Value;
        bool atLeastOneWeekSinceLastWeeklySnapshot = timeSinceLastWeeklySnapshot.TotalDays >= 7d;
        // Check if more than a week ago or if the week number is different by local rules, using the chosen day as the first day of the week
        int lastWeeklySnapshotWeekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear( LastWeeklySnapshotTimestamp.Value.LocalDateTime, CalendarWeekRule.FirstDay, template.WeeklyDay );
        int currentWeekNumber = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear( timestamp.LocalDateTime, CalendarWeekRule.FirstDay, template.WeeklyDay );
        bool weeklySnapshotNeeded = atLeastOneWeekSinceLastWeeklySnapshot || currentWeekNumber != lastWeeklySnapshotWeekNumber;
        Logger.Debug( "Weekly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, weeklySnapshotNeeded ? "" : "not " );
        return weeklySnapshotNeeded;
    }

    /// <summary>
    ///     Gets whether a yearly snapshot is needed, according to the <paramref name="timestamp" /> and properties defined on
    ///     the <see cref="ZfsRecord" />
    /// </summary>
    /// <param name="timestamp">The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type</param>
    /// <returns>
    ///     A <see langword="bool" /> indicating whether the last yearly snapshot is in the same year as
    ///     <paramref name="timestamp" />
    /// </returns>
    /// <remarks>
    ///     Uses culture-aware definitions of years, using the executing user's culture.
    /// </remarks>
    public bool IsYearlySnapshotNeeded( DateTimeOffset timestamp )
    {
        //Exit early if retention settings say no monthlies
        if ( !SnapshotRetentionYearly.IsWanted( ) )
        {
            return false;
        }

        // Yes, this can all be done in-line, but this is easier to debug, is more explicit, and the compiler is
        // going to optimize it all away anyway.
        Logger.Trace( "Checking if yearly snapshot is needed for dataset {0} at timestamp {1:O}", Name, timestamp );
        int lastYearlySnapshotYear = CultureInfo.CurrentCulture.Calendar.GetYear( LastYearlySnapshotTimestamp.Value.LocalDateTime );
        int currentYear = CultureInfo.CurrentCulture.Calendar.GetYear( timestamp.LocalDateTime );
        // Check if the last yearly snapshot was in a different year
        bool yearlySnapshotNeeded = lastYearlySnapshotYear != currentYear;
        Logger.Debug( "Yearly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, yearlySnapshotNeeded ? "" : "not " );
        return yearlySnapshotNeeded;
    }

    /// <exception cref="FormatException">
    ///     <paramref name="propertyValue" /> is not a valid string representation of the target
    ///     property value type.
    /// </exception>
    /// <exception cref="ArgumentNullException"><paramref name="propertyValue" /> is <see langword="null" />.</exception>
    /// <exception cref="ArgumentException">
    ///     For <see cref="DateTimeOffset" /> properties, the offset is greater than 14 hours
    ///     or less than -14 hours.
    /// </exception>
    /// <exception cref="OverflowException">
    ///     For <see langword="int" /> properties, <paramref name="propertyValue" /> represents
    ///     a number less than <see cref="int.MinValue" /> or greater than <see cref="int.MaxValue" />.
    /// </exception>
    public IZfsProperty UpdateProperty( string propertyName, string propertyValue, string propertySource )
    {
        return propertyName switch
        {
            ZfsPropertyNames.EnabledPropertyName => UpdateProperty( propertyName, bool.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.TakeSnapshotsPropertyName => UpdateProperty( propertyName, bool.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.PruneSnapshotsPropertyName => UpdateProperty( propertyName, bool.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.TemplatePropertyName => Template = Template with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.RecursionPropertyName => Recursion = Recursion with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName => UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName => UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName => UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName => UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName => UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName => UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.SnapshotRetentionFrequentPropertyName => UpdateProperty( propertyName, int.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.SnapshotRetentionHourlyPropertyName => UpdateProperty( propertyName, int.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.SnapshotRetentionDailyPropertyName => UpdateProperty( propertyName, int.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName => UpdateProperty( propertyName, int.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName => UpdateProperty( propertyName, int.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.SnapshotRetentionYearlyPropertyName => UpdateProperty( propertyName, int.Parse( propertyValue ), propertySource ),
            ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName => UpdateProperty( propertyName, int.Parse( propertyValue ), propertySource ),
            _ => throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported property" )
        };
    }

    public ZfsProperty<bool> UpdateProperty( string propertyName, bool propertyValue, string propertySource )
    {
        return propertyName switch
        {
            ZfsPropertyNames.EnabledPropertyName => Enabled = Enabled with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.TakeSnapshotsPropertyName => TakeSnapshots = TakeSnapshots with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.PruneSnapshotsPropertyName => PruneSnapshots = PruneSnapshots with { Value = propertyValue, Source = propertySource },
            _ => throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported boolean property" )
        };
    }

    public ZfsProperty<DateTimeOffset> UpdateProperty( string propertyName, DateTimeOffset propertyValue, string propertySource )
    {
        return propertyName switch
        {
            ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName => LastFrequentSnapshotTimestamp = LastFrequentSnapshotTimestamp with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName => LastHourlySnapshotTimestamp = LastHourlySnapshotTimestamp with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName => LastDailySnapshotTimestamp = LastDailySnapshotTimestamp with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName => LastWeeklySnapshotTimestamp = LastWeeklySnapshotTimestamp with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName => LastMonthlySnapshotTimestamp = LastMonthlySnapshotTimestamp with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName => LastYearlySnapshotTimestamp = LastYearlySnapshotTimestamp with { Value = propertyValue, Source = propertySource },
            _ => throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported DateTimeOffset property" )
        };
    }

    public ZfsProperty<int> UpdateProperty( string propertyName, int propertyValue, string propertySource )
    {
        return propertyName switch
        {
            ZfsPropertyNames.SnapshotRetentionFrequentPropertyName => SnapshotRetentionFrequent = SnapshotRetentionFrequent with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.SnapshotRetentionHourlyPropertyName => SnapshotRetentionHourly = SnapshotRetentionHourly with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.SnapshotRetentionDailyPropertyName => SnapshotRetentionDaily = SnapshotRetentionDaily with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName => SnapshotRetentionWeekly = SnapshotRetentionWeekly with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName => SnapshotRetentionMonthly = SnapshotRetentionMonthly with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.SnapshotRetentionYearlyPropertyName => SnapshotRetentionYearly = SnapshotRetentionYearly with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName => SnapshotRetentionPruneDeferral = SnapshotRetentionPruneDeferral with { Value = propertyValue, Source = propertySource },
            _ => throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported int property" )
        };
    }

    public static bool ValidateName( string kind, string name, Regex? validatorRegex = null )
    {
        Logger.Debug( "Validating name \"{0}\"", name );
        if ( string.IsNullOrWhiteSpace( name ) )
        {
            throw new ArgumentNullException( nameof( name ), "name must be a non-null, non-empty, non-whitespace string" );
        }

        if ( name.Length > 255 )
        {
            throw new ArgumentOutOfRangeException( nameof( name ), "name must be 255 characters or less" );
        }

        // Sure they are... They're handled by the default case.
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        validatorRegex ??= kind switch
        {
            ZfsPropertyValueConstants.FileSystem => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsPropertyValueConstants.Volume => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsPropertyValueConstants.Snapshot => ZfsIdentifierRegexes.SnapshotNameRegex( ),
            _ => throw new ArgumentOutOfRangeException( nameof( kind ), "Unknown type of object specified to ValidateName." )
        };

        MatchCollection matches = validatorRegex.Matches( name );

        if ( matches.Count == 0 )
        {
            return false;
        }

        Logger.Trace( "Checking regex matches for {0}", name );
        // No matter which kind was specified, the pool group should exist and be a match
        for ( int matchIndex = 0; matchIndex < matches.Count; matchIndex++ )
        {
            Match match = matches[ matchIndex ];
            Logger.Trace( "Inspecting match {0}", match.Value );
            if ( match.Success )
            {
                continue;
            }

            Logger.Error( "Name of {0} {1} is invalid", kind, name );
            return false;
        }

        Logger.Debug( "Name of {0} {1} is valid", kind, name );

        return true;
    }

    protected internal bool ValidateName( string name )
    {
        return ValidateName( Kind, name, NameValidatorRegex );
    }

    protected internal bool ValidateName( )
    {
        return ValidateName( Name );
    }
}

public record SnapshotRecord : ZfsRecord, IComparable<SnapshotRecord>
{
    public SnapshotRecord( string name, ZfsRecord parentDataset ) : base( name, "snapshot", parentDataset.PoolRoot )
    {
        ParentDataset = parentDataset;
        SnapshotName = new( ZfsPropertyNames.SnapshotNamePropertyName, name, ZfsPropertySourceConstants.Local );
    }

    public SnapshotRecord( string name, DateTimeOffset timestamp, ZfsRecord parentDataset ) : this( name, parentDataset )
    {
        Timestamp = new( ZfsPropertyNames.SnapshotTimestampPropertyName, timestamp, ZfsPropertySourceConstants.Local );
    }

    public SnapshotRecord( string name, bool pruneSnapshots, SnapshotPeriod period, DateTimeOffset timestamp, ZfsRecord parentDataset ) : this( name, timestamp, parentDataset )
    {
        PruneSnapshots = new( ZfsPropertyNames.PruneSnapshotsPropertyName, pruneSnapshots, ZfsPropertySourceConstants.ZfsList );
        Period = new( ZfsPropertyNames.SnapshotPeriodPropertyName, period, ZfsPropertySourceConstants.Local );
    }

    public ZfsRecord ParentDataset { get; }

    public ZfsProperty<SnapshotPeriod> Period { get; private set; } = new( ZfsPropertyNames.SnapshotPeriodPropertyName, SnapshotPeriod.Frequent, ZfsPropertySourceConstants.Local );
    public ZfsProperty<string> SnapshotName { get; private set; } = new( ZfsPropertyNames.SnapshotNamePropertyName, "", ZfsPropertySourceConstants.Local );
    public ZfsProperty<DateTimeOffset> Timestamp { get; private set; } = new( ZfsPropertyNames.SnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local );

    /// <summary>
    ///     Compares the current instance with another <see cref="SnapshotRecord" /> and returns an integer that indicates
    ///     whether the current instance precedes, follows, or occurs in the same position in the sort order as the other
    ///     <see cref="SnapshotRecord" />.
    /// </summary>
    /// <param name="other">Another <see cref="SnapshotRecord" /> to compare with this instance.</param>
    /// <returns>
    ///     A value that indicates the relative order of the <see cref="SnapshotRecord" />s being compared. The return value
    ///     has
    ///     these meanings:
    ///     <list type="table">
    ///         <listheader>
    ///             <term> Value</term><description> Meaning</description>
    ///         </listheader>
    ///         <item>
    ///             <term> Less than zero</term>
    ///             <description> This instance precedes <paramref name="other" /> in the sort order.</description>
    ///         </item>
    ///         <item>
    ///             <term> Zero</term>
    ///             <description> This instance occurs in the same position in the sort order as <paramref name="other" />.</description>
    ///         </item>
    ///         <item>
    ///             <term> Greater than zero</term>
    ///             <description> This instance follows <paramref name="other" /> in the sort order.</description>
    ///         </item>
    ///     </list>
    /// </returns>
    /// <remarks>
    ///     Sort order is as follows:
    ///     <list type="number">
    ///         <listheader>
    ///             <term>Condition</term><description>Result</description>
    ///         </listheader>
    ///         <item>
    ///             <term>Other <see cref="SnapshotRecord" /> is null or has a null <see cref="Timestamp" /></term>
    ///             <description>This <see cref="SnapshotRecord" /> precedes <paramref name="other" /> in the sort order.</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Timestamp" /> of this <see cref="SnapshotRecord" /> is null</term>
    ///             <description>This <see cref="SnapshotRecord" /> follows <paramref name="other" /> in the sort order.</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Timestamp" /> of each <see cref="SnapshotRecord" /> is different</term>
    ///             <description>
    ///                 Sort by <see cref="Timestamp" />, using system rules for the <see cref="DateTimeOffset" />
    ///                 type
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Period" /> of each <see cref="SnapshotRecord" /> is different</term>
    ///             <description>Delegate sort order to <see cref="SnapshotPeriod" />, using <see cref="Period" /> of each</description>
    ///         </item>
    ///         <item>
    ///             <term><see cref="Period" />s of both <see cref="SnapshotRecord" />s are equal</term>
    ///             <description>Sort by <see cref="ZfsRecord.Name" /></description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public int CompareTo( SnapshotRecord? other )
    {
        // If the other snapshot is null, consider this snapshot earlier rank
        if ( other?.Timestamp is null )
        {
            return -1;
        }

        // If timestamps are different, sort on timestamps
        if ( other.Timestamp.Value != Timestamp.Value )
        {
            return Timestamp.Value.CompareTo( other.Timestamp.Value );
        }

        // If timestamps are different, sort on period
        return Period.Value != other.Period.Value
            ? Period.Value.CompareTo( other.Period.Value )
            :
            // If periods are the same, sort by name
            Name.CompareTo( other.Name );
    }

    public void Deconstruct( out string name, out ZfsRecord parentDataset )
    {
        name = Name;
        parentDataset = ParentDataset;
    }

    public static SnapshotRecord GetNewSnapshotObjectForCommandRunner( ZfsRecord ds, SnapshotPeriod period, DateTimeOffset timestamp, TemplateSettings template )
    {
        string snapshotName = template.GenerateFullSnapshotName( ds.Name, period.Kind, timestamp );
        return new( snapshotName, ds )
        {
            SnapshotName = new( ZfsPropertyNames.SnapshotNamePropertyName, snapshotName, ZfsPropertySourceConstants.Local ),
            Period = new( ZfsPropertyNames.SnapshotPeriodPropertyName, period, ZfsPropertySourceConstants.Local ),
            Timestamp = new( ZfsPropertyNames.SnapshotTimestampPropertyName, timestamp, ZfsPropertySourceConstants.Local ),
            Recursion = ds.Recursion with { Source = ZfsPropertySourceConstants.Local }
        };
    }

    public string GetSnapshotOptionsStringForZfsSnapshot( )
    {
        return $"-o {SnapshotName.SetString} -o {Period.SetString} -o {Timestamp.SetString} -o {Recursion.SetString}";
    }

    public new IZfsProperty UpdateProperty( string propertyName, string propertyValue, string propertySource )
    {
        return propertyName switch
        {
            ZfsPropertyNames.SnapshotNamePropertyName => SnapshotName = SnapshotName with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.SnapshotPeriodPropertyName => UpdateProperty( propertyName, (SnapshotPeriod)propertyValue, propertySource ),
            ZfsPropertyNames.SnapshotTimestampPropertyName => UpdateProperty( propertyName, DateTimeOffset.Parse( propertyValue ), propertySource ),
            _ => base.UpdateProperty( propertyName, propertyValue, propertySource )
        };
    }

    public ZfsProperty<SnapshotPeriod> UpdateProperty( string propertyName, SnapshotPeriod propertyValue, string propertySource )
    {
        return propertyName switch
        {
            ZfsPropertyNames.SnapshotPeriodPropertyName => Period = Period with { Value = propertyValue, Source = propertySource },
            _ => throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported SnapshotKind property" )
        };
    }

    public new ZfsProperty<DateTimeOffset> UpdateProperty( string propertyName, DateTimeOffset propertyValue, string propertySource )
    {
        return propertyName switch
        {
            ZfsPropertyNames.SnapshotTimestampPropertyName => Timestamp = Timestamp with { Value = propertyValue, Source = propertySource },
            _ => base.UpdateProperty( propertyName, propertyValue, propertySource )
        };
    }
}
