using MudBlazor;
using OrderPoint.Admin.Shared.Errors;

namespace OrderPoint.Admin.Shared.Services;

internal sealed class ApiService(ISnackbar snackbar)
{
    public async Task ExecuteAsync(Func<Task> apiCall)
    {
        try
        {
            await apiCall();
        }
        catch (ApiException ex)
        {
            HandleApiException(ex);
        }
        catch (Exception ex)
        {
            snackbar.Add(ex.Message, Severity.Error);
        }
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> apiCall)
    {
        try
        {
            return await apiCall();
        }
        catch (ApiException ex)
        {
            HandleApiException(ex);
            return default!;
        }
        catch (Exception ex)
        {
            snackbar.Add(ex.Message, Severity.Error);
            return default!;
        }
    }

    private void HandleApiException(ApiException ex)
    {
        if (ex.ProblemDetails.Errors is not null && ex.ProblemDetails.Errors.Length != 0)
        {
            foreach (Error error in ex.ProblemDetails.Errors)
            {
                snackbar.Add(error.Description, Severity.Error);
            }

            return;
        }

        snackbar.Add(ex.ProblemDetails.Detail, Severity.Error);
    }
}