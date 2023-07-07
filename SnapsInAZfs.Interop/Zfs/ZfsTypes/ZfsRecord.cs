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

    protected ZfsRecord( string name, string kind, ZfsProperty<bool> enabled, ZfsProperty<bool> takeSnapshots, ZfsProperty<bool> pruneSnapshots, ZfsProperty<DateTimeOffset> lastFrequentSnapshotTimestamp, ZfsProperty<DateTimeOffset> lastHourlySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastDailySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastWeeklySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastMonthlySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastYearlySnapshotTimestamp, ZfsProperty<string> recursion, ZfsProperty<string> template, ZfsProperty<int> retentionFrequent, ZfsProperty<int> retentionHourly, ZfsProperty<int> retentionDaily, ZfsProperty<int> retentionWeekly, ZfsProperty<int> retentionMonthly, ZfsProperty<int> retentionYearly, ZfsProperty<int> retentionPruneDeferral, long bytesAvailable, long bytesUsed, ZfsRecord? parent = null )
    {
        Name = name;
        IsPoolRoot = parent is null;
        ParentDataset = parent ?? this;
        Kind = kind;
        NameValidatorRegex = kind switch
        {
            ZfsPropertyValueConstants.FileSystem => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsPropertyValueConstants.Volume => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsPropertyValueConstants.Snapshot => ZfsIdentifierRegexes.SnapshotNameRegex( ),
            _ => throw new InvalidOperationException( "Unknown type of object specified for ZfsIdentifierValidator." )
        };

        Enabled = enabled;
        TakeSnapshots = takeSnapshots;
        PruneSnapshots = pruneSnapshots;
        if ( lastFrequentSnapshotTimestamp.IsLocal )
        {
            LastFrequentSnapshotTimestamp = lastFrequentSnapshotTimestamp;
        }

        if ( lastHourlySnapshotTimestamp.IsLocal )
        {
            LastHourlySnapshotTimestamp = lastHourlySnapshotTimestamp;
        }

        if ( lastDailySnapshotTimestamp.IsLocal )
        {
            LastDailySnapshotTimestamp = lastDailySnapshotTimestamp;
        }

        if ( lastWeeklySnapshotTimestamp.IsLocal )
        {
            LastWeeklySnapshotTimestamp = lastWeeklySnapshotTimestamp;
        }

        if ( lastMonthlySnapshotTimestamp.IsLocal )
        {
            LastMonthlySnapshotTimestamp = lastMonthlySnapshotTimestamp;
        }

        if ( lastYearlySnapshotTimestamp.IsLocal )
        {
            LastYearlySnapshotTimestamp = lastYearlySnapshotTimestamp;
        }

        Recursion = recursion;
        Template = template;
        SnapshotRetentionFrequent = retentionFrequent;
        SnapshotRetentionHourly = retentionHourly;
        SnapshotRetentionDaily = retentionDaily;
        SnapshotRetentionWeekly = retentionWeekly;
        SnapshotRetentionMonthly = retentionMonthly;
        SnapshotRetentionYearly = retentionYearly;
        SnapshotRetentionPruneDeferral = retentionPruneDeferral;
        BytesAvailable = bytesAvailable;
        BytesUsed = bytesUsed;
    }

    public ZfsRecord( string Name, string Kind, ZfsRecord? parent = null )
    {
        this.Name = Name;
        IsPoolRoot = parent is null;
        ParentDataset = parent ?? this;
        this.Kind = Kind;
        NameValidatorRegex = Kind switch
        {
            ZfsPropertyValueConstants.FileSystem => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsPropertyValueConstants.Volume => ZfsIdentifierRegexes.DatasetNameRegex( ),
            ZfsPropertyValueConstants.Snapshot => ZfsIdentifierRegexes.SnapshotNameRegex( ),
            _ => throw new InvalidOperationException( "Unknown type of object specified for ZfsIdentifierValidator." )
        };

        if ( parent is not null )
        {
            Enabled = parent.Enabled.IsLocal ? parent.Enabled with { Source = $"inherited from {parent.Name}" } : parent.Enabled;
            LastDailySnapshotTimestamp = parent.LastDailySnapshotTimestamp.IsLocal ? parent.LastDailySnapshotTimestamp with { Source = $"inherited from {parent.Name}" } : parent.LastDailySnapshotTimestamp;
            LastFrequentSnapshotTimestamp = parent.LastFrequentSnapshotTimestamp.IsLocal ? parent.LastFrequentSnapshotTimestamp with { Source = $"inherited from {parent.Name}" } : parent.LastFrequentSnapshotTimestamp;
            LastHourlySnapshotTimestamp = parent.LastHourlySnapshotTimestamp.IsLocal ? parent.LastHourlySnapshotTimestamp with { Source = $"inherited from {parent.Name}" } : parent.LastHourlySnapshotTimestamp;
            LastMonthlySnapshotTimestamp = parent.LastMonthlySnapshotTimestamp.IsLocal ? parent.LastMonthlySnapshotTimestamp with { Source = $"inherited from {parent.Name}" } : parent.LastMonthlySnapshotTimestamp;
            LastWeeklySnapshotTimestamp = parent.LastWeeklySnapshotTimestamp.IsLocal ? parent.LastWeeklySnapshotTimestamp with { Source = $"inherited from {parent.Name}" } : parent.LastWeeklySnapshotTimestamp;
            LastYearlySnapshotTimestamp = parent.LastYearlySnapshotTimestamp.IsLocal ? parent.LastYearlySnapshotTimestamp with { Source = $"inherited from {parent.Name}" } : parent.LastYearlySnapshotTimestamp;
            PruneSnapshots = parent.PruneSnapshots.IsLocal ? parent.PruneSnapshots with { Source = $"inherited from {parent.Name}" } : parent.PruneSnapshots;
            Recursion = parent.Recursion.IsLocal ? parent.Recursion with { Source = $"inherited from {parent.Name}" } : parent.Recursion;
            SnapshotRetentionDaily = parent.SnapshotRetentionDaily.IsLocal ? parent.SnapshotRetentionDaily with { Source = $"inherited from {parent.Name}" } : parent.SnapshotRetentionDaily;
            SnapshotRetentionFrequent = parent.SnapshotRetentionFrequent.IsLocal ? parent.SnapshotRetentionFrequent with { Source = $"inherited from {parent.Name}" } : parent.SnapshotRetentionFrequent;
            SnapshotRetentionHourly = parent.SnapshotRetentionHourly.IsLocal ? parent.SnapshotRetentionHourly with { Source = $"inherited from {parent.Name}" } : parent.SnapshotRetentionHourly;
            SnapshotRetentionMonthly = parent.SnapshotRetentionMonthly.IsLocal ? parent.SnapshotRetentionMonthly with { Source = $"inherited from {parent.Name}" } : parent.SnapshotRetentionMonthly;
            SnapshotRetentionPruneDeferral = parent.SnapshotRetentionPruneDeferral.IsLocal ? parent.SnapshotRetentionPruneDeferral with { Source = $"inherited from {parent.Name}" } : parent.SnapshotRetentionPruneDeferral;
            SnapshotRetentionWeekly = parent.SnapshotRetentionWeekly.IsLocal ? parent.SnapshotRetentionWeekly with { Source = $"inherited from {parent.Name}" } : parent.SnapshotRetentionWeekly;
            SnapshotRetentionYearly = parent.SnapshotRetentionYearly.IsLocal ? parent.SnapshotRetentionYearly with { Source = $"inherited from {parent.Name}" } : parent.SnapshotRetentionYearly;
            TakeSnapshots = parent.TakeSnapshots.IsLocal ? parent.TakeSnapshots with { Source = $"inherited from {parent.Name}" } : parent.TakeSnapshots;
            Template = parent.Template.IsLocal ? parent.Template with { Source = $"inherited from {parent.Name}" } : parent.Template;
        }
        else
        {
            Enabled = new( ZfsPropertyNames.EnabledPropertyName, false, ZfsPropertySourceConstants.Local );
            LastDailySnapshotTimestamp = new( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local );
            LastFrequentSnapshotTimestamp = new( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local );
            LastHourlySnapshotTimestamp = new( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local );
            LastMonthlySnapshotTimestamp = new( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local );
            LastWeeklySnapshotTimestamp = new( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local );
            LastYearlySnapshotTimestamp = new( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local );
            PruneSnapshots = new( ZfsPropertyNames.PruneSnapshotsPropertyName, false, ZfsPropertySourceConstants.Local );
            Recursion = new( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local );
            SnapshotRetentionDaily = new( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, -1, ZfsPropertySourceConstants.Local );
            SnapshotRetentionFrequent = new( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, -1, ZfsPropertySourceConstants.Local );
            SnapshotRetentionHourly = new( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, -1, ZfsPropertySourceConstants.Local );
            SnapshotRetentionMonthly = new( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, -1, ZfsPropertySourceConstants.Local );
            SnapshotRetentionPruneDeferral = new( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0, ZfsPropertySourceConstants.Local );
            SnapshotRetentionWeekly = new( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, -1, ZfsPropertySourceConstants.Local );
            SnapshotRetentionYearly = new( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, -1, ZfsPropertySourceConstants.Local );
            TakeSnapshots = new( ZfsPropertyNames.TakeSnapshotsPropertyName, false, ZfsPropertySourceConstants.Local );
            Template = new( ZfsPropertyNames.TemplatePropertyName, "default", ZfsPropertySourceConstants.Local );
        }
    }

    public long BytesAvailable { get; }
    public long BytesUsed { get; }

    public ZfsProperty<bool> Enabled { get; private set; }

    [JsonIgnore]
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

    public ZfsProperty<DateTimeOffset> LastDailySnapshotTimestamp { get; private set; }
    public ZfsProperty<DateTimeOffset> LastFrequentSnapshotTimestamp { get; private set; }
    public ZfsProperty<DateTimeOffset> LastHourlySnapshotTimestamp { get; private set; }
    public ZfsProperty<DateTimeOffset> LastMonthlySnapshotTimestamp { get; private set; }

    [JsonIgnore]
    public DateTimeOffset LastObservedDailySnapshotTimestamp { get; private set; } = DateTimeOffset.UnixEpoch;

    [JsonIgnore]
    public DateTimeOffset LastObservedFrequentSnapshotTimestamp { get; private set; } = DateTimeOffset.UnixEpoch;

    [JsonIgnore]
    public DateTimeOffset LastObservedHourlySnapshotTimestamp { get; private set; } = DateTimeOffset.UnixEpoch;

    [JsonIgnore]
    public DateTimeOffset LastObservedMonthlySnapshotTimestamp { get; private set; } = DateTimeOffset.UnixEpoch;

    [JsonIgnore]
    public DateTimeOffset LastObservedWeeklySnapshotTimestamp { get; private set; } = DateTimeOffset.UnixEpoch;

    [JsonIgnore]
    public DateTimeOffset LastObservedYearlySnapshotTimestamp { get; private set; } = DateTimeOffset.UnixEpoch;

    public ZfsProperty<DateTimeOffset> LastWeeklySnapshotTimestamp { get; private set; }
    public ZfsProperty<DateTimeOffset> LastYearlySnapshotTimestamp { get; private set; }

    public string Name { get; init; }

    [JsonIgnore]
    public ZfsRecord ParentDataset { get; init; }

    [JsonIgnore]
    public ZfsRecord PoolRoot => IsPoolRoot ? this : ParentDataset.PoolRoot;

    [JsonIgnore]
    public ZfsProperty<bool> PruneSnapshots { get; private set; }

    public ZfsProperty<string> Recursion { get; private set; }
    public ZfsProperty<int> SnapshotRetentionDaily { get; private set; }
    public ZfsProperty<int> SnapshotRetentionFrequent { get; private set; }
    public ZfsProperty<int> SnapshotRetentionHourly { get; private set; }
    public ZfsProperty<int> SnapshotRetentionMonthly { get; private set; }
    public ZfsProperty<int> SnapshotRetentionPruneDeferral { get; private set; }
    public ZfsProperty<int> SnapshotRetentionWeekly { get; private set; }
    public ZfsProperty<int> SnapshotRetentionYearly { get; private set; }

    public ConcurrentDictionary<SnapshotPeriodKind, ConcurrentDictionary<string, Snapshot>> Snapshots { get; init; } = GetNewSnapshotCollection( );

    public ZfsProperty<bool> TakeSnapshots { get; private set; }
    public ZfsProperty<string> Template { get; private set; }

    [JsonIgnore]
    internal Regex NameValidatorRegex { get; }

    [JsonIgnore]
    private long PercentBytesUsed => BytesUsed * 100 / BytesAvailable;

    /// <inheritdoc />
    /// <remarks>
    ///     The default generated record equality causes stack overflow due to the self-reference in root datasets,
    ///     so it is excluded from equality comparison.<br />
    ///     This equality method ONLY checks value properties and does not check collections.
    /// </remarks>
    public virtual bool Equals( ZfsRecord? other )
    {
        // Returning equality of value properties alphabetically
        return
            other is not null
            && BytesAvailable == other.BytesAvailable
            && BytesUsed == other.BytesUsed
            && Enabled == other.Enabled
            && IsPoolRoot == other.IsPoolRoot
            && Kind == other.Kind
            && LastDailySnapshotTimestamp == other.LastDailySnapshotTimestamp
            && LastFrequentSnapshotTimestamp == other.LastFrequentSnapshotTimestamp
            && LastHourlySnapshotTimestamp == other.LastHourlySnapshotTimestamp
            && LastMonthlySnapshotTimestamp == other.LastMonthlySnapshotTimestamp
            && LastObservedDailySnapshotTimestamp == other.LastObservedDailySnapshotTimestamp
            && LastObservedFrequentSnapshotTimestamp == other.LastObservedFrequentSnapshotTimestamp
            && LastObservedHourlySnapshotTimestamp == other.LastObservedHourlySnapshotTimestamp
            && LastObservedMonthlySnapshotTimestamp == other.LastObservedMonthlySnapshotTimestamp
            && LastObservedWeeklySnapshotTimestamp == other.LastObservedMonthlySnapshotTimestamp
            && LastObservedYearlySnapshotTimestamp == other.LastObservedYearlySnapshotTimestamp
            && LastWeeklySnapshotTimestamp == other.LastWeeklySnapshotTimestamp
            && LastYearlySnapshotTimestamp == other.LastYearlySnapshotTimestamp
            && Name == other.Name
            && PruneSnapshots == other.PruneSnapshots
            && Recursion == other.Recursion
            && SnapshotRetentionDaily == other.SnapshotRetentionDaily
            && SnapshotRetentionFrequent == other.SnapshotRetentionFrequent
            && SnapshotRetentionHourly == other.SnapshotRetentionHourly
            && SnapshotRetentionMonthly == other.SnapshotRetentionMonthly
            && SnapshotRetentionPruneDeferral == other.SnapshotRetentionPruneDeferral
            && SnapshotRetentionWeekly == other.SnapshotRetentionWeekly
            && SnapshotRetentionYearly == other.SnapshotRetentionYearly
            && TakeSnapshots == other.TakeSnapshots
            && Template == other.Template;
    }

    public Snapshot AddSnapshot( Snapshot snap )
    {
        Logger.Trace( "Adding snapshot {0} to {1} {2}", snap.Name, Kind, Name );
        Snapshots[ snap.Period.Value.Kind ][ snap.Name ] = snap;
        switch ( snap.Period.Value.Kind )
        {
            case SnapshotPeriodKind.Frequent:
                if ( LastObservedFrequentSnapshotTimestamp < snap.Timestamp.Value )
                {
                    LastObservedFrequentSnapshotTimestamp = snap.Timestamp.Value;
                }

                break;
            case SnapshotPeriodKind.Hourly:
                if ( LastObservedHourlySnapshotTimestamp < snap.Timestamp.Value )
                {
                    LastObservedHourlySnapshotTimestamp = snap.Timestamp.Value;
                }

                break;
            case SnapshotPeriodKind.Daily:
                if ( LastObservedDailySnapshotTimestamp < snap.Timestamp.Value )
                {
                    LastObservedDailySnapshotTimestamp = snap.Timestamp.Value;
                }

                break;
            case SnapshotPeriodKind.Weekly:
                if ( LastObservedWeeklySnapshotTimestamp < snap.Timestamp.Value )
                {
                    LastObservedWeeklySnapshotTimestamp = snap.Timestamp.Value;
                }

                break;
            case SnapshotPeriodKind.Monthly:
                if ( LastObservedMonthlySnapshotTimestamp < snap.Timestamp.Value )
                {
                    LastObservedMonthlySnapshotTimestamp = snap.Timestamp.Value;
                }

                break;
            case SnapshotPeriodKind.Yearly:
                if ( LastObservedYearlySnapshotTimestamp < snap.Timestamp.Value )
                {
                    LastObservedYearlySnapshotTimestamp = snap.Timestamp.Value;
                }

                break;
            default:
                throw new InvalidOperationException( "Invalid Snapshot Period specified" );
        }

        return snap;
    }

    public static ZfsRecord CreateInstanceFromAllProperties( string name, string kind, ZfsProperty<bool> enabled, ZfsProperty<bool> takeSnapshots, ZfsProperty<bool> pruneSnapshots, ZfsProperty<DateTimeOffset> lastFrequentSnapshotTimestamp, ZfsProperty<DateTimeOffset> lastHourlySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastDailySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastWeeklySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastMonthlySnapshotTimestamp, ZfsProperty<DateTimeOffset> lastYearlySnapshotTimestamp, ZfsProperty<string> recursion, ZfsProperty<string> template, ZfsProperty<int> retentionFrequent, ZfsProperty<int> retentionHourly, ZfsProperty<int> retentionDaily, ZfsProperty<int> retentionWeekly, ZfsProperty<int> retentionMonthly, ZfsProperty<int> retentionYearly, ZfsProperty<int> retentionPruneDeferral, long bytesAvailable, long bytesUsed, ZfsRecord? parent = null )
    {
        return new( name, kind, enabled, takeSnapshots, pruneSnapshots, lastFrequentSnapshotTimestamp, lastHourlySnapshotTimestamp, lastDailySnapshotTimestamp, lastWeeklySnapshotTimestamp, lastMonthlySnapshotTimestamp, lastYearlySnapshotTimestamp, recursion, template, retentionFrequent, retentionHourly, retentionDaily, retentionWeekly, retentionMonthly, retentionYearly, retentionPruneDeferral, bytesAvailable, bytesUsed, parent );
    }

    /// <inheritdoc />
    /// <remarks>
    ///     Uses name and kind only, since they are the only truly immutable properties relevant to this function
    /// </remarks>
    public override int GetHashCode( )
    {
        HashCode hashCode = new( );
        hashCode.Add( Kind, StringComparer.CurrentCulture );
        hashCode.Add( Name, StringComparer.CurrentCulture );
        return hashCode.ToHashCode( );
    }

    /// <summary>
    ///     Gets the collection of <see cref="Snapshot" />s, groups by <see cref="SnapshotPeriodKind" />
    /// </summary>
    /// <remarks>
    ///     Note that this is a reference type, as are the values of this collection.<br />
    ///     Thus, when cloning a <see cref="ZfsRecord" /> using the <see langword="with" /> operator, this collection
    ///     needs to be re-created and all of its values deep-copied manually, if unique references are needed.
    /// </remarks>
    public static ConcurrentDictionary<SnapshotPeriodKind, ConcurrentDictionary<string, Snapshot>> GetNewSnapshotCollection( )
    {
        return new(
            new Dictionary<SnapshotPeriodKind, ConcurrentDictionary<string, Snapshot>>
            {
                { SnapshotPeriodKind.Frequent, new ConcurrentDictionary<string, Snapshot>( ) },
                { SnapshotPeriodKind.Hourly, new ConcurrentDictionary<string, Snapshot>( ) },
                { SnapshotPeriodKind.Daily, new ConcurrentDictionary<string, Snapshot>( ) },
                { SnapshotPeriodKind.Weekly, new ConcurrentDictionary<string, Snapshot>( ) },
                { SnapshotPeriodKind.Monthly, new ConcurrentDictionary<string, Snapshot>( ) },
                { SnapshotPeriodKind.Yearly, new ConcurrentDictionary<string, Snapshot>( ) }
            } );
    }

    public List<Snapshot> GetSnapshotsToPrune( )
    {
        Logger.Debug( "Getting list of snapshots to prune for dataset {0}", Name );
        if ( !Enabled.Value )
        {
            Logger.Debug( "Dataset {0} is disabled. Skipping pruning", Name );
            return new( );
        }

        Logger.Debug( "Checking prune deferral setting for dataset {0}", Name );
        if ( SnapshotRetentionPruneDeferral.Value != 0 && PercentBytesUsed < SnapshotRetentionPruneDeferral.Value )
        {
            Logger.Info( "Used capacity for {0} ({1}%) is below prune deferral threshold of {2}%. Skipping pruning of {0}", Name, PercentBytesUsed, SnapshotRetentionPruneDeferral.Value );
            return new( );
        }

        if ( SnapshotRetentionPruneDeferral.Value == 0 )
        {
            Logger.Debug( "Prune deferral not enabled for {0}", Name );
        }

        List<Snapshot> snapshotsToPrune = new( );

        GetSnapshotsToPruneForPeriod( SnapshotPeriod.Frequent, SnapshotRetentionFrequent.Value, snapshotsToPrune );
        GetSnapshotsToPruneForPeriod( SnapshotPeriod.Hourly, SnapshotRetentionHourly.Value, snapshotsToPrune );
        GetSnapshotsToPruneForPeriod( SnapshotPeriod.Daily, SnapshotRetentionDaily.Value, snapshotsToPrune );
        GetSnapshotsToPruneForPeriod( SnapshotPeriod.Weekly, SnapshotRetentionWeekly.Value, snapshotsToPrune );
        GetSnapshotsToPruneForPeriod( SnapshotPeriod.Monthly, SnapshotRetentionMonthly.Value, snapshotsToPrune );
        GetSnapshotsToPruneForPeriod( SnapshotPeriod.Yearly, SnapshotRetentionYearly.Value, snapshotsToPrune );

        return snapshotsToPrune;
    }

    /// <summary>
    ///     Gets whether a daily snapshot is needed, according to the <paramref name="timestamp" /> and the properties defined
    ///     on the object
    /// </summary>
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type
    /// </param>
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
        if ( timestamp <= LastDailySnapshotTimestamp.Value )
        {
            return false;
        }

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
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type
    /// </param>
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
        if ( timestamp < LastFrequentSnapshotTimestamp.Value )
        {
            return false;
        }

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
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type
    /// </param>
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
        if ( timestamp < LastHourlySnapshotTimestamp.Value )
        {
            return false;
        }

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
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type
    /// </param>
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
        if ( timestamp < LastMonthlySnapshotTimestamp.Value )
        {
            return false;
        }

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
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type
    /// </param>
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
        if ( timestamp < LastWeeklySnapshotTimestamp.Value )
        {
            return false;
        }

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
    /// <param name="timestamp">
    ///     The <see cref="DateTimeOffset" /> value to check against the last known snapshot of this type
    /// </param>
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
        if ( timestamp < LastYearlySnapshotTimestamp.Value )
        {
            return false;
        }

        int lastYearlySnapshotYear = CultureInfo.CurrentCulture.Calendar.GetYear( LastYearlySnapshotTimestamp.Value.LocalDateTime );
        int currentYear = CultureInfo.CurrentCulture.Calendar.GetYear( timestamp.LocalDateTime );
        // Check if the last yearly snapshot was in a different year
        bool yearlySnapshotNeeded = lastYearlySnapshotYear < currentYear;
        Logger.Debug( "Yearly snapshot is {2}needed for dataset {0} at timestamp {1:O}", Name, timestamp, yearlySnapshotNeeded ? "" : "not " );
        return yearlySnapshotNeeded;
    }

    /// <summary>
    ///     Updates a property for this <see cref="ZfsRecord" /> object and returns the new property boxed as an
    ///     <see cref="IZfsProperty" /> instance
    /// </summary>
    /// <param name="propertyName">The name of the property to update</param>
    /// <param name="propertyValue">The new value for the property</param>
    /// <param name="propertySource">The source of the property</param>
    /// <returns>
    ///     The new property created by this method, boxed as an <see cref="IZfsProperty" /> instance
    /// </returns>
    /// <remarks>
    ///     <see cref="ZfsProperty{T}" /> is immutable. This method calls the copy constructor using "<see langword="with" />"
    /// </remarks>
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
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If <paramref name="propertyName" /> is not one of the following values:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.EnabledPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.TakeSnapshotsPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.PruneSnapshotsPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.RecursionPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.TemplatePropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionFrequentPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionHourlyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionDailyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionYearlyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName" />
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    public IZfsProperty UpdateProperty( string propertyName, string propertyValue, string propertySource = ZfsPropertySourceConstants.Local )
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

    /// <summary>
    ///     Updates a <see cref="bool" /> property for this <see cref="ZfsRecord" /> object and returns the new property
    /// </summary>
    /// <param name="propertyName">The name of the property to update</param>
    /// <param name="propertyValue">The new value for the property</param>
    /// <param name="propertySource">The source of the property</param>
    /// <returns>The new property created by this method</returns>
    /// <remarks>
    ///     <see cref="ZfsProperty{T}" /> is immutable. This method calls the copy constructor using "<see langword="with" />"
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If <paramref name="propertyName" /> is not one of the following values:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.EnabledPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.TakeSnapshotsPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.PruneSnapshotsPropertyName" />
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    public ZfsProperty<bool> UpdateProperty( string propertyName, bool propertyValue, string propertySource = ZfsPropertySourceConstants.Local )
    {
        return propertyName switch
        {
            ZfsPropertyNames.EnabledPropertyName => Enabled = Enabled with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.TakeSnapshotsPropertyName => TakeSnapshots = TakeSnapshots with { Value = propertyValue, Source = propertySource },
            ZfsPropertyNames.PruneSnapshotsPropertyName => PruneSnapshots = PruneSnapshots with { Value = propertyValue, Source = propertySource },
            _ => throw new ArgumentOutOfRangeException( nameof( propertyName ), $"{propertyName} is not a supported boolean property" )
        };
    }

    /// <summary>
    ///     Updates a <see cref="DateTimeOffset" /> property for this <see cref="ZfsRecord" /> object and returns the new property
    /// </summary>
    /// <param name="propertyName">The name of the property to update</param>
    /// <param name="propertyValue">The new value for the property</param>
    /// <param name="propertySource">The source of the property</param>
    /// <returns>The new property created by this method</returns>
    /// <remarks>
    ///     <see cref="ZfsProperty{T}" /> is immutable. This method calls the copy constructor using "<see langword="with" />"
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If <paramref name="propertyName" /> is not one of the following values:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName" />
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    public ZfsProperty<DateTimeOffset> UpdateProperty( string propertyName, DateTimeOffset propertyValue, string propertySource = ZfsPropertySourceConstants.Local )
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

    /// <summary>
    ///     Updates an <see cref="int" /> property for this <see cref="ZfsRecord" /> object and returns the new property
    /// </summary>
    /// <param name="propertyName">The name of the property to update</param>
    /// <param name="propertyValue">The new value for the property</param>
    /// <param name="propertySource">The source of the property</param>
    /// <returns>The new property created by this method</returns>
    /// <remarks>
    ///     <see cref="ZfsProperty{T}" /> is immutable. This method calls the copy constructor using "<see langword="with" />"
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If <paramref name="propertyName" /> is not one of the following values:
    ///     <list type="bullet">
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionFrequentPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionHourlyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionDailyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionYearlyPropertyName" />
    ///             </description>
    ///         </item>
    ///         <item>
    ///             <description>
    ///                 <see cref="ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName" />
    ///             </description>
    ///         </item>
    ///     </list>
    /// </exception>
    public ZfsProperty<int> UpdateProperty( string propertyName, int propertyValue, string propertySource = ZfsPropertySourceConstants.Local )
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

    private void GetSnapshotsToPruneForPeriod( SnapshotPeriod snapshotPeriod, int retentionValue, List<Snapshot> snapshotsToPrune )
    {
        List<Snapshot> snapshotsSetForPruning = Snapshots[ snapshotPeriod.Kind ].Where( kvp => kvp.Value.PruneSnapshots.Value ).Select( kvp => kvp.Value ).ToList( );
        Logger.Debug( "{0} snapshots of {1} available for pruning: {2}", snapshotPeriod, Name, snapshotsSetForPruning.Select( s => s.Name ).ToCommaSeparatedSingleLineString( ) );
        Logger.Trace( "{0} retention is {1:D} for {2} {3}", snapshotPeriod, retentionValue, Kind, Name );
        if ( snapshotsSetForPruning.Count > retentionValue )
        {
            int numberToPrune = snapshotsSetForPruning.Count - retentionValue;
            Logger.Debug( "Need to prune oldest {0} {1} snapshots from {2}", numberToPrune, snapshotPeriod, Name );
            snapshotsSetForPruning.Sort( );
            for ( int i = 0; i < numberToPrune; i++ )
            {
                Snapshot snap = snapshotsSetForPruning[ i ];
                Logger.Debug( "Adding snapshot {0} to prune list", snap.Name );
                snapshotsToPrune.Add( snap );
            }
        }
    }
}
