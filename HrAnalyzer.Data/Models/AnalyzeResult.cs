namespace HrAnalyzer.Data.Models;

public class AnalyzeResult
{
    public TechnicalInformation TimeDomainFailures { get; set; }

    public TechnicalInformation FrequencyDomainFailures { get; set; }

    public TechnicalInformation HeartDeceasePredictions { get; set; }

    public TechnicalInformation TechnicalInformation { get; set; }
}