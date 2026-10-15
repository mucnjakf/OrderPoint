using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.AspNetCore.Components.Web;

namespace OrderPoint.Register;

public sealed partial class App
{
    [CascadingParameter]
    private HttpContext HttpContext { get; set; } = null!;

    // The error page is rendered statically: after the exception handler re-executes, the interactive router
    // would render the failing URL again
    private IComponentRenderMode? PageRenderMode => HttpContext.AcceptsInteractiveRouting()
        ? new InteractiveServerRenderMode(prerender: false)
        : null;
}