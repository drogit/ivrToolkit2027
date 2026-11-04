using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.LineWrapperTests.PlayTests;

public class PlayDateLanguageTests
{
    private static LineWrapper GetLineWrapper(string subfolder, out FakeLine fakeLine)
    {
        var loggerFactory = new NullLoggerFactory();
        var properties = new VoiceProperties(loggerFactory) { SystemRecordingSubfolder = subfolder };
        fakeLine = new FakeLine();
        var testPauser = new TestPause(fakeLine);
        return new LineWrapper(loggerFactory, properties, 1, fakeLine, testPauser);
    }

    private static string[] Paths(string subfolder, params string[] files) =>
        files.Select(f => $"System Recordings\\{subfolder}\\{f}.wav").ToArray();

    [Fact]
    public async Task French_PlayDateAsync()
    {
        const string folder = "fr-CA-SylvieNeural";
        var lineWrapper = GetLineWrapper(folder, out var fakeLine);
        await lineWrapper.PlayDateAsync(new DateTime(2025, 3, 1, 14, 23, 0), "m-d-yyyy h:n a/p", CancellationToken.None);

        fakeLine.PlayList.ShouldBe(Paths(folder, "le", "ord1", "March", "2", "thousand", "25", "14", "heures", "23"));
    }

    [Fact]
    public void French_PlayDate()
    {
        const string folder = "fr-CA-SylvieNeural";
        var lineWrapper = GetLineWrapper(folder, out var fakeLine);
        lineWrapper.PlayDate(new DateTime(2025, 3, 1, 21, 0, 0), "h:n");

        fakeLine.PlayList.ShouldBe(Paths(folder, "21f", "heures"));
    }

    [Fact]
    public async Task Original_PlayDateAsync_IsEnglish()
    {
        const string folder = "Original";
        var lineWrapper = GetLineWrapper(folder, out var fakeLine);
        await lineWrapper.PlayDateAsync(new DateTime(2025, 3, 1, 14, 23, 0), "m-d-yyyy h:n a/p", CancellationToken.None);

        fakeLine.PlayList.ShouldBe(Paths(folder, "March", "ord1", "2", "Thousand", "20", "5", "2", "20", "3", "pm"));
    }

    [Fact]
    public void Original_PlayDate_IsEnglish()
    {
        const string folder = "Original";
        var lineWrapper = GetLineWrapper(folder, out var fakeLine);
        lineWrapper.PlayDate(new DateTime(2025, 3, 1, 14, 0, 0), "h:n");

        fakeLine.PlayList.ShouldBe(Paths(folder, "14", "00 hours"));
    }
}
