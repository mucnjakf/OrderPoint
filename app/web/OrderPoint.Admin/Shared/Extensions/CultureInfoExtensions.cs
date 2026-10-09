using System.Globalization;

namespace OrderPoint.Admin.Shared.Extensions;

internal static class CultureInfoExtensions
{
    private const string EnglishCultureName = "en-US";

    private const string EuroSymbol = "€";

    private const int EuroDecimalDigits = 2;

    private static readonly DateTimeFormatInfo EnglishDateTimeFormat =
        CultureInfo.GetCultureInfo(EnglishCultureName).DateTimeFormat;

    internal static CultureInfo ToDisplayCulture(this CultureInfo culture)
    {
        var displayCulture = (CultureInfo)culture.Clone();

        DateTimeFormatInfo dateTimeFormat = displayCulture.DateTimeFormat;
        dateTimeFormat.DayNames = EnglishDateTimeFormat.DayNames;
        dateTimeFormat.AbbreviatedDayNames = EnglishDateTimeFormat.AbbreviatedDayNames;
        dateTimeFormat.ShortestDayNames = EnglishDateTimeFormat.ShortestDayNames;
        dateTimeFormat.MonthNames = EnglishDateTimeFormat.MonthNames;
        dateTimeFormat.AbbreviatedMonthNames = EnglishDateTimeFormat.AbbreviatedMonthNames;
        dateTimeFormat.MonthGenitiveNames = EnglishDateTimeFormat.MonthNames;
        dateTimeFormat.AbbreviatedMonthGenitiveNames = EnglishDateTimeFormat.AbbreviatedMonthNames;
        dateTimeFormat.AMDesignator = EnglishDateTimeFormat.AMDesignator;
        dateTimeFormat.PMDesignator = EnglishDateTimeFormat.PMDesignator;

        displayCulture.NumberFormat.CurrencySymbol = EuroSymbol;
        displayCulture.NumberFormat.CurrencyDecimalDigits = EuroDecimalDigits;

        return displayCulture;
    }

    internal static bool Uses12HourClock(this CultureInfo culture)
    {
        return culture.DateTimeFormat.ShortTimePattern.Contains('t');
    }

    internal static string GetShortMonthDayPattern(this CultureInfo culture)
    {
        return culture.DateTimeFormat.MonthDayPattern.Replace("MMMM", "MMM");
    }
}