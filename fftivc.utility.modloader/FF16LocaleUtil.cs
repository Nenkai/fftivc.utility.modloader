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
public class FF16LocaleUtil
{
    public static readonly Dictionary<uint, FF16LocaleType> LCIDToGameLocale = new()
    {
        // https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/63d3d639-7fd2-4afb-abbe-0d5b5551eef8
        // 40 53 48 83 EC ? 48 8B D9 FF 15 ? ? ? ? BA

        // Japanese
        [0x0411] = FF16LocaleType.Japanese, // jp-JP

        // English
        [0x0409] = FF16LocaleType.English, // en-US
        [0x0809] = FF16LocaleType.English, // en-GB
        [0x1009] = FF16LocaleType.English, // en-CA
        [0x1409] = FF16LocaleType.English, // en-NZ
        [0x1809] = FF16LocaleType.English, // en-IE
        [0x2009] = FF16LocaleType.English, // en-JM
        [0x2809] = FF16LocaleType.English, // en-BZ
        [0x2C09] = FF16LocaleType.English, // en-TT
        [0x4009] = FF16LocaleType.English, // en-IN
        [0x4409] = FF16LocaleType.English, // en-MY
        [0x4809] = FF16LocaleType.English, // en-SG

        // French
        [0x040C] = FF16LocaleType.French, // fr-FR
        [0x080C] = FF16LocaleType.French, // fr-BE
        [0x0C0C] = FF16LocaleType.French, // fr-CA
        [0x100C] = FF16LocaleType.French, // fr-CH
        [0x140C] = FF16LocaleType.French, // fr-LU

        // German
        [0x0407] = FF16LocaleType.German, // de-DE
        [0x0807] = FF16LocaleType.German, // de-CH
        [0x0C07] = FF16LocaleType.German, // de-AT
        [0x1007] = FF16LocaleType.German, // de-LU
        [0x1407] = FF16LocaleType.German, // de-LI

        // Spanish
        [0x040A] = FF16LocaleType.Spanish, // es-ES_tradnl
        [0x0C0A] = FF16LocaleType.Spanish, // es-ES

        // Latin American Spanish
        [0x080A] = FF16LocaleType.LatinSpanish, // es-MX
        [0x100A] = FF16LocaleType.LatinSpanish, // es-GT
        [0x140A] = FF16LocaleType.LatinSpanish, // es-CR
        [0x180A] = FF16LocaleType.LatinSpanish, // es-PA
        [0x1C0A] = FF16LocaleType.LatinSpanish, // es-DO
        [0x200A] = FF16LocaleType.LatinSpanish, // es-VE
        [0x240A] = FF16LocaleType.LatinSpanish, // es-CO
        [0x280A] = FF16LocaleType.LatinSpanish, // es-PE
        [0x2C0A] = FF16LocaleType.LatinSpanish, // es-AR
        [0x300A] = FF16LocaleType.LatinSpanish, // es-EC
        [0x340A] = FF16LocaleType.LatinSpanish, // es-CL
        [0x380A] = FF16LocaleType.LatinSpanish, // es-UY
        [0x3C0A] = FF16LocaleType.LatinSpanish, // es-PY
        [0x4C0A] = FF16LocaleType.LatinSpanish, // es-NI
        [0x500A] = FF16LocaleType.LatinSpanish, // es-PR

        // Simplified Chinese
        [0x0804] = FF16LocaleType.ChineseSimplified, // zh-CN
        [0x1004] = FF16LocaleType.ChineseSimplified, // zh-SG

        // Traditional Chinese
        [0x0404] = FF16LocaleType.ChineseTraditional, // zh-TW
        [0x0C04] = FF16LocaleType.ChineseTraditional, // zh-HK

        // Korean
        [0x0412] = FF16LocaleType.Korean, // ko-KR

        // Italian
        [0x0410] = FF16LocaleType.Italian, // it-IT
        [0x0810] = FF16LocaleType.Italian, // it-CH

        // Arabic
        [0x0401] = FF16LocaleType.Arabic, // ar-SA
        [0x0801] = FF16LocaleType.Arabic, // ar-IQ
        [0x0C01] = FF16LocaleType.Arabic, // ar-EG
        [0x1001] = FF16LocaleType.Arabic, // ar-LY
        [0x1401] = FF16LocaleType.Arabic, // ar-DZ
        [0x1801] = FF16LocaleType.Arabic, // ar-MA
        [0x1C01] = FF16LocaleType.Arabic, // ar-TN
        [0x2001] = FF16LocaleType.Arabic, // ar-OM
        [0x2401] = FF16LocaleType.Arabic, // ar-YE
        [0x2801] = FF16LocaleType.Arabic, // ar-SY
        [0x2C01] = FF16LocaleType.Arabic, // ar-JO
        [0x3001] = FF16LocaleType.Arabic, // ar-LB
        [0x3401] = FF16LocaleType.Arabic, // ar-KW
        [0x3801] = FF16LocaleType.Arabic, // ar-AE
        [0x3C01] = FF16LocaleType.Arabic, // ar-BH
        [0x4001] = FF16LocaleType.Arabic, // ar-QA

        // Polish
        [0x0415] = FF16LocaleType.Polish, // pl-PL

        // Portuguese
        [0x0416] = FF16LocaleType.Portuguese, // pt-BR
        [0x0816] = FF16LocaleType.Portuguese, // pt-PT

        // Russian
        [0x0419] = FF16LocaleType.Russian, // ru-RU
        [0x1819] = FF16LocaleType.Russian, // ru-MD
    };

    /// <summary>
    /// Gets a pack suffix for the specified locale. <br/>
    /// Example: '.en' or '.ja'.
    /// </summary>
    /// <param name="locale"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">Invalid locale.</exception>
    public static string GetPackSuffix(FF16LocaleType locale)
    {
        return locale switch
        {
            FF16LocaleType.Japanese => ".ja",
            FF16LocaleType.English => ".en",
            FF16LocaleType.French => ".fr",
            FF16LocaleType.German => ".de",
            FF16LocaleType.Spanish => ".es",
            FF16LocaleType.LatinSpanish => ".ls",
            FF16LocaleType.ChineseSimplified => ".cs",
            FF16LocaleType.ChineseTraditional => ".ct",
            FF16LocaleType.Korean => ".ko",
            FF16LocaleType.Italian => ".it",
            FF16LocaleType.Arabic => ".ar",
            FF16LocaleType.Polish => ".pl",
            FF16LocaleType.Portuguese => ".pb",
            FF16LocaleType.Russian => ".ru",
            _ => throw new ArgumentException("GetPackSuffix: Invalid locale.", nameof(locale))
        };
    }

    /// <summary>
    /// Gets the default game locale for the current system. Does not take save settings into account.<br/>
    /// Returns <see cref="FF16LocaleType.English"/> if invalid/not found.
    /// </summary>
    /// <returns></returns>
    public static FF16LocaleType GetDefaultLocale()
    {
        uint lcid = PInvoke.GetUserDefaultLCID();
        if (LCIDToGameLocale.TryGetValue(lcid, out FF16LocaleType ff16LocaleType))
            return ff16LocaleType;

        return FF16LocaleType.English;
    }
}
