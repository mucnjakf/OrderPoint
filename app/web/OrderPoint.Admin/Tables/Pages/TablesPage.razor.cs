using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using OrderPoint.Admin.Tables.Api;
using OrderPoint.Admin.Tables.Api.Requests;
using OrderPoint.Admin.Tables.Dialogs;
using OrderPoint.Admin.Tables.Services;
using OrderPoint.Shared.Extensions;
using OrderPoint.Shared.Orders.Services;
using OrderPoint.Shared.Services;
using OrderPoint.Shared.Tables.Dtos;
using OrderPoint.Shared.Tables.Enumerations;

namespace OrderPoint.Admin.Tables.Pages;

// The floor plan editor: one tab per area, drag to move, the panel edits what is selected
public sealed partial class TablesPage : IDisposable
{
    private const string NoRoomMessage = "There is no room for it here. Make the area bigger or move something.";

    private const string TableCodePrefix = "T";

    // What the library above the map adds, with its size in cells
    private static readonly LibraryTable[] TableLibrary =
    [
        new("Square", Icons.Material.Filled.CropSquare, TableShape.Rectangle, 2, 2),
        new("Round", Icons.Material.Filled.RadioButtonUnchecked, TableShape.Round, 2, 2),
        new("Long", Icons.Material.Filled.Crop169, TableShape.Rectangle, 4, 2)
    ];

    private static readonly LibraryFixture[] FixtureLibrary =
    [
        new(Icons.Material.Filled.LocalBar, FixtureType.BarCounter, 6, 1),
        new(Icons.Material.Filled.MeetingRoom, FixtureType.Entrance, 2, 1),
        new(Icons.Material.Filled.HorizontalRule, FixtureType.Wall, 6, 1)
    ];

    [Inject]
    private ApiService ApiService { get; set; } = null!;

    [Inject]
    private TableApiClient TableApiClient { get; set; } = null!;

    [Inject]
    private TableQrCodeService TableQrCodeService { get; set; } = null!;

    [Inject]
    private OrdersHubService OrdersHubService { get; set; } = null!;

    [Inject]
    private IDialogService DialogService { get; set; } = null!;

    [Inject]
    private ISnackbar Snackbar { get; set; } = null!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = null!;

    // A table opened from the search: /tables?table=T7
    [SupplyParameterFromQuery(Name = "table")]
    public string? TableCode { get; set; }

    private IReadOnlyList<AreaDto> Areas { get; set; } = [];

    private bool IsLoading { get; set; } = true;

    private Guid? SelectedAreaId { get; set; }

    private Guid? SelectedPieceId { get; set; }

    private AreaDto? SelectedArea =>
        Areas.FirstOrDefault(area => area.Id == SelectedAreaId) ?? Areas.FirstOrDefault();

    private TableDto? SelectedTable => SelectedArea?.Tables.FirstOrDefault(table => table.Id == SelectedPieceId);

    private FixtureDto? SelectedFixture =>
        SelectedArea?.Fixtures.FirstOrDefault(fixture => fixture.Id == SelectedPieceId);

    private string? SelectedTableUrl =>
        SelectedTable is null ? null : TableQrCodeService.GetTableUrl(SelectedTable.Code);

    private string? SelectedTableQrCodeSvg =>
        SelectedTableUrl is null ? null : TableQrCodeService.GetSvg(SelectedTableUrl);

    protected override async Task OnInitializedAsync()
    {
        OrdersHubService.TablesChanged += OnTablesChanged;

        await GetFloorPlanAsync();

        SelectTableFromQuery();
    }

    // Also when the search opens a table while this page is already shown
    protected override void OnParametersSet()
    {
        SelectTableFromQuery();
    }

    public void Dispose()
    {
        OrdersHubService.TablesChanged -= OnTablesChanged;
    }

    private void OnTablesChanged()
    {
        _ = InvokeAsync(() => GetFloorPlanAsync(isLiveUpdate: true));
    }

