namespace OrderPoint.Domain.Enumerations;

// Out at or below zero, low at or below the par level
public enum StockLevel
{
    Ok,
    Low,
    Out
}