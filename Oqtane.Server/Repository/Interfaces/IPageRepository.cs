using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IPageRepository
    {
        IEnumerable<Page> GetPages(bool IgnoreFilter = false);
        IEnumerable<Page> GetPages(int SiteId, bool IgnoreFilter = false);
        Page AddPage(Page Page);
        Page UpdatePage(Page Page);
        Page GetPage(int PageId);
        void DeletePage(int PageId);
    }
}
