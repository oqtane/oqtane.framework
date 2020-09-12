using Oqtane.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.Themes.Controls
{
    public partial class Breadcrumbs : ThemeControlBase
    {
        protected IEnumerable<Page> BreadCrumbPages => GetBreadCrumbPages().Reverse().ToList();

        private IEnumerable<Page> GetBreadCrumbPages()
        {
            var page = PageState.Page;
            do
            {
                yield return page;
                page = PageState.Pages.FirstOrDefault(p => page != null && p.PageId == page.ParentId);
            } while (page != null);
        }
    }
}
