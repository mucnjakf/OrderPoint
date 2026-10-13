using Microsoft.AspNetCore.Components;

namespace OrderPoint.Shared.Components;

// A new key per change re-creates the spans, so the CSS animation runs again
public sealed partial class RollingNumber<T>
{
    private const string UpDirection = "up";

    private const string DownDirection = "down";

    private bool _isInitialized;

    private T _previousValue = default!;

    private string _previousText = string.Empty;

    [Parameter]
    [EditorRequired]
    public T Value { get; set; } = default!;

    [Parameter]
    [EditorRequired]
    public string Text { get; set; } = null!;

    private int Version { get; set; }

    private string? PreviousText { get; set; }

    private string? Direction { get; set; }

    private string PreviousClass => $"rolling-number-previous rolling-number-out-{Direction}";

    private string? CurrentClass => Direction is null ? null : $"rolling-number-in-{Direction}";

    protected override void OnParametersSet()
    {
        int comparison = Comparer<T>.Default.Compare(Value, _previousValue);

        if (_isInitialized && comparison != 0)
        {
            PreviousText = _previousText;
            Direction = comparison > 0 ? UpDirection : DownDirection;
            Version++;
        }

        _isInitialized = true;
        _previousValue = Value;
        _previousText = Text;
    }
}