using System;

namespace Tgs.Report.Aggregate.Client.Models;

public record MessageResponse
{
    public bool Valid { get; set; } = false;

    public string Message { get; set; } = string.Empty;

    public string[] Errors { get; set; } = Array.Empty<string>();
}