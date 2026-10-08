using MudBlazor;
using OrderPoint.Admin.Shared.Errors;

namespace OrderPoint.Admin.Shared.Services;

internal sealed class ApiService(ISnackbar snackbar)
{
    internal async Task<bool> ExecuteAsync(
        Func<Task> apiCall,
        string successMessage,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await apiCall();

            snackbar.Add(successMessage, Severity.Success);

            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return false;
        }
        catch (ApiException ex)
        {
            HandleApiException(ex);

            return false;
        }
        catch (Exception ex)
        {
            snackbar.Add(ex.Message, Severity.Error);

            return false;
        }
    }

    internal async Task<T?> ExecuteAsync<T>(Func<Task<T>> apiCall, CancellationToken cancellationToken = default)
    {
        try
        {
            return await apiCall();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return default;
        }
        catch (ApiException ex)
        {
            HandleApiException(ex);

            return default;
        }
        catch (Exception ex)
        {
            snackbar.Add(ex.Message, Severity.Error);

            return default;
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