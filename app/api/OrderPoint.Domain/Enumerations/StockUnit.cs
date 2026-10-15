namespace OrderPoint.Domain.Enumerations;

// The base unit a stock item is counted in; packs (a keg, a bottle, a case) are a number of these
public enum StockUnit
{
    Milliliter,
    Gram,
    Piece
}