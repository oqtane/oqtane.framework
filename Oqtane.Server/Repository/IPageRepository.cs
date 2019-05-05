using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IPageRepository
    {
        IEnumerable<Page> GetPages();
        IEnumerable<Page> GetPages(int SiteId);
        void AddPage(Page Page);
        void UpdatePage(Page Page);
        Page GetPage(int PageId);
        void DeletePage(int PageId);
    }
}
