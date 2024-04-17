using HrAnalyzer.Data.Models;

namespace HrAnalyzer.Data.Services;

public interface IAnalyzerService
{
    Task<AnalyzeResult> AnalyzeData(User user, PpgFileData fileData);
}