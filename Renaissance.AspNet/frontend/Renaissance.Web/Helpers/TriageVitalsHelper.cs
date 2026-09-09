namespace Renaissance.Web.Helpers;

/// <summary>
/// Adult BMI helpers using WHO cut-offs (standard for Nigerian hospital triage).
/// Underweight &lt;18.5 | Normal 18.5–24.9 | Overweight 25–29.9 | Obese ≥30
/// </summary>
public static class TriageVitalsHelper
{
    public const double BmiGaugeMin = 15d;
    public const double BmiGaugeMax = 40d;

    public static double? CalculateBmi(double? weightKg, double? heightCm)
    {
        if (weightKg is null or <= 0 || heightCm is null or <= 0)
        {
            return null;
        }

        var heightM = heightCm.Value / 100d;
        return Math.Round(weightKg.Value / (heightM * heightM), 1);
    }

    public static string BmiCategory(double? bmi) => bmi switch
    {
        null => "—",
        < 18.5 => "Underweight",
        < 25 => "Normal weight",
        < 30 => "Overweight",
        _ => "Obese"
    };

    public static string BmiRangeLabel(double? bmi) => bmi switch
    {
        null => string.Empty,
        < 18.5 => "BMI &lt; 18.5",
        < 25 => "BMI 18.5 – 24.9",
        < 30 => "BMI 25.0 – 29.9",
        _ => "BMI ≥ 30.0"
    };

    public static string BmiCssClass(double? bmi) => bmi switch
    {
        null => "bmi-empty",
        < 18.5 => "bmi-underweight",
        < 25 => "bmi-normal",
        < 30 => "bmi-overweight",
        _ => "bmi-obese"
    };

    /// <summary>Maps BMI to a top-semicircle angle (180° = left / 15, 0° = right / 40).</summary>
    public static double BmiNeedleAngle(double? bmi)
    {
        var value = Math.Clamp(bmi ?? BmiGaugeMin, BmiGaugeMin, BmiGaugeMax);
        return 180d - ((value - BmiGaugeMin) / (BmiGaugeMax - BmiGaugeMin)) * 180d;
    }

    public static string BpCssClass(int? systolic, int? diastolic)
    {
        if (systolic is null || diastolic is null) return "is-empty";
        if (systolic >= 140 || diastolic >= 90) return "is-high";
        if (systolic >= 120 || diastolic >= 80) return "is-elevated";
        return "is-normal";
    }

    public static string TempCssClass(double? temp)
    {
        if (temp is null) return "is-empty";
        if (temp >= 38.0) return "is-high";
        if (temp >= 37.3) return "is-elevated";
        if (temp < 36.0) return "is-low";
        return "is-normal";
    }

    public static string PulseCssClass(int? pulse)
    {
        if (pulse is null) return "is-empty";
        if (pulse > 100) return "is-high";
        if (pulse < 60) return "is-low";
        return "is-normal";
    }
}
