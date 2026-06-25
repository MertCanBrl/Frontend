namespace Backend.Utils;

// Devamsızlık durum hesabı. Saf/static — DB gerektirmez, doğrudan birim test edilebilir.
//
// Kural:
//   absenceRate = absentCount / totalWeeks * 100
//   Kaldı (Failed)  : absenceRate > limitPercent
//   Risk            : kalmadıysa ve absenceRate >= limitPercent * 0.8
//   Devam (Safe)    : diğer durumlar
public static class AttendanceCalculator
{
    public const string Failed = "Failed";
    public const string Risk = "Risk";
    public const string Safe = "Safe";

    // Risk eşiği, sınırın bu oranı kadarıdır (sınırın %80'i).
    private const decimal RiskThresholdFactor = 0.8m;

    // Devamsızlık oranı (%). totalWeeks <= 0 ise tanımsızdır → 0 döner.
    public static decimal AbsenceRate(int absentCount, int totalWeeks)
    {
        if (totalWeeks <= 0) return 0m;
        var clamped = Math.Clamp(absentCount, 0, totalWeeks);
        return Math.Round((decimal)clamped / totalWeeks * 100m, 1);
    }

    // Öğrencinin devam durumu: Failed | Risk | Safe.
    public static string Status(int absentCount, int totalWeeks, decimal limitPercent)
    {
        if (totalWeeks <= 0) return Safe;

        var rate = AbsenceRate(absentCount, totalWeeks);

        if (rate > limitPercent) return Failed;
        if (rate >= limitPercent * RiskThresholdFactor) return Risk;
        return Safe;
    }
}
