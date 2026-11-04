using System;
using System.Threading.Tasks;
using ivrToolkit.Core.Util.Speech;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.SpeechTests;

public class EnglishSpeakerTests
{
    private readonly EnglishSpeaker _speaker = new();
    private readonly SpeechRecorder _recorder = new();

    private void ShouldPlay(params string[] expected)
    {
        _recorder.Played.ShouldBe(expected);
        _recorder.ShouldAllExistIn(SystemRecordings.English);
    }

    [Fact]
    public async Task Date_MonthDayYear()
    {
        await _speaker.SpeakDateAsync(new DateTime(2025, 3, 1), "m-d-yyyy", _recorder.Context);
        ShouldPlay("March", "ord1", "2", "Thousand", "20", "5");
    }

    [Fact]
    public async Task Date_DayOfWeek()
    {
        await _speaker.SpeakDateAsync(new DateTime(2025, 1, 20), "ddd", _recorder.Context);
        ShouldPlay("Monday");
    }

    [Theory]
    [InlineData(14, 23, "h:n", new[] { "14", "20", "3" })]
    [InlineData(14, 0, "h:n", new[] { "14", "00 hours" })]
    [InlineData(0, 5, "h:n", new[] { "o", "00 hours", "o", "5" })]
    [InlineData(14, 23, "h:n a/p", new[] { "2", "20", "3", "pm" })]
    [InlineData(0, 2, "h:n a/p", new[] { "12", "o", "2", "am" })]
    public async Task Time(int hour, int minute, string mask, string[] expected)
    {
        await _speaker.SpeakDateAsync(new DateTime(2025, 1, 24, hour, minute, 0), mask, _recorder.Context);
        ShouldPlay(expected);
    }

    [Theory]
    [InlineData(0, new[] { "0" })]
    [InlineData(71, new[] { "70", "1" })]
    [InlineData(200, new[] { "200" })]
    [InlineData(203, new[] { "200", "3" })]
    [InlineData(1000, new[] { "1", "Thousand" })]
    [InlineData(2025, new[] { "2", "Thousand", "20", "5" })]
    [InlineData(-5, new[] { "negative", "5" })]
    [InlineData(1000000, new[] { "1", "Million" })]
    public async Task Integer(long number, string[] expected)
    {
        await _speaker.SpeakIntegerAsync(number, _recorder.Context);
        ShouldPlay(expected);
    }

    [Theory]
    [InlineData(1.00, new[] { "1", "dollar", "and", "0", "cents" })]
    [InlineData(2.01, new[] { "2", "dollars", "and", "1", "cent" })]
    public async Task Money(double number, string[] expected)
    {
        await _speaker.SpeakMoneyAsync(number, _recorder.Context);
        ShouldPlay(expected);
    }

    [Fact]
    public async Task Number()
    {
        await _speaker.SpeakNumberAsync(5.23, _recorder.Context);
        ShouldPlay("5", "point", "2", "3");
    }

    [Fact]
    public async Task Ordinals_1_To_31()
    {
        for (var i = 1; i <= 31; i++)
        {
            await _speaker.SpeakOrdinalAsync(i, _recorder.Context);
            _recorder.Played[^1].ShouldBe("ord" + i);
        }
        _recorder.ShouldAllExistIn(SystemRecordings.English);
    }
}
