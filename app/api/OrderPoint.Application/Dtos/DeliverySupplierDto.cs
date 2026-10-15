namespace OrderPoint.Application.Dtos;

/// <summary>The supplier of a delivery.</summary>
/// <param name="Id">The supplier's id.</param>
/// <param name="Name">The supplier's name.</param>
public sealed record DeliverySupplierDto(Guid Id, string Name);