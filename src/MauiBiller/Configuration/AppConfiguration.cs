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
    public string ApiKey
    {
        get;
        init;
    } = string.Empty;

    public string AuthDomain
    {
        get;
        init;
    } = string.Empty;

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
    } = "Password";

    public string StorageBucket
    {
        get;
        init;
    } = string.Empty;

    public string MessagingSenderId
    {
        get;
        init;
    } = string.Empty;

    public string AppId
    {
        get;
        init;
    } = string.Empty;

    public string MeasurementId
    {
        get;
        init;
    } = string.Empty;
}

public sealed class DiagnosticsConfiguration
{
    public bool EnableDebugLogging
    {
        get;
        init;
    }
}
