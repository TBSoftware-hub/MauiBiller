namespace MauiBiller.Configuration;

public static class AppEnvironment
{
    public const string Development = "Development";
    public const string Production = "Production";

    public static string Current
    {
        get
        {
#if DEBUG
            return Development;
#else
            return Production;
#endif
        }
    }
}
