using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IPageRepository
    {
        IEnumerable<Page> GetPages(int SiteId);
        Page AddPage(Page Page);
        Page UpdatePage(Page Page);
        Page GetPage(int PageId);
        Page GetPage(int PageId, int UserId);
        Page GetPage(string Path, int SiteId);
        void DeletePage(int PageId);
    }
}
