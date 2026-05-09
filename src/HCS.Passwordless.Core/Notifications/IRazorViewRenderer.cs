namespace HCS.Passwordless.Notifications;

/// <summary>Renders a Razor partial view to an HTML string.</summary>
public interface IRazorViewRenderer
{
    /// <summary>Renders <paramref name="viewName"/> with <paramref name="model"/> and returns the HTML output.</summary>
    Task<string> RenderAsync<TModel>(string viewName, TModel model, CancellationToken ct = default);
}
