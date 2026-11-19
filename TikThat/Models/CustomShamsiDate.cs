namespace TikThat.Models;

public class CustomShamsiDate
{
    public int Year { get; }
    public int Month { get; }
    public int Day { get; }
    public string MonthName { get; }

    private static readonly string[] ShamsiMonthNames =
    [
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    ];

    public CustomShamsiDate(DateTime gregorianDate)
    {
        var calendar = new System.Globalization.PersianCalendar();
        Year = calendar.GetYear(gregorianDate);
        Month = calendar.GetMonth(gregorianDate);
        Day = calendar.GetDayOfMonth(gregorianDate);
        MonthName = ShamsiMonthNames[Month - 1];
    }

    public override string ToString()
    {
        return $"{Day} {MonthName}";
    }
}
