using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ivrToolkit.Core.Util.Speech;
using Shouldly;

namespace ivrToolkit.Core.Tests.SpeechTests;

/// <summary>
/// Records every system recording a speaker asks to play.
/// </summary>
internal sealed class SpeechRecorder
{
    public List<string> Played { get; } = new();

    public SpeechContext Context { get; }

    public SpeechRecorder()
    {
        Context = new SpeechContext(file =>
        {
            Played.Add(file);
            return Task.CompletedTask;
        });
    }

    /// <summary>
    /// Checks that every played file is a recording the language's generator creates.
    /// </summary>
    public void ShouldAllExistIn(ISet<string> recordings)
    {
        var missing = Played.Where(file => !recordings.Contains(file)).ToList();
        missing.ShouldBeEmpty($"Recordings not generated: {string.Join(", ", missing)}");
    }
}

/// <summary>
/// The recordings created by Utilities/SystemRecordingsGenerator (English.cs and FrenchCanadian.cs), without ".wav".
/// File names are matched case-insensitively, the same as the Windows file system.
/// </summary>
internal static class SystemRecordings
{
    private static readonly string[] Common =
    {
        "beep", "pause", "nothing",
        "0", "00 hours", "1", "2", "3", "4", "5", "6", "7", "8", "9",
        "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
        "20", "30", "40", "50", "60", "70", "80", "90",
        "100", "200", "300", "400", "500", "600", "700", "800", "900",
        "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m",
        "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z",
        "am", "pm", "and", "billion", "million", "thousand", "cent", "cents", "dollar", "dollars",
        "negative", "oh", "point", "pound", "star", "second", "seconds",
        "January", "February", "March", "April", "May", "June", "July", "August",
        "September", "October", "November", "December",
        "Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"
    };

    private static IEnumerable<string> Ordinals => Enumerable.Range(1, 31).Select(i => "ord" + i);

    public static readonly ISet<string> English =
        new HashSet<string>(Common.Concat(Ordinals), StringComparer.OrdinalIgnoreCase);

    public static readonly ISet<string> FrenchCanadian =
        new HashSet<string>(
            Common.Concat(Ordinals)
                .Concat(Enumerable.Range(21, 79).Where(i => i % 10 != 0).Select(i => i.ToString()))
                .Concat(new[]
                {
                    "80 compound", "hundred", "millions", "billions",
                    "le", "heure", "heures", "une", "21f", "minuit"
                }),
            StringComparer.OrdinalIgnoreCase);
}
