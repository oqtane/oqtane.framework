namespace Oqtane.Maui;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

    private static readonly byte[] _htmlPlaceholder = System.Text.Encoding.UTF8.GetBytes("<html><body>Asset Redirect</body></html>");

    private void BlazorWebView_WebResourceRequested(object sender, WebViewWebResourceRequestedEventArgs e)
    {
        if (e.Uri.Host != "0.0.0.1") return;

        var path = e.Uri.AbsolutePath;

        if (!path.Contains('.') || path.Contains("_framework")) return;

        e.Handled = true;

        //TODO: Get api url from configuration as well
        var redirectUrl = $"{MauiConstants.ApiUrl.TrimEnd('/')}{e.Uri.PathAndQuery}";

        // Create a minimal HTML body (some engines require non-empty content)
        using var stream = new MemoryStream(_htmlPlaceholder);

        // Set redirect headers
        var headers = new Dictionary<string, string>(e.Headers)
        {
            ["Location"] = redirectUrl
        };

        e.SetResponse(code: 302, reason: "Found", headers: headers, content: stream);
    }
}
