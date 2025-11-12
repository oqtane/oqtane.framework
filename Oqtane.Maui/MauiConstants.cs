namespace Oqtane.Maui;

public static class MauiConstants
{
    // the API service url (used as fallback if not set in appsettings.json)
    public static string ApiUrl = "";
    //public static string ApiUrl = $"http://{GetLocalhost()}:44357/"; // for local development (Oqtane.Server must be already running for MAUI client to connect)
    //public static string apiurl = $"http://{GetLocalhost()}:44357/sitename/"; // local microsite example
    //public static string apiurl = "https://www.dnfprojects.com/"; // for testing remote site

    // specify if you wish to allow users to override the url via appsettings.json in the AppDataDirectory
    public static bool UseAppSettings = true;

    private static string GetLocalhost()
    {
        return DeviceInfo.Platform == DevicePlatform.Android ? "10.0.2.2" : "localhost";
    }
}
