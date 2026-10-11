using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Items;

public sealed record AdjustItemsPriceCommand(
    IReadOnlyList<Guid> Ids,
    PriceAdjustmentMode Mode,
    decimal Value) : ICommand;

internal sealed class AdjustItemsPriceCommandHandler(
    IItemRepository itemRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AdjustItemsPriceCommand>
{
    private const decimal PercentBase = 100m;

    private const int PriceDecimals = 2;

    public async Task<Result> Handle(AdjustItemsPriceCommand command, CancellationToken cancellationToken)
    {
        IReadOnlyList<Item> items = await itemRepository.GetAsync(command.Ids, cancellationToken);

        if (items.Count != command.Ids.Count)
        {
            return Result.Failure(ItemErrors.NotFound);
        }

        foreach (Item item in items)
        {
            decimal price = CalculatePrice(item.Price, command.Mode, command.Value);

            Result result = item.ChangePrice(price);

            if (result.IsFailure)
            {
                return Result.Failure(result.Error);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static decimal CalculatePrice(decimal price, PriceAdjustmentMode mode, decimal value)
    {
        decimal newPrice = mode switch
        {
            PriceAdjustmentMode.Set => value,
            PriceAdjustmentMode.ChangeByPercent => price * (1 + value / PercentBase),
            PriceAdjustmentMode.ChangeByAmount => price + value,
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, null)
        };

        return Math.Round(newPrice, PriceDecimals, MidpointRounding.AwayFromZero);
    }
}