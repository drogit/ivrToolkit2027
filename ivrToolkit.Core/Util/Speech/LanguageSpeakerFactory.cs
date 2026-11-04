using System;

namespace ivrToolkit.Core.Util.Speech;

/// <summary>
/// Chooses the <see cref="ILanguageSpeaker"/> to use based on the system recordings subfolder name.
/// </summary>
/// <remarks>
/// The language is inferred from <see cref="VoiceProperties.SystemRecordingSubfolder"/>:
/// <list type="bullet">
/// <item><description><c>Original</c> (case-insensitive) - the legacy recordings, spoken in English.</description></item>
/// <item><description>Starts with <c>fr-</c> (case-insensitive), for example <c>fr-CA-SylvieNeural</c> - French Canadian.</description></item>
/// <item><description>Anything else, including null or empty, for example <c>en-US-JennyNeural</c> - English.</description></item>
/// </list>
/// Subfolders should be named after the Azure TTS voice that generated them.
/// </remarks>
internal static class LanguageSpeakerFactory
{
    public const string OriginalFolder = "Original";
    public const string FrenchPrefix = "fr-";

    public static ILanguageSpeaker Create(string subfolder)
    {
        if (string.Equals(subfolder, OriginalFolder, StringComparison.OrdinalIgnoreCase))
        {
            return new EnglishSpeaker();
        }

        if (subfolder != null && subfolder.StartsWith(FrenchPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return new FrenchCanadianSpeaker();
        }

        return new EnglishSpeaker();
    }
}
