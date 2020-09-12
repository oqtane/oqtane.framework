using Microsoft.AspNetCore.Components;

namespace Oqtane.Themes.Controls
{
    public partial class UserProfile : ThemeControlBase
    {
        private readonly NavigationManager _navigationManager;

        public UserProfile(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;

        }

        private void RegisterUser()
        {
            _navigationManager.NavigateTo(NavigateUrl("register"));
        }

        private void UpdateProfile()
        {
            _navigationManager.NavigateTo(NavigateUrl("profile"));
        }
    }
}
