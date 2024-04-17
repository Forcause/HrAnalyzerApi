namespace HrAnalyzer.Data.Models;

public class PpgFileData
{
    public IList<double> PPG_Rate { get; set; }
    public IList<double> HRV_SDNN { get; set; }
    public IList<double> HRV_RMSSD { get; set; }
    public IList<double> HRV_SDSD { get; set; }
    public IList<double> HRV_CVNN { get; set; }
    public IList<double> HRV_pNN50 { get; set; }
    public IList<double> HRV_LF { get; set; }
    public IList<double> HRV_HF { get; set; }
    public IList<double> HRV_LFn { get; set; }
    public IList<double> HRV_HFn { get; set; }
}