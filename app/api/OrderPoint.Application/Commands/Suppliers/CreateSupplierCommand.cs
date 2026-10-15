using OrderPoint.Application.Caching;
using OrderPoint.Application.Dtos;
using OrderPoint.Application.Dtos.Mappers;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Suppliers;

public sealed record CreateSupplierCommand(
    string Name,
    string? ContactName,
    string? Phone,
    string? Email,
    string? Note,
    SupplierStatus Status)
    : ICommand<SupplierDto>, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.SupplierChanges;
}

internal sealed class CreateSupplierCommandHandler(ISupplierRepository supplierRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateSupplierCommand, SupplierDto>
{
    public async Task<Result<SupplierDto>> Handle(CreateSupplierCommand command, CancellationToken cancellationToken)
    {
        bool nameExists = await supplierRepository.ExistsByNameAsync(command.Name, cancellationToken);

        if (nameExists)
        {
            return Result.Failure<SupplierDto>(SupplierErrors.NameAlreadyExists);
        }

        Result<Supplier> result = Supplier.Create(
            command.Name,
            command.ContactName,
            command.Phone,
            command.Email,
            command.Note,
            command.Status);

        if (result.IsFailure)
        {
            return Result.Failure<SupplierDto>(result.Error);
        }

        await supplierRepository.CreateAsync(result.Value, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // New: no stock items, nothing to reorder, never delivered
        var supplierDto = result.Value.ToSupplierDto(stockItemsCount: 0, reorderCount: 0, lastDeliveredOn: null);

        return Result.Success(supplierDto);
    }
}