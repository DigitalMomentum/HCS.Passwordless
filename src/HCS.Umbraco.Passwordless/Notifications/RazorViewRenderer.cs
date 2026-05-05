using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

namespace HCS.Umbraco.Passwordless.Notifications;

internal sealed class RazorViewRenderer : IRazorViewRenderer
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public RazorViewRenderer(
        IRazorViewEngine viewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> RenderAsync<TModel>(string viewName, TModel model, CancellationToken ct = default)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? new DefaultHttpContext { RequestServices = _serviceProvider };

        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        var viewResult = FindView(actionContext, viewName);
        if (!viewResult.Success)
            throw new InvalidOperationException($"Could not find view '{viewName}'. Searched: {string.Join(", ", viewResult.SearchedLocations ?? Array.Empty<string>())}");

        await using var writer = new StringWriter();
        var viewData = new ViewDataDictionary<TModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };
        var tempData = new TempDataDictionary(httpContext, _tempDataProvider);
        var viewContext = new ViewContext(
            actionContext, viewResult.View!, viewData, tempData, writer, new HtmlHelperOptions());

        await viewResult.View!.RenderAsync(viewContext);
        return writer.ToString();
    }

    private ViewEngineResult FindView(ActionContext context, string viewName)
    {
        var result = _viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: false);
        if (result.Success) return result;

        return _viewEngine.FindView(context, viewName, isMainPage: false);
    }
}
