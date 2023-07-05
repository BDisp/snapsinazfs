﻿// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license

using System.Collections.Concurrent;
using SnapsInAZfs.Interop.Zfs.ZfsTypes;
using SnapsInAZfs.Interop.Zfs.ZfsTypes.Validation;

namespace SnapsInAZfs.Interop.Tests.Zfs.ZfsTypes.ZfsRecordTests;

[TestFixture]
[TestOf( typeof( ZfsRecord ) )]
[Order( 100 )]
public class ZfsRecordTests_Constructors
{
    [Test]
    [Description( "Test of the constructor that can only be used to create pool root FileSystem records" )]
    [Order( 1 )]
    [NonParallelizable]
    [Category( "TypeChecks" )]
    [TestCase( "testRootZfsRecord1", ZfsPropertyValueConstants.FileSystem )]
    [TestCase( "testRootZfsRecord2", ZfsPropertyValueConstants.FileSystem )]
    public void Constructor_ZfsRecord_Name_Kind_ExpectedPropertiesSet( string name, string kind )
    {
        ZfsRecord dataset = new( name, kind );
        Assume.That( ZfsRecordTestHelpers.TestDatasets, Is.Not.Null );
        Assume.That( ZfsRecordTestHelpers.TestDatasets, Is.InstanceOf<ConcurrentDictionary<string, ZfsRecord>>( ) );
        Assume.That( ZfsRecordTestHelpers.TestDatasets, Does.Not.ContainKey( name ) );
        Assert.That( dataset, Is.Not.Null );
        Assert.That( dataset, Is.InstanceOf<ZfsRecord>( ) );
        Assert.Multiple( ( ) =>
        {
            Assert.That( dataset.Name, Is.EqualTo( name ) );
            Assert.That( dataset.Kind, Is.EqualTo( kind ) );
            Assert.That( dataset.ParentDataset, Is.SameAs( dataset ) );
            Assert.That( dataset.IsPoolRoot, Is.True );
            Assert.That( dataset.NameValidatorRegex, Is.SameAs( ZfsIdentifierRegexes.DatasetNameRegex( ) ) );
            Assert.That( dataset.BytesAvailable, Is.Zero );
            Assert.That( dataset.BytesUsed, Is.Zero );
            Assert.That( dataset.Enabled, Is.EqualTo( new ZfsProperty<bool>( ZfsPropertyNames.EnabledPropertyName, false, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.LastDailySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.LastFrequentSnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.LastHourlySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.LastMonthlySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.LastObservedDailySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedFrequentSnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedHourlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedMonthlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedWeeklySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedYearlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastWeeklySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.LastYearlySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.PruneSnapshots, Is.EqualTo( new ZfsProperty<bool>( ZfsPropertyNames.PruneSnapshotsPropertyName, false, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.Recursion, Is.EqualTo( new ZfsProperty<string>( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.SnapshotRetentionDaily, Is.EqualTo( new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, -1, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.SnapshotRetentionFrequent, Is.EqualTo( new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, -1, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.SnapshotRetentionHourly, Is.EqualTo( new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, -1, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.SnapshotRetentionMonthly, Is.EqualTo( new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, -1, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.SnapshotRetentionPruneDeferral, Is.EqualTo( new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.SnapshotRetentionWeekly, Is.EqualTo( new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, -1, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.SnapshotRetentionYearly, Is.EqualTo( new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, -1, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.TakeSnapshots, Is.EqualTo( new ZfsProperty<bool>( ZfsPropertyNames.TakeSnapshotsPropertyName, false, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.Template, Is.EqualTo( new ZfsProperty<string>( ZfsPropertyNames.TemplatePropertyName, "default", ZfsPropertySourceConstants.Local ) ) );
        } );
        ZfsRecordTestHelpers.TestDatasets[ dataset.Name ] = dataset;
    }

    [Test]
    [Order( 2 )]
    [NonParallelizable]
    [Category( "TypeChecks" )]
    [TestCase( "testRootZfsRecord1/ChildFs1", ZfsPropertyValueConstants.FileSystem, "testRootZfsRecord1" )]
    [TestCase( "testRootZfsRecord1/ChildVol1", ZfsPropertyValueConstants.Volume, "testRootZfsRecord1" )]
    [TestCase( "testRootZfsRecord2/ChildFs1", ZfsPropertyValueConstants.FileSystem, "testRootZfsRecord2" )]
    [TestCase( "testRootZfsRecord2/ChildVol1", ZfsPropertyValueConstants.Volume, "testRootZfsRecord2" )]
    public void Constructor_ZfsRecord_Name_Kind_Parent_ExpectedPropertiesSet( string name, string kind, string parentName )
    {
        Assume.That( ZfsRecordTestHelpers.TestDatasets, Is.Not.Null );
        Assume.That( ZfsRecordTestHelpers.TestDatasets, Is.InstanceOf<ConcurrentDictionary<string, ZfsRecord>>( ) );
        Assume.That( ZfsRecordTestHelpers.TestDatasets, Is.Not.Empty );
        Assume.That( ZfsRecordTestHelpers.TestDatasets, Does.ContainKey( parentName ) );
        Assume.That( ZfsRecordTestHelpers.TestDatasets, Does.Not.ContainKey( name ) );
        ZfsRecord parentDataset = ZfsRecordTestHelpers.TestDatasets[ parentName ];
        Assume.That( parentDataset, Is.Not.Null );
        Assume.That( parentDataset, Is.TypeOf<ZfsRecord>( ) );
        ZfsRecord dataset = new( name, kind, ZfsRecordTestHelpers.TestDatasets[ parentName ] );
        Assert.Multiple( ( ) =>
        {
            Assert.That( dataset, Is.Not.Null );
            Assert.That( dataset, Is.InstanceOf<ZfsRecord>( ) );
        } );
        Assert.Multiple( ( ) =>
        {
            Assert.That( dataset.Name, Is.EqualTo( name ) );
            Assert.That( dataset.Kind, Is.EqualTo( kind ) );
            Assert.That( dataset.ParentDataset, Is.SameAs( parentDataset ) );
            Assert.That( dataset.IsPoolRoot, Is.False );
            Assert.That( dataset.NameValidatorRegex, Is.SameAs( ZfsIdentifierRegexes.DatasetNameRegex( ) ) );
            Assert.That( dataset.BytesAvailable, Is.Zero );
            Assert.That( dataset.BytesUsed, Is.Zero );
            Assert.That( dataset.Enabled, Is.EqualTo( new ZfsProperty<bool>( ZfsPropertyNames.EnabledPropertyName, false, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.LastDailySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastDailySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.LastFrequentSnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastFrequentSnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.LastHourlySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastHourlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.LastMonthlySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastMonthlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.LastObservedDailySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedFrequentSnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedHourlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedMonthlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedWeeklySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastObservedYearlySnapshotTimestamp, Is.EqualTo( DateTimeOffset.UnixEpoch ) );
            Assert.That( dataset.LastWeeklySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastWeeklySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.LastYearlySnapshotTimestamp, Is.EqualTo( new ZfsProperty<DateTimeOffset>( ZfsPropertyNames.DatasetLastYearlySnapshotTimestampPropertyName, DateTimeOffset.UnixEpoch, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.PruneSnapshots, Is.EqualTo( new ZfsProperty<bool>( ZfsPropertyNames.PruneSnapshotsPropertyName, false, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.Recursion, Is.EqualTo( new ZfsProperty<string>( ZfsPropertyNames.RecursionPropertyName, ZfsPropertyValueConstants.SnapsInAZfs, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.SnapshotRetentionDaily, Is.EqualTo( new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionDailyPropertyName, -1, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.SnapshotRetentionFrequent, Is.EqualTo( new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionFrequentPropertyName, -1, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.SnapshotRetentionHourly, Is.EqualTo( new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionHourlyPropertyName, -1, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.SnapshotRetentionMonthly, Is.EqualTo( new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionMonthlyPropertyName, -1, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.SnapshotRetentionPruneDeferral, Is.EqualTo( new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionPruneDeferralPropertyName, 0, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.SnapshotRetentionWeekly, Is.EqualTo( new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionWeeklyPropertyName, -1, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.SnapshotRetentionYearly, Is.EqualTo( new ZfsProperty<int>( ZfsPropertyNames.SnapshotRetentionYearlyPropertyName, -1, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.TakeSnapshots, Is.EqualTo( new ZfsProperty<bool>( ZfsPropertyNames.TakeSnapshotsPropertyName, false, ZfsPropertySourceConstants.Local ) ) );
            Assert.That( dataset.Template, Is.EqualTo( new ZfsProperty<string>( ZfsPropertyNames.TemplatePropertyName, "default", ZfsPropertySourceConstants.Local ) ) );
        } );
        ZfsRecordTestHelpers.TestDatasets[ dataset.Name ] = dataset;
    }
}
