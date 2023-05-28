﻿// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using NLog;

namespace Sanoid.Interop.Zfs.ZfsTypes;

public class ZfsProperty
{
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    public ZfsProperty(string propertyNamespace, string propertyName, string propertyValue, string valueSource, string? inheritedFrom = null)
    {
        Namespace = propertyNamespace;
        Name = propertyName;
        Value = propertyValue;
        Source = valueSource;
        InheritedFrom = inheritedFrom;
    }
    private ZfsProperty(string[] components)
    {
        Logger.Debug( "Creating new ZfsProperty from array [{0}]", string.Join( ',', components ) );
        string[] nameComponents = components[0].Split(':', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        switch (nameComponents.Length)
        {
            case 2:
                Namespace = nameComponents[0];
                Name = nameComponents[1];
                break;
            default:
                Namespace = string.Empty;
                Name = components[0];
                break;
        }

        Value = components[1];
        Source = components[2];
        if (components.Length > 3 && components[3].Length >= 16)
        {
            InheritedFrom = components[3][16..];
        }

        Logger.Debug( "ZfsProperty created: {0}", this );
    }

    public string? InheritedFrom { get; set; }
    public string Source { get; set; }

    public static Dictionary<string, ZfsProperty> DefaultProperties { get; } = new()
    {
        { "sanoid.net:template", new( "sanoid.net:","template", "default","sanoid" ) },
        { "sanoid.net:enabled", new("sanoid.net:", "enabled", "false","sanoid" ) },
        { "sanoid.net:skipchildren", new("sanoid.net:", "skipchildren", "true","sanoid" ) },
        { "sanoid.net:autoprune", new("sanoid.net:", "autoprune", "false","sanoid" ) },
        { "sanoid.net:autosnapshot", new("sanoid.net:", "autosnapshot", "false","sanoid" ) },
        { "sanoid.net:recursive", new("sanoid.net:", "recursive", "false","sanoid" ) },
    };

    public string Name { get; set; }
    public string Namespace { get; set; }
    public string Value { get; set; }

    /// <inheritdoc />
    public override string ToString( )
    {
        return $"{Namespace}{Name}: {Value}";
    }

    public static bool TryParse(string value, out ZfsProperty? property)
    {
        Logger.Debug("Trying to parse new ZfsProperty from {0}", value);
        property = null;
        try
        {
            property = Parse(value);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return false;
        }
        catch (ArgumentNullException ex)
        {
            return false;
        }

        Logger.Debug( "Successfully parsed ZfsProperty {0}", property );

        return true;
    }

    /// <summary>
    ///     Gets a <see cref="ZfsProperty" /> parsed from the supplied string
    /// </summary>
    /// <exception cref="ArgumentNullException">If <paramref propertyName="value" /> is a null, empty, or entirely whitespace string</exception>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If the provided property string has less than 3 components separated by a
    ///     tab character.
    /// </exception>
    public static ZfsProperty Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            const string errorString = "Unable to parse ZfsProperty. String must not be null, empty, or whitespace.";
            Logger.Error(errorString);
            throw new ArgumentNullException(nameof(value), errorString);
        }
        Logger.Debug("Parsing ZfsProperty from {0}", value);

        string[] components = value.Split('\t', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (components.Length < 3)
        {
            const string errorString = "ZfsProperty value string is invalid.";
            Logger.Error(errorString);
            throw new ArgumentOutOfRangeException(nameof(value), errorString);
        }

        return new(components);
    }

    public string SetString => $"{Namespace}{Name}={Value}";
}