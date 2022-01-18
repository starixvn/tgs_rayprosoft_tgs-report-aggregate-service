namespace Tgs.Report.Aggregate.Domain
{
    public record ReportErrorMessage(string Message, string? StackTrace, string? InnerMessage);
}