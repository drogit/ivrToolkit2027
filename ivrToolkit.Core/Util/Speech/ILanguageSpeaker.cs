using System;
using System.Threading.Tasks;

namespace ivrToolkit.Core.Util.Speech;

/// <summary>
/// Converts dates, times, numbers and money into a sequence of system recordings for one language.
/// </summary>
internal interface ILanguageSpeaker
{
    Task SpeakDateAsync(DateTime dateTime, string mask, SpeechContext context);
    Task SpeakIntegerAsync(long number, SpeechContext context);
    Task SpeakNumberAsync(double number, SpeechContext context);
    Task SpeakMoneyAsync(double number, SpeechContext context);
    Task SpeakOrdinalAsync(int number, SpeechContext context);
}
