namespace HrAnalyzer.Data.Models;

public class TechnicalInformation
{
    public string Errors { get; set; }

    public bool IsErrorsValid { get; set; }

    public List<string> AboutErrors { get; set; }
}