 using HrAnalyzer.Data.Constants;
using HrAnalyzer.Data.Models;

namespace HrAnalyzer.Data.Services;

public class AnalyzerService : IAnalyzerService
{
    public Task<AnalyzeResult> AnalyzeData(User user, PpgFileData fileData)
    {
        var results = new AnalyzeResult();

        var heartDeceasePredictions = new List<string>();
        var timeDomainFailures = new List<string>();
        var frequencyDomainFailures = new List<string>();

        if (fileData is null)
        {
            return Task.FromResult(results);
        }

        foreach (var t in fileData.PPG_Rate)
        {
            if (t > user.MaxNormPulse)
            {
                heartDeceasePredictions.Add($"Есть подозрение на тахикардию:\r\n" +
                                            $"Верхняя граница в норме: {user.MaxNormPulse}\r\n" +
                                            $"Показатель пульса был зафиксирован: {t}");
            }

            else if (t < user.MinNormPulse)
            {
                if (t < user.MinNormPulse)
                {
                    heartDeceasePredictions.Add($"Есть подозрение на брадикардию:\r\n" + $"Нижняя граница в норме: {user.MinNormPulse}\r\n"
                        + $"Показатель пульса был зафиксирован: {t}");
                }
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

            if (sdnn < NormalRangeDictionary.SdnnNorm.Start.Value && pnn50 < NormalRangeDictionary.Pnn50Norm.Start.Value)
            {
                timeDomainFailures.Add($"Возможно наличие экстрасистолии, необходимо обратиться к специалистам");
            }
        }

        if (fileData.HRV_RMSSD.Any())
        {
            var rmssd = fileData.HRV_RMSSD.Last();

            if (rmssd < NormalRangeDictionary.RmssdNorm.Start.Value)
            {
                timeDomainFailures.Add($"При наличии ишемической болезни сердца необходимо обратиться к специалисту," +
                                       $" присутствуют подозрения на возможные осложнения");
            }
        }

        if (timeDomainFailures.Count > 0)
        {
            var timeDomainFailuresAggregated = string.Join("\r\n", timeDomainFailures);

            Console.WriteLine(timeDomainFailuresAggregated);
            results.TimeDomainFailures = new TechnicalInformation
            {
                Errors = timeDomainFailuresAggregated,
                IsErrorsValid = true,
                AboutErrors = new List<string>()
            };
        }

        if (frequencyDomainFailures.Count > 0)
        {
            var frequencyDomainFailuresAggregated = string.Join("\r\n", frequencyDomainFailures);

            Console.WriteLine(frequencyDomainFailuresAggregated);
            results.FrequencyDomainFailures = new TechnicalInformation
            {
                Errors = frequencyDomainFailuresAggregated,
                IsErrorsValid = true,
                AboutErrors = new List<string>()
            };
        }

        var sd = fileData.PPG_Rate.Count * 0.12;
        if (heartDeceasePredictions.Count > 0)
        {
            var heartDeceasePredictionsAggregated = string.Join("\r\n", heartDeceasePredictions);

            Console.WriteLine(heartDeceasePredictionsAggregated);
            results.HeartDeceasePredictions = new TechnicalInformation
            {
                Errors = heartDeceasePredictionsAggregated,
                IsErrorsValid = false,
                AboutErrors = new List<string>()
            };
        }
        Console.WriteLine($"Total errors count: {heartDeceasePredictions.Count}");

        if (heartDeceasePredictions.Count < sd)
        {
            var message = "Количество ошибок не превышает погрешность 12%";
            Console.WriteLine(message);

            results.HeartDeceasePredictions.IsErrorsValid = false;
            results.HeartDeceasePredictions.AboutErrors.Add(message);
        }
        else
        {
            var message = "Количество ошибок превышает погрешность 12%, рекомендовано обращение к специалисту";
            Console.WriteLine(message);

            results.HeartDeceasePredictions.IsErrorsValid = true;
            results.HeartDeceasePredictions.AboutErrors.Add(message);
        }

        return Task.FromResult(results);
    }
}