using OrderPoint.Application.Caching;
using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Enumerations;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Suppliers;

public sealed record UpdateSupplierCommand(
    Guid Id,
    string Name,
    string? ContactName,
    string? Phone,
    string? Email,
    string? Note,
    SupplierStatus Status) : ICommand, ICacheInvalidatingCommand
{
    IReadOnlyList<string> ICacheInvalidatingCommand.InvalidatedCacheTags => CacheTags.SupplierChanges;
}

internal sealed class UpdateSupplierCommandHandler(ISupplierRepository supplierRepository, IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateSupplierCommand>
{
    public async Task<Result> Handle(UpdateSupplierCommand command, CancellationToken cancellationToken)
    {
        Supplier? supplier = await supplierRepository.GetAsync(command.Id, cancellationToken);

        if (supplier is null)
        {
            return Result.Failure(SupplierErrors.NotFound);
        }

        if (command.Name != supplier.Name)
        {
            bool nameExists = await supplierRepository.ExistsByNameAsync(command.Name, cancellationToken);

            if (nameExists)
            {
                return Result.Failure(SupplierErrors.NameAlreadyExists);
            }
        }

        Result result = supplier.Update(
            command.Name,
            command.ContactName,
            command.Phone,
            command.Email,
            command.Note,
            command.Status);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}