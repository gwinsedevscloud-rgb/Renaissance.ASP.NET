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
}
