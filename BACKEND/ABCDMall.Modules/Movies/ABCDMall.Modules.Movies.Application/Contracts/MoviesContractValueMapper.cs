using ABCDMall.Modules.Movies.Domain.Enums;

namespace ABCDMall.Modules.Movies.Application.Contracts;

public static class MoviesContractValueMapper
{
    public static string ToContractValue(HallType hallType)
    {
        return hallType switch
        {
            HallType.Standard2D => "2D",
            HallType.Standard3D => "3D",
            HallType.Imax => "IMAX",
            HallType.FourDx => "4DX",
            _ => hallType.ToString()
        };
    }

    public static string ToContractValue(LanguageType languageType)
    {
        return languageType switch
        {
            LanguageType.Subtitle => "Sub",
            LanguageType.Dubbed => "Dub",
            _ => languageType.ToString()
        };
    }

    public static bool TryParseHallType(string? value, out HallType hallType)
    {
        hallType = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Trim().ToUpperInvariant() switch
        {
            "2D" => SetHallType(HallType.Standard2D, out hallType),
            "3D" => SetHallType(HallType.Standard3D, out hallType),
            "IMAX" => SetHallType(HallType.Imax, out hallType),
            "4DX" => SetHallType(HallType.FourDx, out hallType),
            _ => Enum.TryParse(value.Trim(), true, out hallType)
        };
    }

    public static bool TryParseLanguageType(string? value, out LanguageType languageType)
    {
        languageType = default;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Trim().ToUpperInvariant() switch
        {
            "SUB" => SetLanguageType(LanguageType.Subtitle, out languageType),
            "DUB" => SetLanguageType(LanguageType.Dubbed, out languageType),
            _ => Enum.TryParse(value.Trim(), true, out languageType)
        };
    }

    private static bool SetHallType(HallType value, out HallType hallType)
    {
        hallType = value;
        return true;
    }

    private static bool SetLanguageType(LanguageType value, out LanguageType languageType)
    {
        languageType = value;
        return true;
    }
}
