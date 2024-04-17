namespace HrAnalyzer.Data.Constants;

public static class NormalRangeDictionary
{
    // 69 +- 28
    public static Range SdnnNorm { get; } = new(41, 97);

    // 50 +- 21
    public static Range RmssdNorm { get; } = new(29, 71);

    // 50 +- 21
    public static Range SdsdNorm { get; } = new(29, 71);

    // 25 +- 15
    public static Range Pnn50Norm { get; } = new(10, 40);
}