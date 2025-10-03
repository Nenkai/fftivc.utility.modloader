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
    public static readonly Dictionary<uint, FFTOLocaleType> LCIDToGameLocale = new()
    {
        // https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/63d3d639-7fd2-4afb-abbe-0d5b5551eef8

        // Japanese
        [0x0411] = FFTOLocaleType.Japanese, // jp-JP

        // English
        [0x0409] = FFTOLocaleType.English, // en-US
        [0x0809] = FFTOLocaleType.English, // en-GB
        [0x1009] = FFTOLocaleType.English, // en-CA
        [0x1409] = FFTOLocaleType.English, // en-NZ
        [0x1809] = FFTOLocaleType.English, // en-IE
        [0x2009] = FFTOLocaleType.English, // en-JM
        [0x2809] = FFTOLocaleType.English, // en-BZ
        [0x2C09] = FFTOLocaleType.English, // en-TT
        [0x4009] = FFTOLocaleType.English, // en-IN
        [0x4409] = FFTOLocaleType.English, // en-MY
        [0x4809] = FFTOLocaleType.English, // en-SG

        // French
        [0x040C] = FFTOLocaleType.French, // fr-FR
        [0x080C] = FFTOLocaleType.French, // fr-BE
        [0x0C0C] = FFTOLocaleType.French, // fr-CA
        [0x100C] = FFTOLocaleType.French, // fr-CH
        [0x140C] = FFTOLocaleType.French, // fr-LU

        // German
        [0x0407] = FFTOLocaleType.German, // de-DE
        [0x0807] = FFTOLocaleType.German, // de-CH
        [0x0C07] = FFTOLocaleType.German, // de-AT
        [0x1007] = FFTOLocaleType.German, // de-LU
        [0x1407] = FFTOLocaleType.German, // de-LI

        // Spanish
        [0x040A] = FFTOLocaleType.Spanish, // es-ES_tradnl
        [0x0C0A] = FFTOLocaleType.Spanish, // es-ES

        // Latin American Spanish
        [0x080A] = FFTOLocaleType.LatinSpanish, // es-MX
        [0x100A] = FFTOLocaleType.LatinSpanish, // es-GT
        [0x140A] = FFTOLocaleType.LatinSpanish, // es-CR
        [0x180A] = FFTOLocaleType.LatinSpanish, // es-PA
        [0x1C0A] = FFTOLocaleType.LatinSpanish, // es-DO
        [0x200A] = FFTOLocaleType.LatinSpanish, // es-VE
        [0x240A] = FFTOLocaleType.LatinSpanish, // es-CO
        [0x280A] = FFTOLocaleType.LatinSpanish, // es-PE
        [0x2C0A] = FFTOLocaleType.LatinSpanish, // es-AR
        [0x300A] = FFTOLocaleType.LatinSpanish, // es-EC
        [0x340A] = FFTOLocaleType.LatinSpanish, // es-CL
        [0x380A] = FFTOLocaleType.LatinSpanish, // es-UY
        [0x3C0A] = FFTOLocaleType.LatinSpanish, // es-PY
        [0x4C0A] = FFTOLocaleType.LatinSpanish, // es-NI
        [0x500A] = FFTOLocaleType.LatinSpanish, // es-PR

        // Simplified Chinese
        [0x0804] = FFTOLocaleType.ChineseSimplified, // zh-CN
        [0x1004] = FFTOLocaleType.ChineseSimplified, // zh-SG

        // Traditional Chinese
        [0x0404] = FFTOLocaleType.ChineseTraditional, // zh-TW
        [0x0C04] = FFTOLocaleType.ChineseTraditional, // zh-HK

        // Korean
        [0x0412] = FFTOLocaleType.Korean, // ko-KR

        // Italian
        [0x0410] = FFTOLocaleType.Italian, // it-IT
        [0x0810] = FFTOLocaleType.Italian, // it-CH

        // Arabic
        [0x0401] = FFTOLocaleType.Arabic, // ar-SA
        [0x0801] = FFTOLocaleType.Arabic, // ar-IQ
        [0x0C01] = FFTOLocaleType.Arabic, // ar-EG
        [0x1001] = FFTOLocaleType.Arabic, // ar-LY
        [0x1401] = FFTOLocaleType.Arabic, // ar-DZ
        [0x1801] = FFTOLocaleType.Arabic, // ar-MA
        [0x1C01] = FFTOLocaleType.Arabic, // ar-TN
        [0x2001] = FFTOLocaleType.Arabic, // ar-OM
        [0x2401] = FFTOLocaleType.Arabic, // ar-YE
        [0x2801] = FFTOLocaleType.Arabic, // ar-SY
        [0x2C01] = FFTOLocaleType.Arabic, // ar-JO
        [0x3001] = FFTOLocaleType.Arabic, // ar-LB
        [0x3401] = FFTOLocaleType.Arabic, // ar-KW
        [0x3801] = FFTOLocaleType.Arabic, // ar-AE
        [0x3C01] = FFTOLocaleType.Arabic, // ar-BH
        [0x4001] = FFTOLocaleType.Arabic, // ar-QA

        // Polish
        [0x0415] = FFTOLocaleType.Polish, // pl-PL

        // Portuguese
        [0x0416] = FFTOLocaleType.Portuguese, // pt-BR
        [0x0816] = FFTOLocaleType.Portuguese, // pt-PT

        // Russian
        [0x0419] = FFTOLocaleType.Russian, // ru-RU
        [0x1819] = FFTOLocaleType.Russian, // ru-MD
    };

    /// <summary>
    /// Gets a pack suffix for the specified locale. <br/>
    /// Example: '.en' or '.ja'.
    /// </summary>
    /// <param name="locale"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Invalid locale.</exception>
    public static string GetPackSuffix(FFTOLocaleType locale)
    {
        return locale switch
        {
            FFTOLocaleType.Japanese => ".ja",
            FFTOLocaleType.English => ".en",
            FFTOLocaleType.French => ".fr",
            FFTOLocaleType.German => ".de",
            FFTOLocaleType.Spanish => ".es",
            FFTOLocaleType.LatinSpanish => ".ls",
            FFTOLocaleType.ChineseSimplified => ".cs",
            FFTOLocaleType.ChineseTraditional => ".ct",
            FFTOLocaleType.Korean => ".ko",
            FFTOLocaleType.Italian => ".it",
            FFTOLocaleType.Arabic => ".ar",
            FFTOLocaleType.Polish => ".pl",
            FFTOLocaleType.Portuguese => ".pb",
            FFTOLocaleType.Russian => ".ru",
            _ => throw new ArgumentException("GetPackSuffix: Invalid locale.", nameof(locale))
        };
    }

    /// <summary>
    /// Gets the default game locale for the current system. Does not take save settings into account.<br/>
    /// Returns <see cref="FFTOLocaleType.English"/> if invalid/not found.
    /// </summary>
    /// <returns></returns>
    public static FFTOLocaleType GetDefaultLocale()
    {
        uint lcid = PInvoke.GetUserDefaultLCID();
        if (LCIDToGameLocale.TryGetValue(lcid, out FFTOLocaleType fftoLocaleType))
            return fftoLocaleType;

        return FFTOLocaleType.English;
    }
}
