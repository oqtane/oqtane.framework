namespace Oqtane.UI.Navigation
{
    public interface INavigator
    {
        string NavigateUrl(string path = "", string parameters = "");
    }
}
