namespace ABCDMall.Modules.Users.Infrastructure.Persistence.Configurations;

internal static class ModelConfigurationDefaults
{
    internal const string SqlServerStringIdDefault = "LOWER(REPLACE(CONVERT(varchar(36), NEWID()), '-', ''))";
}
