namespace MauiBiller.ViewModels;

public sealed class FeatureMetricViewModel(string label, string value, string detail)
{
    public string Label
    {
        get;
    } = label;

    public string Value
    {
        get;
    } = value;

    public string Detail
    {
        get;
    } = detail;
}