    // A live update shows no skeleton, and a failed one keeps what is shown
    private async Task GetFloorPlanAsync(bool isLiveUpdate = false)
    {
        if (!isLiveUpdate)
        {
            IsLoading = true;
        }

        IReadOnlyList<AreaDto>? floorPlan = await ApiService.ExecuteAsync(() => TableApiClient.GetFloorPlanAsync());

        if (floorPlan is not null || !isLiveUpdate)
        {
            Areas = floorPlan ?? [];
        }

        IsLoading = false;

        StateHasChanged();
    }

    // The link is dropped once applied, so opening the same table again from the search selects it again
    private void SelectTableFromQuery()
    {
        if (string.IsNullOrWhiteSpace(TableCode) || IsLoading)
        {
            return;
        }

        AreaDto? area = Areas.FirstOrDefault(area => area.Tables.Any(table => table.Code == TableCode));

        if (area is not null)
        {
            SelectedAreaId = area.Id;
            SelectedPieceId = area.Tables.First(table => table.Code == TableCode).Id;
        }

        NavigationManager.NavigateTo("/tables", replace: true);
    }

    private void SelectArea(Guid areaId)
    {
        SelectedAreaId = areaId;
        SelectedPieceId = null;
    }

    private void SelectPiece(Guid? pieceId)
    {
        SelectedPieceId = pieceId;
    }

    private void SelectTable(TableDto table)
    {
        SelectedPieceId = table.Id;
    }

    // A drop on the floor plan; it stays where it was dropped only if it fits there
    private async Task<bool> MovePieceAsync(FloorPlanMoveDto move)
    {
        AreaDto area = SelectedArea!;

        TableDto? table = area.Tables.FirstOrDefault(table => table.Id == move.Id);

        if (table is not null)
        {
            return await PlaceTableAsync(area, table, move.X, move.Y, table.Width, table.Height);
        }

        FixtureDto? fixture = area.Fixtures.FirstOrDefault(fixture => fixture.Id == move.Id);

        if (fixture is not null)
        {
            return await PlaceFixtureAsync(area, fixture, move.X, move.Y, fixture.Width, fixture.Height);
        }

        return false;
    }

    private async Task RotateTableAsync()
    {
        TableDto table = SelectedTable!;

        bool isPlaced = await PlaceTableAsync(SelectedArea!, table, table.X, table.Y, table.Height, table.Width);

        if (!isPlaced)
        {
            Snackbar.Add(NoRoomMessage, Severity.Warning);
        }
    }

    private async Task RotateFixtureAsync()
    {
        FixtureDto fixture = SelectedFixture!;

        bool isPlaced = await PlaceFixtureAsync(
            SelectedArea!,
            fixture,
            fixture.X,
            fixture.Y,
            fixture.Height,
            fixture.Width);

        if (!isPlaced)
        {
            Snackbar.Add(NoRoomMessage, Severity.Warning);
        }
    }

    // Shown at once and saved in the background; a refused save reloads the floor plan
    private async Task<bool> PlaceTableAsync(AreaDto area, TableDto table, int x, int y, int width, int height)
    {
        if (!Fits(area, table.Id, x, y, width, height))
        {
            return false;
        }

        ReplaceArea(area with
        {
            Tables = area.Tables
                .Select(other => other.Id == table.Id
                    ? other with { X = x, Y = y, Width = width, Height = height }
                    : other)
                .ToList()
        });

        var request = new MoveTableRequest { AreaId = area.Id, X = x, Y = y, Width = width, Height = height };

        bool isSaved = await ApiService.ExecuteAsync(async () =>
        {
            await TableApiClient.MoveTableAsync(table.Id, request);

            return true;
        });

        if (!isSaved)
        {
            await GetFloorPlanAsync(isLiveUpdate: true);
        }

        return isSaved;
    }

