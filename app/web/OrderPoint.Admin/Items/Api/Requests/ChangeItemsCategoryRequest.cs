using System.Text.Json.Serialization;

namespace OrderPoint.Admin.Items.Api.Requests;

internal sealed class ChangeItemsCategoryRequest
{
    public IReadOnlyList<Guid> Ids { get; set; } = [];

    public Guid CategoryId { get; set; }

    [JsonIgnore]
    public string CategoryName { get; set; } = null!;
}