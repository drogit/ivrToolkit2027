using System;
using System.Threading.Tasks;
using ivrToolkit.Core.Util.Speech;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.SpeechTests;

public class FrenchCanadianSpeakerTests
{
    private readonly FrenchCanadianSpeaker _speaker = new();
    private readonly SpeechRecorder _recorder = new();

    private void ShouldPlay(params string[] expected)
    {
        _recorder.Played.ShouldBe(expected);
        _recorder.ShouldAllExistIn(SystemRecordings.FrenchCanadian);
    }

    [Theory]
    [InlineData("m-d-yyyy")]
    [InlineData("d-m-yyyy")]
    [InlineData("yyyy-m-d")]
    [InlineData("yyyy m d")]
    public async Task Date_MaskOrderIsIgnored(string mask)
    {
        await _speaker.SpeakDateAsync(new DateTime(2025, 3, 1), mask, _recorder.Context);
        ShouldPlay("le", "ord1", "March", "2", "thousand", "25");
    }

    [Fact]
    public async Task Date_DayIsCardinalExceptTheFirst()
    {
        await _speaker.SpeakDateAsync(new DateTime(2025, 3, 2), "d", _recorder.Context);
        ShouldPlay("le", "2");
    }

    [Fact]
    public async Task Date_DayOfWeekDateAndTime()
    {
        await _speaker.SpeakDateAsync(new DateTime(2025, 3, 3, 14, 23, 0), "h:n dddd m-d-yyyy", _recorder.Context);
        ShouldPlay("Monday", "le", "3", "March", "2", "thousand", "25", "14", "heures", "23");
    }

    [Theory]
    [InlineData(14, 23, "h:n", new[] { "14", "heures", "23" })]
    [InlineData(14, 0, "h:n", new[] { "14", "heures" })]
    [InlineData(14, 5, "h:n", new[] { "14", "heures", "5" })]
    [InlineData(0, 5, "h:n", new[] { "minuit", "5" })]
    [InlineData(1, 15, "h:n", new[] { "une", "heure", "15" })]
    [InlineData(21, 0, "h:n", new[] { "21f", "heures" })]
    [InlineData(14, 23, "h:n a/p", new[] { "14", "heures", "23" })]
    [InlineData(2, 2, "h:n a/p", new[] { "2", "heures", "2" })]
    public async Task Time_Is24HourFrench(int hour, int minute, string mask, string[] expected)
    {
        await _speaker.SpeakDateAsync(new DateTime(2025, 1, 24, hour, minute, 0), mask, _recorder.Context);
        ShouldPlay(expected);
        _recorder.Played.ShouldNotContain("o");
        _recorder.Played.ShouldNotContain("am");
        _recorder.Played.ShouldNotContain("pm");
    }

    [Theory]
    [InlineData(0, new[] { "0" })]
    [InlineData(21, new[] { "21" })]
    [InlineData(71, new[] { "71" })]
    [InlineData(80, new[] { "80" })]
    [InlineData(99, new[] { "99" })]
    [InlineData(100, new[] { "100" })]
    [InlineData(200, new[] { "200" })]
    [InlineData(203, new[] { "2", "hundred", "3" })]
    [InlineData(1000, new[] { "thousand" })]
    [InlineData(2025, new[] { "2", "thousand", "25" })]
    [InlineData(80000, new[] { "80 compound", "thousand" })]
    [InlineData(200000, new[] { "2", "hundred", "thousand" })]
    [InlineData(1000000, new[] { "1", "million" })]
    [InlineData(2000000, new[] { "2", "millions" })]
    [InlineData(-5, new[] { "negative", "5" })]
    public async Task Integer(long number, string[] expected)
    {
        await _speaker.SpeakIntegerAsync(number, _recorder.Context);
        ShouldPlay(expected);
    }

    [Theory]
    [InlineData(1.00, new[] { "1", "dollar" })]
    [InlineData(2.01, new[] { "2", "dollars", "and", "1", "cent" })]
    [InlineData(5.23, new[] { "5", "dollars", "and", "23", "cents" })]
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
        _recorder.ShouldAllExistIn(SystemRecordings.FrenchCanadian);
    }
}
