namespace MauiBiller.Configuration;

public sealed class AppConfiguration
{
    public string EnvironmentName
    {
        get;
        init;
    } = AppEnvironment.Production;

    public FirebaseConfiguration Firebase
    {
        get;
        init;
    } = new();

    public DiagnosticsConfiguration Diagnostics
    {
        get;
        init;
    } = new();
}

public sealed class FirebaseConfiguration
{
    public string ProjectId
    {
        get;
        init;
    } = string.Empty;

    public string DatabaseUrl
    {
        get;
        init;
    } = string.Empty;

    public string OAuthProvider
    {
        get;
        init;
    } = "OAuth";
}

public sealed class DiagnosticsConfiguration
{
    public bool EnableDebugLogging
    {
        get;
        init;
    }
}
