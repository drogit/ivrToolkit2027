using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.TTS;
using ivrToolkit.Plugin.SipSorcery;
using Microsoft.Extensions.Logging;
using static Google.Rpc.Context.AttributeContext.Types;

namespace SystemRecordingsGenerator;

public class FrenchCanadian
{
    private readonly SipVoiceProperties _sipVoiceProperties;
    private readonly ITextToSpeech _ttsEngine;

    public FrenchCanadian(ILoggerFactory loggerFactory)
    {
        // this is one way to set up your properties, with a property file
        _sipVoiceProperties = new SipVoiceProperties(loggerFactory, @"c:\repos\Config\SipSorcery\voice.properties")
        {
            DefaultWavSampleRate = 8000,
            TtsAzureVoice = "fr-CA-SylvieNeural"
        };

        // i'm using azure TTS
        var ttsFactory = new AzureTtsFactory(loggerFactory, _sipVoiceProperties);
        _ttsEngine = ttsFactory.Create();
    }

    public void Generate()
    {
        var voicePath = Path.Combine("System Recordings", _sipVoiceProperties.TtsAzureVoice);
        Directory.CreateDirectory(voicePath);

        File.Copy(Path.Combine("System Recordings", "beep.wav"), Path.Combine(voicePath, "beep.wav"), true);
        File.Copy(Path.Combine("System Recordings", "pause.wav"), Path.Combine(voicePath, "pause.wav"), true);
        File.Copy(Path.Combine("System Recordings", "nothing.wav"), Path.Combine(voicePath, "nothing.wav"), true);

        Convert("0", "0.wav");
        Convert("heures", "00 hours.wav");
        Convert("1", "1.wav");
        Convert("10", "10.wav");
        Convert("100", "100.wav");
        Convert("11", "11.wav");
        Convert("12", "12.wav");
        Convert("13", "13.wav");
        Convert("14", "14.wav");
        Convert("15", "15.wav");
        Convert("16", "16.wav");
        Convert("17", "17.wav");
        Convert("18", "18.wav");
        Convert("19", "19.wav");
        Convert("2", "2.wav");
        Convert("20", "20.wav");
        Convert("200", "200.wav");
        Convert("3", "3.wav");
        Convert("30", "30.wav");
        Convert("300", "300.wav");
        Convert("4", "4.wav");
        Convert("40", "40.wav");
        Convert("400", "400.wav");
        Convert("5", "5.wav");
        Convert("50", "50.wav");
        Convert("500", "500.wav");
        Convert("6", "6.wav");
        Convert("60", "60.wav");
        Convert("600", "600.wav");
        Convert("7", "7.wav");
        Convert("70", "70.wav");
        Convert("700", "700.wav");
        Convert("8", "8.wav");
        Convert("80", "80.wav");
        Convert("800", "800.wav");
        Convert("9", "9.wav");
        Convert("90", "90.wav");
        Convert("900", "900.wav");
        Convert("a", "a.wav");
        Convert("<say-as interpret-as='characters'>am</say-as>", "am.wav");
        Convert("et", "and.wav");
        Convert("avril", "April.wav");
        Convert("août", "August.wav");
        Convert("b", "b.wav");
        Convert("milliard", "billion.wav");
        Convert("c", "c.wav");
        Convert("cent", "cent.wav");
        Convert("cents", "cents.wav");
        Convert("d", "d.wav");
        Convert("décembre", "December.wav");
        Convert("dollar", "dollar.wav");
        Convert("dollars", "dollars.wav");
        Convert("e", "e.wav");
        Convert("f", "f.wav");
        Convert("février", "February.wav");
        Convert("vendredi", "Friday.wav");
        Convert("g", "g.wav");
        Convert("h", "h.wav");
        Convert("i", "i.wav");
        Convert("j", "j.wav");
        Convert("janvier", "January.wav");
        Convert("juillet", "July.wav");
        Convert("juin", "June.wav");
        Convert("k", "k.wav");
        Convert("l", "l.wav");
        Convert("m", "m.wav");
        Convert("mars", "March.wav");
        Convert("mai", "May.wav");
        Convert("million", "million.wav");
        Convert("lundi", "Monday.wav");
        Convert("n", "n.wav");
        Convert("moins", "negative.wav");
        Convert("novembre", "November.wav");
        Convert("o", "o.wav");
        Convert("octobre", "October.wav");
        Convert("zéro", "oh.wav");
        Convert("premier", "ord1.wav");
        Convert("dixième", "ord10.wav");
        Convert("onzième", "ord11.wav");
        Convert("douzième", "ord12.wav");
        Convert("treizième", "ord13.wav");
        Convert("quatorzième", "ord14.wav");
        Convert("quinzième", "ord15.wav");
        Convert("seizième", "ord16.wav");
        Convert("dix-septième", "ord17.wav");
        Convert("dix-huitième", "ord18.wav");
        Convert("dix-neuvième", "ord19.wav");
        Convert("deuxième", "ord2.wav");
        Convert("vingtième", "ord20.wav");
        Convert("vingt et unième", "ord21.wav");
        Convert("vingt-deuxième", "ord22.wav");
        Convert("vingt-troisième", "ord23.wav");
        Convert("vingt-quatrième", "ord24.wav");
        Convert("vingt-cinquième", "ord25.wav");
        Convert("vingt-sixième", "ord26.wav");
        Convert("vingt-septième", "ord27.wav");
        Convert("vingt-huitième", "ord28.wav");
        Convert("vingt-neuvième", "ord29.wav");
        Convert("troisième", "ord3.wav");
        Convert("trentième", "ord30.wav");
        Convert("trente et unième", "ord31.wav");
        Convert("quatrième", "ord4.wav");
        Convert("cinquième", "ord5.wav");
        Convert("sixième", "ord6.wav");
        Convert("septième", "ord7.wav");
        Convert("huitième", "ord8.wav");
        Convert("neuvième", "ord9.wav");
        Convert("p", "p.wav");
        Convert("<say-as interpret-as='characters'>pm</say-as>", "pm.wav");
        Convert("virgule", "point.wav");
        Convert("carré", "pound.wav");
        Convert("q", "q.wav");
        Convert("r", "r.wav");
        Convert("s", "s.wav");
        Convert("samedi", "Saturday.wav");
        Convert("seconde", "second.wav");
        Convert("secondes", "seconds.wav");
        Convert("septembre", "September.wav");
        Convert("étoile", "star.wav");
        Convert("dimanche", "Sunday.wav");
        Convert("t", "t.wav");
        Convert("mille", "thousand.wav");
        Convert("jeudi", "Thursday.wav");
        Convert("mardi", "Tuesday.wav");
        Convert("u", "u.wav");
        Convert("v", "v.wav");
        Convert("w", "w.wav");
        Convert("mercredi", "Wednesday.wav");
        Convert("x", "x.wav");
        Convert("y", "y.wav");
        Convert("z", "z.wav");
    }

    private void Convert(string text, string output)
    {
        var audioStream = _ttsEngine.TextToSpeech($"{text} <break time='100ms'/>");
        var voicePath = Path.Combine("System Recordings", _sipVoiceProperties.TtsAzureVoice, output);
        File.WriteAllBytes(voicePath, audioStream.ToArray());
    }
}
