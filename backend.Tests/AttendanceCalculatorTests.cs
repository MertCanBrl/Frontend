using Backend.Utils;

namespace Backend.Tests;

// Devamsızlık hesaplama ve sınır aşma mantığının birim testleri.
// AttendanceCalculator saf/static olduğundan DB gerekmez.
public class AttendanceCalculatorTests
{
    // ── AbsenceRate ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0, 14, 0.0)]      // hiç devamsızlık
    [InlineData(4, 14, 28.6)]     // 4/14 → %28.6 (yuvarlama)
    [InlineData(5, 14, 35.7)]     // 5/14 → %35.7
    [InlineData(14, 14, 100.0)]   // tüm haftalar
    [InlineData(7, 14, 50.0)]     // yarısı
    public void AbsenceRate_DogruOraniHesaplar(int absent, int total, double expected)
    {
        Assert.Equal((decimal)expected, AttendanceCalculator.AbsenceRate(absent, total));
    }

    [Fact]
    public void AbsenceRate_ToplamHaftaSifirsa_SifirDoner()
    {
        Assert.Equal(0m, AttendanceCalculator.AbsenceRate(5, 0));
    }

    [Fact]
    public void AbsenceRate_DevamsizlikToplamHaftayiAsarsa_YuzdeYuzeKenetlenir()
    {
        Assert.Equal(100m, AttendanceCalculator.AbsenceRate(20, 14));
    }

    // ── Status: 14 hafta / %30 sınır (varsayılan senaryo) ─────────────────────
    // Risk eşiği = sınır * 0.8 = %24

    [Theory]
    [InlineData(0)]   // %0
    [InlineData(3)]   // %21.4  (< %24)
    public void Status_SinirAltindaVeRiskAltinda_Safe(int absent)
    {
        Assert.Equal(AttendanceCalculator.Safe, AttendanceCalculator.Status(absent, 14, 30m));
    }

    [Theory]
    [InlineData(4)]   // %28.6 — sınıra yaklaşan
    public void Status_SiniraYaklasan_Risk(int absent)
    {
        Assert.Equal(AttendanceCalculator.Risk, AttendanceCalculator.Status(absent, 14, 30m));
    }

    [Theory]
    [InlineData(5)]    // %35.7 — sınırı aşan → kaldı
    [InlineData(14)]   // %100  — kesin kaldı
    public void Status_SiniriAsan_Failed(int absent)
    {
        Assert.Equal(AttendanceCalculator.Failed, AttendanceCalculator.Status(absent, 14, 30m));
    }

    // ── Status: kenar durumlar ────────────────────────────────────────────────

    [Fact]
    public void Status_SiniraTamEsit_KalmazRiskSayilir()
    {
        // 10 hafta, %30 sınır → 3 devamsız = tam %30. "Aşan" katı büyüktür olduğundan kalmaz.
        Assert.Equal(AttendanceCalculator.Risk, AttendanceCalculator.Status(3, 10, 30m));
        // 4 devamsız = %40 > %30 → kaldı
        Assert.Equal(AttendanceCalculator.Failed, AttendanceCalculator.Status(4, 10, 30m));
    }

    [Fact]
    public void Status_RiskEsigineTamEsit_Risk()
    {
        // 25 hafta, %30 sınır → risk eşiği %24. 6 devamsız = tam %24 → Risk.
        Assert.Equal(AttendanceCalculator.Risk, AttendanceCalculator.Status(6, 25, 30m));
        // 5 devamsız = %20 < %24 → Safe.
        Assert.Equal(AttendanceCalculator.Safe, AttendanceCalculator.Status(5, 25, 30m));
    }

    [Fact]
    public void Status_ToplamHaftaSifirsa_Safe()
    {
        Assert.Equal(AttendanceCalculator.Safe, AttendanceCalculator.Status(0, 0, 30m));
    }

    [Fact]
    public void Status_FarkliSinir_DogruDavranir()
    {
        // %50 sınır, 14 hafta → risk eşiği %40.
        // 5 devamsız = %35.7 < %40 → Safe
        Assert.Equal(AttendanceCalculator.Safe, AttendanceCalculator.Status(5, 14, 50m));
        // 6 devamsız = %42.9 → Risk
        Assert.Equal(AttendanceCalculator.Risk, AttendanceCalculator.Status(6, 14, 50m));
        // 8 devamsız = %57.1 > %50 → Failed
        Assert.Equal(AttendanceCalculator.Failed, AttendanceCalculator.Status(8, 14, 50m));
    }
}
