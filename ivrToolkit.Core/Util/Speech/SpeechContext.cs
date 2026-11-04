using System;
using System.Threading.Tasks;

namespace ivrToolkit.Core.Util.Speech;

/// <summary>
/// Supplies the delegate a <see cref="ILanguageSpeaker"/> uses to play a system recording.
/// </summary>
internal sealed class SpeechContext
{
    /// <summary>
    /// Plays a system recording by name, without the folder or the ".wav" extension.
    /// </summary>
    public Func<string, Task> PlayF { get; }

    public SpeechContext(Func<string, Task> playF)
    {
        PlayF = playF ?? throw new ArgumentNullException(nameof(playF));
    }
}
