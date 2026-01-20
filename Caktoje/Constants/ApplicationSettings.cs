namespace Caktoje.Constants;

public class ApplicationSettings
{
    public static TimeOnly OpeningHour { get; set; }

    public static TimeOnly ClosingHour { get; set; }
    public static bool WorksOnSunday { get; set; }
    public static bool WorksOnSaturday { get; set; }
}