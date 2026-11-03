namespace SharedKernel.Persistence;

public sealed class JsonRepositoryOptions
{
    public string FileName { get; set; } = "data.json";
    public string DataPath { get; set; } = "data";
    public bool SeedInDevelopment { get; set; } = true;
}
