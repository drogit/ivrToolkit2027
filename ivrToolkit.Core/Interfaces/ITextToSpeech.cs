using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Util;

namespace ivrToolkit.Core.Interfaces;

/// <summary>
/// Represents the TTS engine.
/// </summary>
public interface ITextToSpeech
{
    /// <summary>
    /// Converts the text to Speech and returns a wav audio stream including the wav header.
    /// </summary>
    /// <param name="text">The text to convert to a wav stream</param>
    /// <returns>A wav stream representing the text. Includes the wav header too.</returns>
    WavStream TextToSpeech(string text);
    
    /// <summary>
    /// Converts the text to Speech and returns a wav audio stream including the wav header.
    /// </summary>
    /// <param name="text">The text to convert to a wav stream</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A wav stream representing the text. Includes the wav header too.</returns>
    Task<WavStream> TextToSpeechAsync(string text, CancellationToken cancellationToken);

    /// <summary>
    /// Converts the text to Speech using a specific voice and returns a wav audio stream including the wav header.
    /// Engines that don't support a voice override ignore <paramref name="voiceName"/>.
    /// </summary>
    /// <param name="text">The text to convert to a wav stream</param>
    /// <param name="voiceName">The voice to use, for example "fr-CA-SylvieNeural". Null or empty uses the configured voice.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A wav stream representing the text. Includes the wav header too.</returns>
    Task<WavStream> TextToSpeechAsync(string text, string voiceName, CancellationToken cancellationToken) =>
        TextToSpeechAsync(text, cancellationToken);
}