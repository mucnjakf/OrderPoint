namespace OrderPoint.Register.Shared.Layout;

// The side room of the app bar and the pages: a tenth of the screen on each side, at least 32px
internal static class PageGutter
{
    internal const string Width = "max(32px, 10vw)";

    internal const string PaddingStyle = $"padding-left: {Width}; padding-right: {Width};";
}