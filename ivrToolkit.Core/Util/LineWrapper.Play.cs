using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ivrToolkit.Core.Extensions;
using ivrToolkit.Core.Interfaces;
using ivrToolkit.Core.Util.Speech;
using Microsoft.Extensions.Logging;

namespace ivrToolkit.Core.Util;

internal partial class LineWrapper
{
    /// <summary>
    /// Plays a phone number as long as it is 7, 10 or 11 characters long.
    /// </summary>
    /// <param name="phoneNumber">The phone number must be 7, 10 or 11 characters long with no spaces or dashes. Just numbers.</param>
    public void PlayPhoneNumber(string phoneNumber)
    {
        if (phoneNumber == null) return;
        
        _logger.LogDebug("{method}({phoneNumber})", phoneNumber, nameof(PlayPhoneNumber));
        phoneNumber = phoneNumber.PadLeft(11);
        phoneNumber = phoneNumber.Substring(0, 1) + " " + phoneNumber.Substring(1, 3) + " " + 
                      phoneNumber.Substring(4, 3) + " " + phoneNumber.Substring(7, 4);
        PlayCharacters(phoneNumber.Trim());
    }
    
    public async Task PlayPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken)
    {
        if (phoneNumber == null) return;
        
        _logger.LogDebug("{method}({phoneNumber})", phoneNumber, nameof(PlayPhoneNumber));
        phoneNumber = phoneNumber.PadLeft(11);
        phoneNumber = phoneNumber.Substring(0, 1) + " " + phoneNumber.Substring(1, 3) + " " + 
                      phoneNumber.Substring(4, 3) + " " + phoneNumber.Substring(7, 4);
        await PlayCharactersAsync(phoneNumber.Trim(), cancellationToken);
    }

    /// <summary>
    /// Plays a DateTime object given based on the mask parameter.<br/>
    /// </summary>
    /// <param name="dateTime">Can be just the date, just the time or both</param>
    /// <param name="mask">Tells the voice plugin how to speak the datetime.</param>
    /// <remarks>
    ///
    /// Mask parts can be separated by the following characters: `:`, ` `, or `-`.<br/>
    /// <br/>
    /// <b>"m" "mm", or "mmm"</b>  - Speaks the month. Example: December<br/>
    /// <b>"d" or "dd"</b>     - Speaks the day of the month. Example: 3rd<br/>
    /// <b>"ddd" or "dddd"</b> - Speaks the day of the week. Example: Saturday<br/>
    /// <b>"yyy" or "yyyy"</b> - Speaks the year. Speak years 2010 to 2099 with the word "thousand".<br/>
    /// <b>"h" or "hh"</b>     - Speaks the hours. If your mask contains `a/p`, it is 12-hour time; otherwise, it is 24-hour time.<br/>
    /// <b>"n" or "nn"</b>     - Speaks the minutes.<br/>
    /// <b>"a/p"</b>           - Speaks either "am" or "pm".<br/>
    /// <para>
    /// The language comes from <see cref="VoiceProperties.SystemRecordingSubfolder"/> (see <see cref="Speech.LanguageSpeakerFactory"/>).
    /// For <c>fr-</c> subfolders the parts are spoken in French order and time is always 24-hour (<c>a/p</c> is ignored).
    /// <c>Original</c> is the legacy English folder.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    ///     PlayDate(myDateTime,"m-d-yyy h:m a/p");
    /// </code>
    /// </example>
    public void PlayDate(DateTime dateTime, string mask)
    {
        _logger.LogDebug("{method}({dateTime}, {mask})", nameof(PlayDate), dateTime, mask);
        Speaker.SpeakDateAsync(dateTime, mask, SyncContext()).GetAwaiter().GetResult();
    }

    public async Task PlayDateAsync(DateTime dateTime, string mask, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({dateTime}, {mask})", nameof(PlayDateAsync), dateTime, mask);
        await Speaker.SpeakDateAsync(dateTime, mask, AsyncContext(cancellationToken));
    }

    private SpeechContext SyncContext()
    {
        return new SpeechContext(file => { PlayF(file); return Task.CompletedTask; });
    }

    private SpeechContext AsyncContext(CancellationToken cancellationToken)
    {
        return new SpeechContext(async file => await PlayFAsync(file, cancellationToken));
    }

    /// <summary>
    /// Speaks a number in money format. For example 5.23 would be spoken as 'five dollars and twenty three cents'
    /// </summary>
    /// <param name="number">The number you want to speak</param>
    public void PlayMoney(double number)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayMoney), number);
        Speaker.SpeakMoneyAsync(number, SyncContext()).GetAwaiter().GetResult();
    }

    public async Task PlayMoneyAsync(double number, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayMoneyAsync), number);
        await Speaker.SpeakMoneyAsync(number, AsyncContext(cancellationToken));
    }

    /// <summary>
    /// Speaks out the digits in the string.
    /// </summary>
    /// <param name="characters">0-9, a-z, # and *</param>
    public void PlayCharacters(string characters)
    {
        _logger.LogDebug("{method}({characters})", nameof(PlayCharacters), characters);
        PlayCharactersInternalAsync(characters,
            chars => { PlayF(chars); return Task.CompletedTask; }
        ).GetAwaiter().GetResult();
    }
    public async Task PlayCharactersAsync(string characters, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({characters})", nameof(PlayCharactersAsync), characters);
        await PlayCharactersInternalAsync(characters,
            async chars => await PlayFAsync(chars, cancellationToken));
    }

    private async Task PlayCharactersInternalAsync(string characters, 
        Func<string, Task> playF)
    {
        if (characters == null) return;
        
        _logger.LogDebug("{method}({characters})", nameof(PlayCharactersInternalAsync), characters);
        var chars = characters.ToCharArray();
        foreach (var c in chars)
        {
            switch (c)
            {
                case ' ':
                    await _pauseHandler.PauseAsync(500, CancellationToken.None);
                    break;
                case '*':
                    await playF("star");
                    break;
                case '#':
                    await playF("pound");
                    break;
                default:
                    if (c is >= 'A' and <= 'Z' or >= 'a' and <= 'z' or >= '0' and <= '9')
                    {
                        await playF(c.ToString(CultureInfo.InvariantCulture));
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Speaks out a number. For example 5.23 would be spoken as 'five point two three'
    /// </summary>
    /// <param name="number">The number to speak out</param>
    public void PlayNumber(double number)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayNumber), number);
        Speaker.SpeakNumberAsync(number, SyncContext()).GetAwaiter().GetResult();
    }

    public async Task PlayNumberAsync(double number, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayNumberAsync), number);
        await Speaker.SpeakNumberAsync(number, AsyncContext(cancellationToken));
    }

    /// <summary>
    /// Same as PlayNumber but for an integer.
    /// </summary>
    /// <param name="number">The integer to speak out.</param>
    public void PlayInteger(long number)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayInteger), number);
        Speaker.SpeakIntegerAsync(number, SyncContext()).GetAwaiter().GetResult();
    }

    public async Task PlayIntegerAsync(long number, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayIntegerAsync), number);
        await Speaker.SpeakIntegerAsync(number, AsyncContext(cancellationToken));
    }
        
    /// <summary>
    /// Plays a number from 1 to 31. Example 31 would speak 'thirty first'.
    /// </summary>
    /// <param name="number">A number between 1 and 31</param>
    public void PlayOrdinal(int number)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayOrdinal), number);
        Speaker.SpeakOrdinalAsync(number, SyncContext()).GetAwaiter().GetResult();
    }

    public async Task PlayOrdinalAsync(int number, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({number})", nameof(PlayOrdinalAsync), number);
        await Speaker.SpeakOrdinalAsync(number, AsyncContext(cancellationToken));
    }

    /// <summary>
    /// Plays a collection of one or more string parts which are separated with a comma. Each string part is in the format of 'data|code'. You would string them together like: 'data|code,data|code,data|code' etc.
    /// </summary>
    /// <remarks>
    /// Each string part is in the format of 'data|code'. You would string them together like: 'data|code,data|code,data|code' etc.
    ///     <table>
    ///         <tr><th>Codes</th><th>Data</th></tr>
    ///         <tr><td> C </td><td> Characters that will be played with the PlayCharater method </td></tr>
    ///         <tr><td> D </td><td> Expects the data to be a date in month/day/year format. Can be a date, time or both date and time. Uses DateTime.Parse(data, new CultureInfo("en-US"));</td></tr>
    ///         <tr><td> F </td><td> A file name </td></tr>
    ///         <tr><td> M </td><td> A string that can convert to a double value and spoken with the PlayMoney method </td></tr>
    ///         <tr><td> N </td><td> A string that can convert to a double value and spoken with the PlayNumber method </td></tr>
    ///         <tr><td> O </td><td> A string number between 1 and 31 that will be spoken with the PlayOrdinal method </td></tr>
    ///     </table>
    /// </remarks>
    /// <param name="str">The string to interpret in the format of 'data|code,data|code,data|code'...</param>
    public void PlayString(string str)
    {
        _logger.LogDebug("{method}({str})", nameof(PlayString), str);
        PlayStringInternalAsync(
            str,
            characters => { PlayCharacters(characters); return Task.CompletedTask; },
            file => { PlayFile(file); return Task.CompletedTask; },
            money => { PlayMoney(money); return Task.CompletedTask; },
            nmbr => { PlayNumber(nmbr); return Task.CompletedTask; },
            ordinal => { PlayOrdinal(ordinal); return Task.CompletedTask; },
            (dt, msk) => { PlayDate(dt, msk); return Task.CompletedTask; }).GetAwaiter().GetResult();
    }
        
    public async Task PlayStringAsync(string str, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({str})", nameof(PlayStringAsync), str);
        await PlayStringInternalAsync(
            str,
            async characters => await PlayCharactersAsync(characters, cancellationToken),
            async file => await PlayFileAsync(file, cancellationToken),
            async money => await PlayMoneyAsync(money, cancellationToken),
            async nmbr => await PlayNumberAsync(nmbr, cancellationToken),
            async ordinal => await PlayOrdinalAsync(ordinal, cancellationToken),
            async (dt, msk) => await PlayDateAsync(dt, msk, cancellationToken));
    }

    private async Task PlayStringInternalAsync(string str,
        Func<string, Task> playCharacters,
        Func<string, Task> playFile,
        Func<double, Task> playMoney,
        Func<double, Task> playNumber,
        Func<int, Task> playOrdinal,
        Func<DateTime, string, Task> playDate)
    {
        _logger.LogDebug("{method}({str})", nameof(PlayStringInternalAsync), str);
        var parts = str.Split(',', StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var sections = part.Split('|', StringSplitOptions.RemoveEmptyEntries);
            if (sections.Length is not (2 or 3))
            {
                throw new FormatException($"Requires 1 or 2 pipes: {part}.");
            }
            
            string mask = null;
            var data = sections[0];
            var command = sections[1].ToLower();
            if (sections.Length > 2)
            {
                mask = sections[2];
            }
            switch (command)
            {
                // character
                case "c":
                    await playCharacters(data);
                    break;
                // date
                case "d":
                {
                    // US english culture does month/day/year instead of day/month/year
                    var dt = DateTime.Parse(data, new CultureInfo("en-US"));
                    await playDate(dt, mask);
                    break;
                }
                // file
                case "f":
                    // TODO these are not system files - legacy todo statement
                    await playFile(data);
                    break;
                // money
                case "m":
                    await playMoney(double.Parse(data));
                    break;
                // number
                case "n":
                    await playNumber(double.Parse(data));
                    break;
                // ordinal
                case "o":
                    await playOrdinal(int.Parse(data));
                    break;
                default:
                    throw new FormatException($"Invalid command: {command}");
            }
        }
    }

    private void PlayF(string filename)
    {
        _logger.LogDebug("{method}({fileName})", nameof(PlayF), filename);
        PlayFile(Path.Combine(SystemRecordingsRoot, $"{filename}.wav"));
    }
        
    private async Task PlayFAsync(string filename, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({fileName})", nameof(PlayFAsync), filename);
        await PlayFileAsync(Path.Combine(SystemRecordingsRoot, $"{filename}.wav"), cancellationToken);
    }
    
    public void PlayFileOrPhrase(string fileNameOrPhrase)
    {
        _logger.LogDebug("{method}({fileNameOrPhrase})", nameof(PlayFileOrPhrase), fileNameOrPhrase);
        if (fileNameOrPhrase.IndexOf("|", StringComparison.Ordinal) != -1)
        {
            PlayString(fileNameOrPhrase);
        }
        else
        {
            PlayFile(fileNameOrPhrase);
        }
    }
        
    public async Task PlayFileOrPhraseAsync(string fileNameOrPhrase, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({fileNameOrPhrase})", nameof(PlayFileOrPhraseAsync), fileNameOrPhrase);
        if (fileNameOrPhrase.IndexOf("|", StringComparison.Ordinal) != -1)
        {
            await PlayStringAsync(fileNameOrPhrase, cancellationToken);
        }
        else
        {
            await PlayFileAsync(fileNameOrPhrase, cancellationToken);
        }
    }

    public void PlayTextToSpeech(string textToSpeech)
    {
        PlayTextToSpeechAsync(textToSpeech, CancellationToken.None).GetAwaiter().GetResult();
    }
    
    public void PlayTextToSpeech(ITextToSpeechCache textToSpeechCache)
    {
        PlayTextToSpeechAsync(textToSpeechCache, CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task PlayTextToSpeechAsync(string textToSpeech, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({textToSpeech})", nameof(PlayTextToSpeechAsync), textToSpeech);
        CheckDispose();
        
        var voice = EffectiveTextToSpeechVoice;
        var textToSpeechBuilder = voice == null
            ? _textToSpeechCacheFactory.Create(textToSpeech)
            : _textToSpeechCacheFactory.Create(textToSpeech, null, voice);

        var audioStream = await textToSpeechBuilder.GetOrGenerateCacheAsync(cancellationToken);
        await _lineImplementation.PlayWavStreamAsync(audioStream, cancellationToken);
    }

    public async Task PlayTextToSpeechAsync(ITextToSpeechCache textToSpeechCache, CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({textToSpeechBuilder})", nameof(PlayTextToSpeechAsync), textToSpeechCache);
        CheckDispose();
        textToSpeechCache.ThrowIfNull(nameof(textToSpeechCache));

        var audioStream = await textToSpeechCache.GetOrGenerateCacheAsync(cancellationToken);
        await _lineImplementation.PlayWavStreamAsync(audioStream, cancellationToken);
    }

    public void PlayFile(ITextToSpeechCache textToSpeechCache)
    {
        PlayFileAsync(textToSpeechCache, CancellationToken.None).GetAwaiter().GetResult();
    }

    public async Task PlayFileAsync(ITextToSpeechCache textToSpeechCache,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("{method}({textToSpeechBuilder})", nameof(PlayFileAsync), textToSpeechCache);
        CheckDispose();
        
        textToSpeechCache.ThrowIfNull(nameof(textToSpeechCache));
        // legacy plugins don't have PlayWavStreamAsync implemented
        // but we still want to give them the ability to do TTS
        
        // throws VoiceException if FileName is missing
        await textToSpeechCache.GenerateCacheAsync(cancellationToken);
        await _lineImplementation.PlayFileAsync(textToSpeechCache.GetCacheFileName(), cancellationToken);
    }
    
    public void PlayWavStream(WavStream audioStream)
    {
        _lineImplementation.PlayWavStream(audioStream);
    }

    public async Task PlayWavStreamAsync(WavStream audioStream, CancellationToken cancellationToken)
    {
        await _lineImplementation.PlayWavStreamAsync(audioStream, cancellationToken);
    }
}
