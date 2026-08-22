namespace Renaissance.Web.Helpers;

public static class TriageVitalsHelper
{
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
        < 25 => "Normal",
        < 30 => "Overweight",
        _ => "Obese"
    };

    public static string BmiCssClass(double? bmi) => bmi switch
    {
        null => "bmi-empty",
        < 18.5 => "bmi-underweight",
        < 25 => "bmi-normal",
        < 30 => "bmi-overweight",
        _ => "bmi-obese"
    };

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