    private async Task<bool> PlaceFixtureAsync(
        AreaDto area,
        FixtureDto fixture,
        int x,
        int y,
        int width,
        int height)
    {
        if (!Fits(area, fixture.Id, x, y, width, height))
        {
            return false;
        }

        ReplaceArea(area with
        {
            Fixtures = area.Fixtures
                .Select(other => other.Id == fixture.Id
                    ? other with { X = x, Y = y, Width = width, Height = height }
                    : other)
                .ToList()
        });

        var request = new MoveFixtureRequest { AreaId = area.Id, X = x, Y = y, Width = width, Height = height };

        bool isSaved = await ApiService.ExecuteAsync(async () =>
        {
            await TableApiClient.MoveFixtureAsync(fixture.Id, request);

            return true;
        });

        if (!isSaved)
        {
            await GetFloorPlanAsync(isLiveUpdate: true);
        }

        return isSaved;
    }

    private void ReplaceArea(AreaDto area)
    {
        Areas = Areas.Select(other => other.Id == area.Id ? area : other).ToList();

        StateHasChanged();
    }

    private async Task ShowCreateAreaDialogAsync()
    {
        IDialogReference dialogReference = await DialogService
            .ShowAsync<CreateAreaDialog>(string.Empty, CreateFormOptions());

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as CreateAreaRequest)!;

        Guid? createdAreaId = null;

        bool isSuccess = await ApiService.ExecuteAsync(
            async () => createdAreaId = (await TableApiClient.CreateAreaAsync(request)).Id,
            $"Area {request.Name} created successfully");

        if (!isSuccess)
        {
            return;
        }

        await GetFloorPlanAsync(isLiveUpdate: true);

