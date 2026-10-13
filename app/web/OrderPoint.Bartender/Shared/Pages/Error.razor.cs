using System.Diagnostics;
using Microsoft.AspNetCore.Components;

namespace OrderPoint.Bartender.Shared.Pages;

public sealed partial class Error
{
    [CascadingParameter]
    private HttpContext? HttpContext { get; set; }

    private string? RequestId => Activity.Current?.Id ?? HttpContext?.TraceIdentifier;
}