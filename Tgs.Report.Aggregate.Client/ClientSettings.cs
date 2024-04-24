namespace Tgs.Report.Aggregate.Client;

public record ClientSettings
{
    public string Url { get; init; } = default!;
}