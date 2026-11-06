using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.TTS;
using ivrToolkit.Core.Util;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.LineWrapperTests.PlayTests;

public class TextToSpeechVoiceOverrideTests
{
    private const string French = "fr-CA-SylvieNeural";
    private static readonly WavStream Wav = new(
    [
        82, 73, 70, 70, 100, 31, 0, 0, 87, 65, 86, 69, 102, 109, 116, 32,
        16, 0, 0, 0, 1, 0, 1, 0, 64, 31, 0, 0, 128, 62, 0, 0, 2, 0, 16, 0,
        100, 97, 116, 97, 64, 31, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
    ]);

    private static LineWrapper GetLineWrapper(VoiceProperties properties, Mock<ITextToSpeech> tts)
    {
        var baseLine = new Mock<IIvrBaseLine>();
        var cacheFactory = new TextToSpeechCacheFactory(new NullLoggerFactory(), tts.Object, new Mock<IFileHandler>().Object);
        return new LineWrapper(new NullLoggerFactory(), properties, 1, baseLine.Object, new TestPause(baseLine.Object), cacheFactory);
    }

    private static Mock<ITextToSpeech> GetTts()
    {
        var tts = new Mock<ITextToSpeech>(MockBehavior.Strict);
        tts.Setup(x => x.TextToSpeechAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(Wav);
        tts.Setup(x => x.TextToSpeechAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(Wav);
        return tts;
    }

    [Fact]
    public async Task NoOverride_UsesConfiguredVoice()
    {
        var tts = GetTts();
        var line = GetLineWrapper(new VoiceProperties(new NullLoggerFactory()), tts);

        await line.PlayTextToSpeechAsync("hello", CancellationToken.None);

        tts.Verify(x => x.TextToSpeechAsync("hello", It.IsAny<CancellationToken>()), Times.Once);
        tts.Verify(x => x.TextToSpeechAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task VoiceOverride_AffectsOnlyThatLine()
    {
        var properties = new VoiceProperties(new NullLoggerFactory());
        var tts1 = GetTts();
        var tts2 = GetTts();
        var line1 = GetLineWrapper(properties, tts1);
        var line2 = GetLineWrapper(properties, tts2);

        line1.TextToSpeechVoice = French;
        await line1.PlayTextToSpeechAsync("bonjour", CancellationToken.None);
        await line2.PlayTextToSpeechAsync("hello", CancellationToken.None);

        tts1.Verify(x => x.TextToSpeechAsync("bonjour", French, It.IsAny<CancellationToken>()), Times.Once);
        tts2.Verify(x => x.TextToSpeechAsync("hello", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubfolderOverride_IsUsedAsVoice()
    {
        var tts = GetTts();
        var line = GetLineWrapper(new VoiceProperties(new NullLoggerFactory()), tts);

        line.SystemRecordingSubfolder = French;
        await line.PlayTextToSpeechAsync("bonjour", CancellationToken.None);

        tts.Verify(x => x.TextToSpeechAsync("bonjour", French, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Hangup_ClearsVoiceOverride()
    {
        var tts = GetTts();
        var line = GetLineWrapper(new VoiceProperties(new NullLoggerFactory()), tts);

        line.TextToSpeechVoice = French;
        line.Hangup();

        line.TextToSpeechVoice.ShouldBeNull();
        await line.PlayTextToSpeechAsync("hello", CancellationToken.None);
        tts.Verify(x => x.TextToSpeechAsync("hello", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void CacheFactory_AddsVoiceToWavFileName()
    {
        var factory = new TextToSpeechCacheFactory(new NullLoggerFactory(), null, new Mock<IFileHandler>().Object);

        var cache = factory.Create("bonjour", System.IO.Path.Combine("cache", "greeting.wav"), French);

        cache.GetCacheFileName().ShouldBe(System.IO.Path.Combine("cache", $"greeting.{French}.wav"));
    }
}
