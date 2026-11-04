using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ivrToolkit.Core.Util.Speech;

/// <summary>
/// English speaking rules. This is the original ivrToolkit behaviour and is also used for the legacy "Original" recordings.
/// </summary>
internal sealed class EnglishSpeaker : ILanguageSpeaker
{
    private static readonly string[] Months =
    {
        "January", "February", "March", "April", "May", "June", "July", "August", "September", "October",
        "November", "December"
    };

    private static bool Is24Hour(string mask)
    {
        var parts = mask.Split(' ', ':', '-');
        return parts.All(part => part != "a/p");
    }

    public async Task SpeakDateAsync(DateTime dateTime, string mask, SpeechContext context)
    {
        var playF = context.PlayF;
        mask = mask.ToLower();
        var parts = mask.Split(' ', ':', '-');
        foreach (var part in parts)
        {
            switch (part)
            {
                case "m" or "mm" or "mmm":
                {
                    var m = dateTime.Month;
                    if (m is > 0 and < 13)
                    {
                        await playF(Months[m - 1]);
                    }

                    break;
                }
                case "d" or "dd":
                    await playF("ord" + dateTime.Day);
                    break;
                case "ddd" or "dddd":
                {
                    var day = Enum.Format(typeof(DayOfWeek), dateTime.DayOfWeek, "G");
                    await playF(day);
                    break;
                }
                case "yyy" or "yyyy":
                {
                    var year = dateTime.Year.ToString(CultureInfo.InvariantCulture);
                    // speak years 2010 to 2099 with the word thousand
                    await SpeakYearThousandsAsync(year, context);
                    break;
                }
                case "h" or "hh":
                {
                    var h = dateTime.Hour;
                    if (Is24Hour(mask))
                    {
                        if (h < 10)
                        {
                            await playF("o");
                        }
                        if (h > 0)
                        {
                            await SpeakIntegerAsync(h, context);
                        }
                        var m = dateTime.Minute;
                        if (m == 0 || h == 0)
                        {
                            await playF("00 hours");
                        }
                    }
                    else
                    {
                        switch (h)
                        {
                            case 0:
                                await playF("12");
                                break;
                            case > 12:
                                await SpeakIntegerAsync(h - 12, context);
                                break;
                            default:
                                await SpeakIntegerAsync(h, context);
                                break;
                        }
                    }

                    break;
                }
                case "n" or "nn":
                {
                    var m = dateTime.Minute;
                    switch (m)
                    {
                        case > 0 and < 10:
                            await playF("o");
                            await playF(m.ToString(CultureInfo.InvariantCulture));
                            break;
                        case >= 10:
                            await SpeakIntegerAsync(m, context);
                            break;
                    }

                    break;
                }
                case "a/p":
                    await playF(dateTime.Hour < 12 ? "am" : "pm");
                    break;
            }
        }
    }

    private async Task SpeakYearThousandsAsync(string year, SpeechContext context)
    {
        var playF = context.PlayF;
        var y1 = year.Substring(0, 2);
        var y2 = year.Substring(2, 2);
        var y1Int = int.Parse(y1);
        var y2Int = int.Parse(y2);

        if (y1.EndsWith("0"))
        {
            await playF(y1.Substring(0, 1));
            await playF("Thousand");
            if (y2Int > 0)
            {
                await SpeakIntegerAsync(y2Int, context);
            }
        }
        else
        {
            await SpeakIntegerAsync(y1Int, context);
            if (y2Int == 0)
            {
                await playF("o");
                await playF("o");
            }
            else if (y2Int < 10)
            {
                await playF("o");
                await playF(y2Int.ToString(CultureInfo.InvariantCulture));
            }
            else
            {
                await SpeakIntegerAsync(y2Int, context);
            }
        }
    }

    public async Task SpeakMoneyAsync(double number, SpeechContext context)
    {
        var playF = context.PlayF;
        var numberAsString = number.ToString("F"); // two decimal places
        var index = numberAsString.IndexOf(".", StringComparison.Ordinal);
        var wholePart = numberAsString.Substring(0, index);
        var fractionPart = numberAsString.Substring(index + 1);

        var whole = long.Parse(wholePart);
        await SpeakIntegerAsync(whole, context);
        await playF(whole == 1 ? "dollar" : "dollars");
        if (fractionPart != "")
        {
            await playF("and");
            var cents = long.Parse(fractionPart);
            await SpeakIntegerAsync(cents, context);
            await playF(cents == 1 ? "cent" : "cents");
        }
    }

    public async Task SpeakNumberAsync(double number, SpeechContext context)
    {
        var playF = context.PlayF;
        var n = number.ToString("G");
        var index = n.IndexOf(".", StringComparison.Ordinal);
        string whole;
        var fraction = "";
        if (index == -1)
        {
            whole = n;
        }
        else
        {
            whole = n.Substring(0, index);
            fraction = n.Substring(index + 1);
        }
        await SpeakIntegerAsync(long.Parse(whole), context);
        if (fraction != "")
        {
            await playF("point");
            foreach (var c in fraction)
            {
                await playF(c.ToString(CultureInfo.InvariantCulture));
            }
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
            await SpeakUpTo999Async(billions, context);
            await playF("Billion");
        }

        var millions = rest / million;
        rest %= million;
        if (millions > 0)
        {
            await SpeakUpTo999Async(millions, context);
            await playF("Million");
        }

        var thousands = rest / thousand;
        rest %= thousand;
        if (thousands > 0)
        {
            await SpeakUpTo999Async(thousands, context);
            await playF("Thousand");
        }
        if (rest > 0)
        {
            await SpeakUpTo999Async(rest, context);
        }
    }

    public Task SpeakOrdinalAsync(int number, SpeechContext context)
    {
        return context.PlayF("ord" + number);
    }

    private static async Task SpeakUpTo999Async(long number, SpeechContext context)
    {
        var playF = context.PlayF;
        var hundreds = number / 100;
        var rest = number % 100;
        if (hundreds > 0)
        {
            await playF(hundreds + "00");
        }
        if (rest == 0) return;
        if (rest < 20)
        {
            await playF(rest.ToString(CultureInfo.InvariantCulture));
            return;
        }
        var n = rest.ToString(CultureInfo.InvariantCulture);
        await playF(n.Substring(0, 1) + "0");
        if (n.Substring(1, 1) != "0")
        {
            await playF(n.Substring(1, 1));
        }
    }
}
