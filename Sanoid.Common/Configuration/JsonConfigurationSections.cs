// LICENSE:
// 
// This software is licensed for use under the Free Software Foundation's GPL v3.0 license, as retrieved
// from http://www.gnu.org/licenses/gpl-3.0.html on 2014-11-17.  A copy should also be available in this
// project's Git repository at https://github.com/jimsalterjrs/sanoid/blob/master/LICENSE.

using System.Text.Json;
using Json.Schema;
using Microsoft.Extensions.Configuration;
using NLog;

namespace Sanoid.Common.Configuration;

/// <summary>
///     Singleton class for easy global access to utility functions and properties such as configuration.
/// </summary>
public static class JsonConfigurationSections
{
    /// <summary>
    ///     Gets the /Formatting configuration section of Sanoid.json
    /// </summary>
    /// <seealso cref="SnapshotNamingConfiguration" />
    public static IConfigurationSection FormattingConfiguration => RootConfiguration.GetRequiredSection( "Formatting" );

    /// <summary>
    ///     Gets the /Monitoring configuration section of Sanoid.json
    /// </summary>
    public static IConfigurationSection MonitoringConfiguration => FormattingConfiguration.GetRequiredSection( "Monitoring" );

    /// <summary>
    ///     Gets the root configuration section of Sanoid.json
    /// </summary>
    /// <remarks>
    ///     Should only explicitly be used for access to properties in the configuration root.<br />
    ///     Other static properties are exposed in <see cref="JsonConfigurationSections" /> for sub-sections of Sanoid.json.
    /// </remarks>
    /// <seealso cref="FormattingConfiguration" />
    /// <seealso cref="SnapshotNamingConfiguration" />
    /// <seealso cref="ValidateSanoidConfiguration()" />
    public static IConfigurationRoot RootConfiguration
    {
        get
        {
            if ( !ConfigIsValid )
            {
                ValidateSanoidConfiguration();
                ConfigIsValid = true;
            }

            return _rootConfiguration ??= new ConfigurationManager( ).AddJsonFile( "Sanoid.json", false, true ).Build( );
        }
    }

    private static bool ConfigIsValid;

    /// <summary>
    ///     Gets the /Formatting/SnapshotNaming configuration section of Sanoid.json
    /// </summary>
    public static IConfigurationSection SnapshotNamingConfiguration => FormattingConfiguration.GetRequiredSection( "SnapshotNaming" );

    private static IConfigurationRoot? _rootConfiguration;
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

    /// <summary>
    ///     Validates Sanoid.json against Sanoid.schema.json.<br />
    ///     If the method does not throw, the configuration is valid for use.
    /// </summary>
    /// <exception cref="JsonException">If Sanoid.json is invalid according to Sanoid.schema.json</exception>
    private static void ValidateSanoidConfiguration()
    {
        JsonSchema sanoidConfigJsonSchema = JsonSchema.FromFile( "Sanoid.schema.json" );
        using JsonDocument sanoidConfigJsonDocument = JsonDocument.Parse( File.ReadAllText( "Sanoid.json" ) );
        EvaluationOptions evaluationOptions = new()
        {
            EvaluateAs = SpecVersion.Draft7,
            RequireFormatValidation = true,
            OnlyKnownFormats = true,
            OutputFormat = OutputFormat.List,
            ValidateAgainstMetaSchema = false
        };

        EvaluationResults configValidationResults = sanoidConfigJsonSchema.Evaluate( sanoidConfigJsonDocument, evaluationOptions );

        if ( !configValidationResults.IsValid )
        {
            Logger.Error( "Sanoid.json validation failed." );
            foreach ( EvaluationResults validationDetail in configValidationResults.Details )
            {
                if ( validationDetail is { IsValid: false, HasErrors: true } )
                {
                    Logger.Error( $"{validationDetail.InstanceLocation} has {validationDetail.Errors!.Count} problems:" );
                    foreach ( KeyValuePair<string, string> error in validationDetail.Errors )
                    {
                        Logger.Error( $"  Problem: {error.Key}; Details: {error.Value}" );
                    }
                }
            }

            throw new ConfigurationValidationException( "Sanoid.json validation failed. Please check Sanoid.json and ensure it complies with the schema specified in Sanoid.schema.json." );
        }
    }
}