        SelectArea(createdAreaId!.Value);
    }

    private async Task ShowUpdateAreaDialogAsync()
    {
        AreaDto area = SelectedArea!;

        var parameters = new DialogParameters<UpdateAreaDialog>
        {
            { dialog => dialog.Area, area }
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<UpdateAreaDialog>(string.Empty, parameters, CreateFormOptions());

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as UpdateAreaRequest)!;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => TableApiClient.UpdateAreaAsync(area.Id, request),
            $"Area {request.Name} updated successfully");

        if (isSuccess)
        {
            await GetFloorPlanAsync(isLiveUpdate: true);
        }
    }

    private async Task ShowDeleteAreaDialogAsync()
    {
        AreaDto area = SelectedArea!;

        var parameters = new DialogParameters<DeleteAreaDialog>
        {
            { dialog => dialog.Area, area }
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<DeleteAreaDialog>(string.Empty, parameters, CreateFormOptions());

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        bool isSuccess = await ApiService.ExecuteAsync(
            () => TableApiClient.DeleteAreaAsync(area.Id),
            $"Area {area.Name} deleted successfully");

        if (!isSuccess)
        {
            return;
        }

        SelectedAreaId = null;
        SelectedPieceId = null;

        await GetFloorPlanAsync(isLiveUpdate: true);
    }

    // A new table goes to the first free spot of the area, top left first
    private async Task ShowCreateTableDialogAsync(LibraryTable libraryTable)
    {
        AreaDto area = SelectedArea!;

        (int X, int Y)? spot = FindFreeSpot(area, libraryTable.Width, libraryTable.Height);

        if (spot is null)
        {
            Snackbar.Add(NoRoomMessage, Severity.Warning);

            return;
        }

        var parameters = new DialogParameters<CreateTableDialog>
        {
            { dialog => dialog.SuggestedCode, GetSuggestedTableCode() },
            { dialog => dialog.Shape, libraryTable.Shape }
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<CreateTableDialog>(string.Empty, parameters, CreateFormOptions());

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as CreateTableRequest)!;

        request.AreaId = area.Id;
        request.X = spot.Value.X;
        request.Y = spot.Value.Y;
        request.Width = libraryTable.Width;
        request.Height = libraryTable.Height;

        Guid? createdTableId = null;

        bool isSuccess = await ApiService.ExecuteAsync(
            async () => createdTableId = (await TableApiClient.CreateTableAsync(request)).Id,
            $"Table {request.Code} created successfully");

        if (!isSuccess)
        {
            return;
        }

        await GetFloorPlanAsync(isLiveUpdate: true);

        SelectedPieceId = createdTableId;
    }

    private async Task ShowUpdateTableDialogAsync()
    {
        AreaDto area = SelectedArea!;
        TableDto table = SelectedTable!;

        var parameters = new DialogParameters<UpdateTableDialog>
        {
            { dialog => dialog.Table, table }
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<UpdateTableDialog>(string.Empty, parameters, CreateFormOptions());

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as UpdateTableRequest)!;

        bool isResized = request.Width != table.Width || request.Height != table.Height;

        if (isResized && !Fits(area, table.Id, table.X, table.Y, request.Width, request.Height))
        {
            Snackbar.Add(NoRoomMessage, Severity.Warning);

            return;
        }

        bool isSuccess = await ApiService.ExecuteAsync(
            () => UpdateTableAsync(area, table, request, isResized),
            $"Table {request.Code} updated successfully");

        if (isSuccess)
        {
            await GetFloorPlanAsync(isLiveUpdate: true);
        }
    }

    // The size is saved with the position endpoint
    private async Task UpdateTableAsync(AreaDto area, TableDto table, UpdateTableRequest request, bool isResized)
    {
        await TableApiClient.UpdateTableAsync(table.Id, request);

        if (!isResized)
        {
            return;
        }

        var moveRequest = new MoveTableRequest
        {
            AreaId = area.Id,
            X = table.X,
            Y = table.Y,
            Width = request.Width,
            Height = request.Height
        };

        await TableApiClient.MoveTableAsync(table.Id, moveRequest);
    }

    private async Task ShowDeleteTableDialogAsync()
    {
        TableDto table = SelectedTable!;

        var parameters = new DialogParameters<DeleteTableDialog>
        {
            { dialog => dialog.Table, table }
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<DeleteTableDialog>(string.Empty, parameters, CreateFormOptions());

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        bool isSuccess = await ApiService.ExecuteAsync(
            () => TableApiClient.DeleteTableAsync(table.Id),
            $"Table {table.Code} deleted successfully");

        if (!isSuccess)
        {
            return;
        }

        SelectedPieceId = null;

        await GetFloorPlanAsync(isLiveUpdate: true);
    }

    // Fixtures need no form: they go to the first free spot and the panel edits them
    private async Task CreateFixtureAsync(LibraryFixture libraryFixture)
    {
        AreaDto area = SelectedArea!;

        (int X, int Y)? spot = FindFreeSpot(area, libraryFixture.Width, libraryFixture.Height);

        if (spot is null)
        {
            Snackbar.Add(NoRoomMessage, Severity.Warning);

            return;
        }

        var request = new CreateFixtureRequest
        {
            AreaId = area.Id,
            Type = libraryFixture.Type,
            X = spot.Value.X,
            Y = spot.Value.Y,
            Width = libraryFixture.Width,
            Height = libraryFixture.Height
        };

        // No snackbar: the fixture appears on the map, selected
        FixtureDto? fixture = await ApiService.ExecuteAsync(() => TableApiClient.CreateFixtureAsync(request));

        if (fixture is null)
        {
            return;
        }

        await GetFloorPlanAsync(isLiveUpdate: true);

        SelectedPieceId = fixture.Id;
    }

    private async Task ShowUpdateFixtureDialogAsync()
    {
        AreaDto area = SelectedArea!;
        FixtureDto fixture = SelectedFixture!;

        var parameters = new DialogParameters<UpdateFixtureDialog>
        {
            { dialog => dialog.Fixture, fixture }
        };

        IDialogReference dialogReference = await DialogService
            .ShowAsync<UpdateFixtureDialog>(string.Empty, parameters, CreateFormOptions());

        DialogResult dialogResult = (await dialogReference.Result)!;

        if (dialogResult.Canceled)
        {
            return;
        }

        var request = (dialogResult.Data as UpdateFixtureRequest)!;

        bool isResized = request.Width != fixture.Width || request.Height != fixture.Height;

        if (isResized && !Fits(area, fixture.Id, fixture.X, fixture.Y, request.Width, request.Height))
        {
            Snackbar.Add(NoRoomMessage, Severity.Warning);

            return;
        }

        bool isSuccess = await ApiService.ExecuteAsync(
            () => UpdateFixtureAsync(area, fixture, request, isResized),
            $"{request.Type.GetLabel()} updated successfully");

        if (isSuccess)
        {
            await GetFloorPlanAsync(isLiveUpdate: true);
        }
    }

    private async Task UpdateFixtureAsync(
        AreaDto area,
        FixtureDto fixture,
        UpdateFixtureRequest request,
        bool isResized)
    {
        await TableApiClient.UpdateFixtureAsync(fixture.Id, request);

        if (!isResized)
        {
            return;
        }

        var moveRequest = new MoveFixtureRequest
        {
            AreaId = area.Id,
            X = fixture.X,
            Y = fixture.Y,
            Width = request.Width,
            Height = request.Height
        };

        await TableApiClient.MoveFixtureAsync(fixture.Id, moveRequest);
    }

    // Easy to put back, so no confirmation
    private async Task DeleteFixtureAsync()
    {
        FixtureDto fixture = SelectedFixture!;

        bool isSuccess = await ApiService.ExecuteAsync(
            () => TableApiClient.DeleteFixtureAsync(fixture.Id),
            $"{fixture.Type.GetLabel()} deleted successfully");

        if (!isSuccess)
        {
            return;
        }

        SelectedPieceId = null;

        await GetFloorPlanAsync(isLiveUpdate: true);
    }

    // The next free "T" number, so tables added one after another get T13, T14, ...
    private string GetSuggestedTableCode()
    {
        int highestNumber = Areas
            .SelectMany(area => area.Tables)
            .Select(table => TableNumberRegex().Match(table.Code))
            .Where(match => match.Success)
            .Select(match => int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture))
            .DefaultIfEmpty(0)
            .Max();

        return $"{TableCodePrefix}{highestNumber + 1}";
    }

    // Inside the grid and on no other table or fixture
    private static bool Fits(AreaDto area, Guid? pieceId, int x, int y, int width, int height)
    {
        if (x < 0 || y < 0 || x + width > area.Columns || y + height > area.Rows)
        {
            return false;
        }

        IEnumerable<(Guid Id, int X, int Y, int Width, int Height)> pieces = area.Tables
            .Select(table => (table.Id, table.X, table.Y, table.Width, table.Height))
            .Concat(area.Fixtures.Select(fixture => (fixture.Id, fixture.X, fixture.Y, fixture.Width, fixture.Height)));

        return !pieces.Any(piece =>
            piece.Id != pieceId
            && x < piece.X + piece.Width
            && piece.X < x + width
            && y < piece.Y + piece.Height
            && piece.Y < y + height);
    }

    private static (int X, int Y)? FindFreeSpot(AreaDto area, int width, int height)
    {
        for (int y = 0; y + height <= area.Rows; y++)
        {
            for (int x = 0; x + width <= area.Columns; x++)
            {
                if (Fits(area, pieceId: null, x, y, width, height))
                {
                    return (x, y);
                }
            }
        }

        return null;
    }

    private static DialogOptions CreateFormOptions()
    {
        return new DialogOptions
        {
            MaxWidth = MaxWidth.Small,
            FullWidth = true
        };
    }

    [GeneratedRegex("^T([0-9]+)$")]
    private static partial Regex TableNumberRegex();

    private sealed record LibraryTable(string Label, string Icon, TableShape Shape, int Width, int Height);

    private sealed record LibraryFixture(string Icon, FixtureType Type, int Width, int Height);
}