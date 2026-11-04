using ivrToolkit.Core.Util.Speech;
using Shouldly;
using Xunit;

namespace ivrToolkit.Core.Tests.SpeechTests;

public class LanguageSpeakerFactoryTests
{
    [Theory]
    [InlineData("Original")]
    [InlineData("original")]
    [InlineData("en-US-JennyNeural")]
    [InlineData("")]
    [InlineData(null)]
    public void English(string subfolder)
    {
        LanguageSpeakerFactory.Create(subfolder).ShouldBeOfType<EnglishSpeaker>();
    }

    [Theory]
    [InlineData("fr-CA-SylvieNeural")]
    [InlineData("FR-ca-x")]
    public void FrenchCanadian(string subfolder)
    {
        LanguageSpeakerFactory.Create(subfolder).ShouldBeOfType<FrenchCanadianSpeaker>();
    }
}
