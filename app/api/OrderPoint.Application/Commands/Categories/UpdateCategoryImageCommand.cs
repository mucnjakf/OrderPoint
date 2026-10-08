using OrderPoint.Application.Mediator;
using OrderPoint.Application.Repositories;
using OrderPoint.Application.Storage;
using OrderPoint.Domain.Entities;
using OrderPoint.Domain.Errors;
using OrderPoint.Domain.Outcomes;

namespace OrderPoint.Application.Commands.Categories;

public sealed record UpdateCategoryImageCommand(Guid Id, Stream Content, string ContentType) : ICommand;

internal sealed class UpdateCategoryImageCommandHandler(
    ICategoryRepository categoryRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
    : ICommandHandler<UpdateCategoryImageCommand>
{
    private const string ImageFolder = "categories";

    public async Task<Result> Handle(UpdateCategoryImageCommand command, CancellationToken cancellationToken)
    {
        Category? category = await categoryRepository.GetAsync(command.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(CategoryErrors.NotFound);
        }

        string? previousImageUrl = category.ImageUrl;

        string imageUrl = await imageStorage.UploadAsync(
            command.Content,
            command.ContentType,
            ImageFolder,
            cancellationToken);

        Result result = category.SetImage(imageUrl);

        if (result.IsFailure)
        {
            return Result.Failure(result.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (previousImageUrl is not null)
        {
            await imageStorage.DeleteAsync(previousImageUrl, cancellationToken);
        }

        return Result.Success();
    }
}