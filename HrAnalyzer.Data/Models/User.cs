using HrAnalyzer.Data.Constants;

namespace HrAnalyzer.Data.Models;

public class User
{
    public byte Age { get; set; }

    public string Name { get; set; }

    public double Weight { get; set; }

    public double Height { get; set; }

    public Gender Gender { get; }

    public double MaxNormPulse { get; }

    public double MinNormPulse { get; }

    public double MinNormHf { get; }

    public double MaxNormHf { get; }

    public double MinNormLf { get; }

    public double MaxNormLf { get; }

    public User()
    {
    }

    public User(Gender gender, string name, byte age, double weight, double height)
    {
        Gender = gender;
        Name = name;
        Age = age;
        Weight = weight;
        Height = height;

        var (max, min) = CalculateNormalPulseRange(Age, Gender, Weight, Height);
        MaxNormPulse = max;
        MinNormPulse = min;

        (MinNormHf, MaxNormHf, MinNormLf, MaxNormLf) = GetNormalHfLfForUser(Age);
    }

    private static (double MinNormHf, double MaxNormHf, double MinNormLf, double MaxNormLf) GetNormalHfLfForUser(byte age)
    {
        double minNormHf = 0, maxNormHf = 0, maxNormLf = 0, minNormLf = 0;

        switch (age)
        {
            case >= 18 and <= 29:
                minNormHf = 0.15;
                maxNormHf = 0.50;
                maxNormLf = 0.40;
                minNormLf = 0.10;
                break;
            case >= 30 and <= 39:
                minNormHf = 0.10;
                maxNormHf = 0.40;
                maxNormLf = 0.35;
                minNormLf = 0.08;
                break;
            case >= 40 and <= 49:
                minNormHf = 0.08;
                maxNormHf = 0.35;
                maxNormLf = 0.30;
                minNormLf = 0.06;
                break;
            case >= 50 and <= 59:
                minNormHf = 0.06;
                maxNormHf = 0.30;
                maxNormLf = 0.25;
                minNormLf = 0.05;
                break;
            case >= 60 and <= 69:
                minNormHf = 0.05;
                maxNormHf = 0.25;
                maxNormLf = 0.20;
                minNormLf = 0.04;
                break;
            case >= 70 and <= 79:
                minNormHf = 0.04;
                maxNormHf = 0.20;
                maxNormLf = 0.15;
                minNormLf = 0.03;
                break;
            case >= 80 and <= 90:
                minNormHf = 0.03;
                maxNormHf = 0.15;
                maxNormLf = 0.10;
                minNormLf = 0.02;
                break;
            default:
                Console.WriteLine("Invalid age range");
                break;
        }

        return (minNormHf, maxNormHf, minNormLf, maxNormLf);
    }

    private static (double, double) CalculateNormalPulseRange(int age, Gender gender, double weight, double height)
    {
        double maxPulse = 0;
        maxPulse = gender switch
        {
            Gender.Male => 208.609 - 0.716 * age,
            Gender.Female => 209 - 0.804 * age,
            _ => maxPulse
        };

        var bmi = weight / Math.Pow((height / 60), 2);

        double minPulse = bmi > 30 ? 50 : 40;
        return (maxPulse, minPulse);
    }
}