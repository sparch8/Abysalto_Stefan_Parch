namespace Abysalto.StefanParch.Api.Options;

public sealed class PostgreSqlOptions
{
    public const string SectionName = "PostgreSql";

    public int CommandTimeoutSeconds { get; init; } = 30;

    public bool EnableSensitiveDataLogging { get; init; }
}
