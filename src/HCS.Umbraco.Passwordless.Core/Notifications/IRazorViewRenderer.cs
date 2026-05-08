namespace HCS.Umbraco.Passwordless.Notifications;

public interface IRazorViewRenderer
{
    Task<string> RenderAsync<TModel>(string viewName, TModel model, CancellationToken ct = default);
}
