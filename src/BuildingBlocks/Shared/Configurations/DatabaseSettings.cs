namespace Shared.Configurations;

public class DatabaseSettings
{
    public required string DBProvider { get; set; }
    public required string ConnectionString { get; set; }
    public required string DatabaseName { get; set; }
}