namespace OrderPoint.Admin.Shared.Errors;

// Thrown instead of calling the API when there is no session; the layout already shows the login form
internal sealed class SessionExpiredException() : Exception("The session has expired");