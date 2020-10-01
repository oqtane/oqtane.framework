namespace Oqtane.Infrastructure
{
    public interface ILocalizationManager
    {
        string GetDefaultCulture();

        string[] GetSupportedCultures();
    }
}
