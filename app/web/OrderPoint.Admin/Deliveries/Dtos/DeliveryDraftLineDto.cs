namespace OrderPoint.Admin.Deliveries.Dtos;

public sealed record DeliveryDraftLineDto(DeliveryStockItemDto StockItem, decimal Packs);