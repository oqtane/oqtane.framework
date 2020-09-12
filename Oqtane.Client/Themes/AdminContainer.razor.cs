using Microsoft.AspNetCore.Components;

namespace Oqtane.Themes
{
    public partial class AdminContainer : ContainerBase
    {
        private readonly NavigationManager _navigationManager;

        public AdminContainer (NavigationManager navigationManager)
	    {
            _navigationManager = navigationManager;

        }

        private void CloseModal()
        {
            _navigationManager.NavigateTo(NavigateUrl());
        }
    }
}
