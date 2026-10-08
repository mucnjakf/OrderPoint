using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Bartenders.Api.Requests;
using OrderPoint.Admin.Bartenders.Dtos;

namespace OrderPoint.Admin.Bartenders.Dialogs;

public sealed partial class UpdateBartenderDialog
{
    [Parameter]
    public BartenderDto Bartender { get; set; } = null!;

    [CascadingParameter]
    private IMudDialogInstance MudDialogInstance { get; set; } = null!;

    private UpdateBartenderRequest Request { get; set; } = null!;

    protected override void OnInitialized()
    {
        Request = new UpdateBartenderRequest
        {
            FirstName = Bartender.FirstName,
            LastName = Bartender.LastName,
            Email = Bartender.Email,
            PhoneNumber = Bartender.PhoneNumber,
            Status = Bartender.Status,
            Notes = Bartender.Notes,
            ImageUrl = Bartender.ImageUrl
        };
    }

    private void OnValidSubmit()
    {
        MudDialogInstance.Close(DialogResult.Ok(Request));
    }

    private void Cancel()
    {
        MudDialogInstance.Cancel();
    }
}