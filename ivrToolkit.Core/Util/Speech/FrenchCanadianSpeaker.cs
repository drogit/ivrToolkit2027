using System;
using System.Globalization;
using System.Threading.Tasks;

namespace ivrToolkit.Core.Util.Speech;

/// <summary>
/// French Canadian speaking rules.
/// </summary>
/// <remarks>
/// Recording names keep the English file names where the meaning is the same (for example "March.wav" contains "mars").
/// Numbers 1 to 99 each have their own recording because French tens (soixante-dix, quatre-vingt-dix, vingt et un...)
/// can't be built from smaller parts.
/// </remarks>
internal sealed class FrenchCanadianSpeaker : ILanguageSpeaker
{
    internal static readonly string[] Months =
    {
        "January", "February", "March", "April", "May", "June", "July", "August", "September", "October",
        "November", "December"
    };

    /// <summary>
    /// The mask only decides which parts are spoken. The parts are always spoken in French order:
    /// [day of week] [le day] [month] [year] [hour heures] [minutes]. Time is always 24-hour and "a/p" is ignored.
    /// </summary>
    public async Task SpeakDateAsync(DateTime dateTime, string mask, SpeechContext context)
    {
        var playF = context.PlayF;
        bool dayOfWeek = false, day = false, month = false, year = false, hour = false, minute = false;

        foreach (var part in mask.ToLower().Split(' ', ':', '-'))
        {
            switch (part)
            {
                case "m" or "mm" or "mmm": month = true; break;
                case "d" or "dd": day = true; break;
                case "ddd" or "dddd": dayOfWeek = true; break;
                case "yyy" or "yyyy": year = true; break;
                case "h" or "hh": hour = true; break;
                case "n" or "nn": minute = true; break;
            }
        }

        if (dayOfWeek)
        {
            await playF(Enum.Format(typeof(DayOfWeek), dateTime.DayOfWeek, "G"));
        }

        if (day)
        {
            await playF("le");
            if (dateTime.Day == 1)
            {
                // "le premier"
                await playF("ord1");
            }
            else
            {
                await SpeakIntegerAsync(dateTime.Day, context);
            }
        }

        if (month)
        {
            await playF(Months[dateTime.Month - 1]);
        }

        if (year)
        {
            await SpeakIntegerAsync(dateTime.Year, context);
        }

        if (hour)
        {
            switch (dateTime.Hour)
            {
                case 0:
                    await playF("minuit");
                    break;
                case 1:
                    await playF("une");
                    await playF("heure");
                    break;
                case 21:
                    await playF("21f");
                    await playF("heures");
                    break;
                default:
                    await SpeakIntegerAsync(dateTime.Hour, context);
                    await playF("heures");
                    break;
            }
        }

        if (minute && dateTime.Minute > 0)
        {
            await SpeakIntegerAsync(dateTime.Minute, context);
        }
    }

    public async Task SpeakIntegerAsync(long number, SpeechContext context)
    {
        var playF = context.PlayF;
        if (number < 0)
        {
            await playF("negative");
            number *= -1;
        }
        if (number == 0)
        {
            await playF("0");
            return;
        }
        const long billion = 1000000000;
        const long million = 1000000;
        const long thousand = 1000;

        var billions = number / billion;
        var rest = number % billion;
        if (billions > 0)
        {
            await SpeakUpTo999Async(billions, false, context);
            await playF(billions == 1 ? "billion" : "billions");
        }

        var millions = rest / million;
        rest %= million;
        if (millions > 0)
        {
            await SpeakUpTo999Async(millions, false, context);
            await playF(millions == 1 ? "million" : "millions");
        }

        var thousands = rest / thousand;
        rest %= thousand;
        if (thousands > 0)
        {
            // "mille", never "un mille"
            if (thousands > 1)
            {
                await SpeakUpTo999Async(thousands, true, context);
            }
            await playF("thousand");
        }
        if (rest > 0)
        {
            await SpeakUpTo999Async(rest, false, context);
        }
    }

    public async Task SpeakNumberAsync(double number, SpeechContext context)
    {
        var playF = context.PlayF;
        var n = number.ToString("G", CultureInfo.InvariantCulture);
        var index = n.IndexOf(".", StringComparison.Ordinal);
        var whole = index == -1 ? n : n.Substring(0, index);
        var fraction = index == -1 ? "" : n.Substring(index + 1);

        await SpeakIntegerAsync(long.Parse(whole, CultureInfo.InvariantCulture), context);
        if (fraction != "")
        {
            // "point.wav" contains "virgule"
            await playF("point");
            foreach (var c in fraction)
            {
                await playF(c.ToString(CultureInfo.InvariantCulture));
            }
        }
    }

    public async Task SpeakMoneyAsync(double number, SpeechContext context)
    {
        var playF = context.PlayF;
        var numberAsString = number.ToString("F2", CultureInfo.InvariantCulture);
        var index = numberAsString.IndexOf(".", StringComparison.Ordinal);
        var whole = long.Parse(numberAsString.Substring(0, index), CultureInfo.InvariantCulture);
        var cents = long.Parse(numberAsString.Substring(index + 1), CultureInfo.InvariantCulture);

        await SpeakIntegerAsync(whole, context);
        await playF(Math.Abs(whole) <= 1 ? "dollar" : "dollars");
        if (cents > 0)
        {
            // "and.wav" contains "et"
            await playF("and");
            await SpeakIntegerAsync(cents, context);
            await playF(cents == 1 ? "cent" : "cents");
        }
    }

    public Task SpeakOrdinalAsync(int number, SpeechContext context)
    {
        return context.PlayF("ord" + number);
    }

    /// <param name="number">1 to 999</param>
    /// <param name="beforeMille">"cents" and "quatre-vingts" lose their "s" when followed by "mille".</param>
    /// <param name="context">The speech context.</param>
    private static async Task SpeakUpTo999Async(long number, bool beforeMille, SpeechContext context)
    {
        var playF = context.PlayF;
        var hundreds = number / 100;
        var rest = number % 100;

        if (hundreds == 1)
        {
            await playF("100");
        }
        else if (hundreds > 1)
        {
            if (rest == 0 && !beforeMille)
            {
                // "deux cents"
                await playF(hundreds + "00");
            }
            else
            {
                // "deux cent trois" or "deux cent mille"
                await playF(hundreds.ToString(CultureInfo.InvariantCulture));
                await playF("hundred");
            }
        }

        if (rest == 0) return;

        if (rest == 80 && beforeMille)
        {
            // "quatre-vingt mille"
            await playF("80 compound");
            return;
        }

        await playF(rest.ToString(CultureInfo.InvariantCulture));
    }
}
