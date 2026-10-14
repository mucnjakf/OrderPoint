using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Shared.Extensions;

internal static class FixtureTypeExtensions
{
    internal static string GetLabel(this FixtureType type)
    {
        return type switch
        {
            FixtureType.BarCounter => "Bar counter",
            FixtureType.Entrance => "Entrance",
            FixtureType.Wall => "Wall",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}