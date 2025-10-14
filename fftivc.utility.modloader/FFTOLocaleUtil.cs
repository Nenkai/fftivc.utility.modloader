using fftivc.utility.modloader.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Windows.Win32;

namespace fftivc.utility.modloader;

/// <summary>
/// Locale/language util, reversed from the game.
/// </summary>
public class FFTOLocaleUtil
{
    public static readonly Dictionary<uint, FFTOLanguageType> LCIDToGameLanguage = new()
    {
        // https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/63d3d639-7fd2-4afb-abbe-0d5b5551eef8

        // Japanese
        [0x0411] = FFTOLanguageType.Japanese, // jp-JP

        // English
        [0x0409] = FFTOLanguageType.English, // en-US
        [0x0809] = FFTOLanguageType.English, // en-GB
        [0x1009] = FFTOLanguageType.English, // en-CA
        [0x1409] = FFTOLanguageType.English, // en-NZ
        [0x1809] = FFTOLanguageType.English, // en-IE
        [0x2009] = FFTOLanguageType.English, // en-JM
        [0x2809] = FFTOLanguageType.English, // en-BZ
        [0x2C09] = FFTOLanguageType.English, // en-TT
        [0x4009] = FFTOLanguageType.English, // en-IN
        [0x4409] = FFTOLanguageType.English, // en-MY
        [0x4809] = FFTOLanguageType.English, // en-SG

        // French
        [0x040C] = FFTOLanguageType.French, // fr-FR
        [0x080C] = FFTOLanguageType.French, // fr-BE
        [0x0C0C] = FFTOLanguageType.French, // fr-CA
        [0x100C] = FFTOLanguageType.French, // fr-CH
        [0x140C] = FFTOLanguageType.French, // fr-LU

        // German
        [0x0407] = FFTOLanguageType.German, // de-DE
        [0x0807] = FFTOLanguageType.German, // de-CH
        [0x0C07] = FFTOLanguageType.German, // de-AT
        [0x1007] = FFTOLanguageType.German, // de-LU
        [0x1407] = FFTOLanguageType.German, // de-LI

        // Spanish
        [0x040A] = FFTOLanguageType.Spanish, // es-ES_tradnl
        [0x0C0A] = FFTOLanguageType.Spanish, // es-ES

        // Latin American Spanish
        [0x080A] = FFTOLanguageType.LatinSpanish, // es-MX
        [0x100A] = FFTOLanguageType.LatinSpanish, // es-GT
        [0x140A] = FFTOLanguageType.LatinSpanish, // es-CR
        [0x180A] = FFTOLanguageType.LatinSpanish, // es-PA
        [0x1C0A] = FFTOLanguageType.LatinSpanish, // es-DO
        [0x200A] = FFTOLanguageType.LatinSpanish, // es-VE
        [0x240A] = FFTOLanguageType.LatinSpanish, // es-CO
        [0x280A] = FFTOLanguageType.LatinSpanish, // es-PE
        [0x2C0A] = FFTOLanguageType.LatinSpanish, // es-AR
        [0x300A] = FFTOLanguageType.LatinSpanish, // es-EC
        [0x340A] = FFTOLanguageType.LatinSpanish, // es-CL
        [0x380A] = FFTOLanguageType.LatinSpanish, // es-UY
        [0x3C0A] = FFTOLanguageType.LatinSpanish, // es-PY
        [0x4C0A] = FFTOLanguageType.LatinSpanish, // es-NI
        [0x500A] = FFTOLanguageType.LatinSpanish, // es-PR

        // Simplified Chinese
        [0x0804] = FFTOLanguageType.ChineseSimplified, // zh-CN
        [0x1004] = FFTOLanguageType.ChineseSimplified, // zh-SG

        // Traditional Chinese
        [0x0404] = FFTOLanguageType.ChineseTraditional, // zh-TW
        [0x0C04] = FFTOLanguageType.ChineseTraditional, // zh-HK

        // Korean
        [0x0412] = FFTOLanguageType.Korean, // ko-KR

        // Italian
        [0x0410] = FFTOLanguageType.Italian, // it-IT
        [0x0810] = FFTOLanguageType.Italian, // it-CH

        // Arabic
        [0x0401] = FFTOLanguageType.Arabic, // ar-SA
        [0x0801] = FFTOLanguageType.Arabic, // ar-IQ
        [0x0C01] = FFTOLanguageType.Arabic, // ar-EG
        [0x1001] = FFTOLanguageType.Arabic, // ar-LY
        [0x1401] = FFTOLanguageType.Arabic, // ar-DZ
        [0x1801] = FFTOLanguageType.Arabic, // ar-MA
        [0x1C01] = FFTOLanguageType.Arabic, // ar-TN
        [0x2001] = FFTOLanguageType.Arabic, // ar-OM
        [0x2401] = FFTOLanguageType.Arabic, // ar-YE
        [0x2801] = FFTOLanguageType.Arabic, // ar-SY
        [0x2C01] = FFTOLanguageType.Arabic, // ar-JO
        [0x3001] = FFTOLanguageType.Arabic, // ar-LB
        [0x3401] = FFTOLanguageType.Arabic, // ar-KW
        [0x3801] = FFTOLanguageType.Arabic, // ar-AE
        [0x3C01] = FFTOLanguageType.Arabic, // ar-BH
        [0x4001] = FFTOLanguageType.Arabic, // ar-QA

        // Polish
        [0x0415] = FFTOLanguageType.Polish, // pl-PL

        // Portuguese
        [0x0416] = FFTOLanguageType.Portuguese, // pt-BR
        [0x0816] = FFTOLanguageType.Portuguese, // pt-PT

        // Russian
        [0x0419] = FFTOLanguageType.Russian, // ru-RU
        [0x1819] = FFTOLanguageType.Russian, // ru-MD
    };

    /// <summary>
    /// Gets a pack suffix for the specified locale. <br/>
    /// Example: '.en' or '.ja'.
    /// </summary>
    /// <param name="locale"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Invalid locale.</exception>
    public static string GetPackSuffix(FFTOLanguageType locale)
    {
        return locale switch
        {
            FFTOLanguageType.Japanese => ".ja",
            FFTOLanguageType.English => ".en",
            FFTOLanguageType.French => ".fr",
            FFTOLanguageType.German => ".de",
            FFTOLanguageType.Spanish => ".es",
            FFTOLanguageType.LatinSpanish => ".ls",
            FFTOLanguageType.ChineseSimplified => ".cs",
            FFTOLanguageType.ChineseTraditional => ".ct",
            FFTOLanguageType.Korean => ".ko",
            FFTOLanguageType.Italian => ".it",
            FFTOLanguageType.Arabic => ".ar",
            FFTOLanguageType.Polish => ".pl",
            FFTOLanguageType.Portuguese => ".pb",
            FFTOLanguageType.Russian => ".ru",
            _ => throw new ArgumentException("GetPackSuffix: Invalid locale.", nameof(locale))
        };
    }

    /// <summary>
    /// Gets the default game locale for the current system. Does not take save settings into account.<br/>
    /// Returns <see cref="FFTOLanguageType.English"/> if invalid/not found.
    /// </summary>
    /// <returns></returns>
    public static FFTOLanguageType GetDefaultLanguage()
    {
        uint lcid = PInvoke.GetUserDefaultLCID();
        if (LCIDToGameLanguage.TryGetValue(lcid, out FFTOLanguageType fftoLocaleType))
            return fftoLocaleType;

        return FFTOLanguageType.English;
    }
}
