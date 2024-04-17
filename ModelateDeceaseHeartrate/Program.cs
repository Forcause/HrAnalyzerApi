using Newtonsoft.Json;

namespace ModelateDeceaseHeartrate
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var user = new User(Gender.Male, "Test", 81, 76.5, 176);

            var heartDeceasePredictions = new List<string>();

            var timeDomainFailures = new List<string>();

            var frequencyDomainFailures = new List<string>();
             
            string json = File.ReadAllText("F:\\Learn\\Projects\\DnnPpg\\output_828r6nd1.json");

            var fileData = JsonConvert.DeserializeObject<HrvData>(json);

            if (fileData is null)
            {
                return;
            }

            for (var i = 0; i < fileData.PPG_Rate.Count; i++)
            {
                if (fileData.PPG_Rate[i] > user.MaxNormPulse)
                {
                    heartDeceasePredictions.Add($"Есть подозрение на тахикардию:\r\n" +
                        $"Верхняя граница в норме: {user.MaxNormPulse}\r\n" +
                        $"Показатель пульса был зафиксирован: {fileData.PPG_Rate[i]}");
                }

                else if (fileData.PPG_Rate[i] < user.MinNormPulse)
                {
                    if (fileData.PPG_Rate[i] < user.MinNormPulse)
                    {
                        heartDeceasePredictions.Add($"Есть подозрение на брадикардию:\r\n" + $"Нижняя граница в норме: {user.MinNormPulse}\r\n"
                            + $"Показатель пульса был зафиксирован: {fileData.PPG_Rate[i]}");
                    }
                    continue;
                }
            }

            if (fileData.HRV_LFn.Any())
            {
                var lf = fileData.HRV_LFn.Last();

                if (lf < user.MinNormLf)
                {
                    frequencyDomainFailures.Add($"Зафиксировано понижение низких частот при частотном анализе ВСР. " +
                        $"\nЭто может указывать на повышенную симпатическую активность и потенциальный стресс." +
                        "\n Снижение активности НЧ было связано с повышенным риском сердечно-сосудистых заболеваний, включая гипертонию, инфаркт миокарда и инсульт.");
                }

                if (lf > user.MaxNormLf)
                {

                }
            }

            if (fileData.HRV_HFn.Any())
            {
                var hf = fileData.HRV_HFn.Last();

                if (hf < user.MinNormHf)
                {

                }

                if (hf > user.MaxNormHf)
                {
                    frequencyDomainFailures.Add($"Зафиксировано звышение высоких частот при частотном анализе ВСР. " +
                        "\nЭто может указывать на повышенную симпатическую активность и потенциальный стресс." +
                        "\nПовышенная активность ВЧ была связана с повышенным риском сердечно-сосудистых заболеваний, включая гипертонию, инфаркт миокарда и инсульт.");

                }
            }

            if (fileData.HRV_pNN50.Any() && fileData.HRV_SDNN.Any())
            {
                var sdnn = fileData.HRV_SDNN.Last();
                var pnn50 = fileData.HRV_pNN50.Last();

                if (sdnn < NormalRangeDictionary.SDNN_NORM.Start.Value && pnn50 < NormalRangeDictionary.PNN50_NORM.Start.Value)
                {
                    timeDomainFailures.Add($"Возможно наличие экстрасистолии, необходимо обратиться к специалистам");
                }
            }

            if (fileData.HRV_RMSSD.Any())
            {
                var rmssd = fileData.HRV_RMSSD.Last();

                if (rmssd < NormalRangeDictionary.RMSSD_NORM.Start.Value)
                {
                    timeDomainFailures.Add($"При наличии ишемической болезни сердца необходимо обратиться к специалисту," +
                        $" присутствуют подозрения на возможные осложнения");
                }
            }

            if (timeDomainFailures.Count > 0)
            {
                Console.WriteLine(string.Join("\r\n", timeDomainFailures));
            }

            if (frequencyDomainFailures.Count > 0)
            {
                Console.WriteLine(string.Join("\r\n", frequencyDomainFailures));
            }

            var sd = fileData.PPG_Rate.Count * 0.12;
            if (heartDeceasePredictions.Count > 0)
            {
                Console.WriteLine(string.Join("\r\n", heartDeceasePredictions));
            }
            Console.WriteLine($"Total errors count: {heartDeceasePredictions.Count}");
            if (heartDeceasePredictions.Count < sd)
            {
                Console.WriteLine($"Количество ошибок не превышает погрешность 12%");
            }
            else { Console.WriteLine($"Количество ошибок превышает погрешность 12%, рекомендовано обращение к специалисту"); }
        }

        public class HrvData
        {
            public List<double> PPG_Rate { get; set; }
            public List<double> HRV_SDNN { get; set; }
            public List<double> HRV_RMSSD { get; set; }
            public List<double> HRV_SDSD { get; set; }
            public List<double> HRV_CVNN { get; set; }
            public List<double> HRV_pNN50 { get; set; }
            public List<double> HRV_LF { get; set; }
            public List<double> HRV_HF { get; set; }
            public List<double> HRV_LFn { get; set; }
            public List<double> HRV_HFn { get; set; }
        }

        public static class NormalRangeDictionary
        {
            // 69 +- 28
            public static Range SDNN_NORM { get; } = new Range(41, 97);

            // 50 +- 21
            public static Range RMSSD_NORM { get; } = new Range(29, 71);

            // 50 +- 21
            public static Range SDSD_NORM { get; } = new Range(29, 71);

            // 25 +- 15
            public static Range PNN50_NORM { get; } = new Range(10, 40);
        }

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

            public User(Gender gender, string name, byte age, double weight, double height)
            {
                Gender = gender;
                Name = name;
                Age = age;
                Weight = weight;
                Height = height;

                (var max, var min) = CalculateNormalPulseRange(this.Age, this.Gender, this.Weight, this.Height);
                MaxNormPulse = max;
                MinNormPulse = min;

                (MinNormHf, MaxNormHf, MinNormLf, MaxNormLf) = GetNormalHfLfForUser(this.Age, this.Gender);
            }

            static (double MinNormHf, double MaxNormHf, double MinNormLf, double MaxNormLf) GetNormalHfLfForUser(byte age, Gender gender)
            {
                double MinNormHf = 0, MaxNormHf = 0, MaxNormLf = 0, MinNormLf = 0;

                switch (age)
                {
                    case >= 18 and <= 29:
                        MinNormHf = 0.15;
                        MaxNormHf = 0.50;
                        MaxNormLf = 0.40;
                        MinNormLf = 0.10;
                        break;
                    case >= 30 and <= 39:
                        MinNormHf = 0.10;
                        MaxNormHf = 0.40;
                        MaxNormLf = 0.35;
                        MinNormLf = 0.08;
                        break;
                    case >= 40 and <= 49:
                        MinNormHf = 0.08;
                        MaxNormHf = 0.35;
                        MaxNormLf = 0.30;
                        MinNormLf = 0.06;
                        break;
                    case >= 50 and <= 59:
                        MinNormHf = 0.06;
                        MaxNormHf = 0.30;
                        MaxNormLf = 0.25;
                        MinNormLf = 0.05;
                        break;
                    case >= 60 and <= 69:
                        MinNormHf = 0.05;
                        MaxNormHf = 0.25;
                        MaxNormLf = 0.20;
                        MinNormLf = 0.04;
                        break;
                    case >= 70 and <= 79:
                        MinNormHf = 0.04;
                        MaxNormHf = 0.20;
                        MaxNormLf = 0.15;
                        MinNormLf = 0.03;
                        break;
                    case >= 80 and <= 90:
                        MinNormHf = 0.03;
                        MaxNormHf = 0.15;
                        MaxNormLf = 0.10;
                        MinNormLf = 0.02;
                        break;
                    default:
                        Console.WriteLine("Invalid age range");
                        break;
                }

                return (MinNormHf, MaxNormHf, MinNormLf, MaxNormLf);
            }

            static (double, double) CalculateNormalPulseRange(int age, Gender gender, double weight, double height)
            {
                double maxPulse = 0, minPulse = 0;
                if (gender == Gender.Male)
                {
                    maxPulse = 208.609 - (0.716 * age);
                }
                if (gender == Gender.Female)
                {
                    maxPulse = 209 - (0.804 * age);
                }

                var bmi = weight / Math.Pow((height / 60), 2);

                if (bmi > 30)
                {
                    minPulse = 50;
                }
                else minPulse = 40;
                return (maxPulse, minPulse);
            }
        }

        public enum Gender
        {
            Male,
            Female
        }
    }
}
