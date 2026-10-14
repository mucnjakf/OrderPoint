namespace OrderPoint.Shared.Constants;

// Soft corners for every item and category picture; small thumbnails get a smaller radius, so they don't turn
// into circles
internal static class ImageStyles
{
    // Pictures from 32 px up
    internal const string Rounded = "border-radius: var(--mud-default-borderradius);";

    // Thumbnails up to 24 px
    internal const string RoundedSmall = "border-radius: 4px;";
}