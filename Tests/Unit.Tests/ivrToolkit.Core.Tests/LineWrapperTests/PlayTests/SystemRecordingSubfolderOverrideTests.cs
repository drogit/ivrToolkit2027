using System.IO;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.LineWrapperTests.PlayTests;

public class SystemRecordingSubfolderOverrideTests
{
    private const string Override = "fr-CA-SylvieNeural";

    private static LineWrapper GetLineWrapper(VoiceProperties properties, int lineNumber, out FakeLine fakeLine)
    {
        fakeLine = new FakeLine();
        var testPauser = new TestPause(fakeLine);
        return new LineWrapper(new NullLoggerFactory(), properties, lineNumber, fakeLine, testPauser);
    }

    private static string Prefix(string subfolder) => Path.Combine("System Recordings", subfolder) + Path.DirectorySeparatorChar;

    [Fact]
    public void Override_AffectsOnlyThatLine()
    {
        var properties = new VoiceProperties(new NullLoggerFactory());
        var defaultFolder = properties.SystemRecordingSubfolder;

        var line1 = GetLineWrapper(properties, 1, out var fakeLine1);
        var line2 = GetLineWrapper(properties, 2, out var fakeLine2);

        line1.SystemRecordingSubfolder = Override;

        line1.PlayCharacters("5");
        line2.PlayCharacters("5");

        fakeLine1.PlayList.ShouldNotBeEmpty();
        fakeLine1.PlayList.ShouldAllBe(p => p.StartsWith(Prefix(Override)));
        fakeLine2.PlayList.ShouldNotBeEmpty();
        fakeLine2.PlayList.ShouldAllBe(p => p.StartsWith(Prefix(defaultFolder)));
        properties.SystemRecordingSubfolder.ShouldBe(defaultFolder);
    }

    [Fact]
    public void Override_SetBackToNull_RestoresDefault()
    {
        var properties = new VoiceProperties(new NullLoggerFactory());
        var defaultFolder = properties.SystemRecordingSubfolder;

        var line = GetLineWrapper(properties, 1, out var fakeLine);

        line.SystemRecordingSubfolder = Override;
        line.SystemRecordingSubfolder = null;

        line.PlayCharacters("5");

        fakeLine.PlayList.ShouldNotBeEmpty();
        fakeLine.PlayList.ShouldAllBe(p => p.StartsWith(Prefix(defaultFolder)));
    }
}
