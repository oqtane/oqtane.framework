using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IPageRepository
    {
        IEnumerable<Page> GetPages(int siteId);
        Page AddPage(Page page);
        Page UpdatePage(Page page);
        Page GetPage(int pageId);
        Page GetPage(int pageId, int userId);
        Page GetPage(string path, int siteId);
        void DeletePage(int pageId);
    }
}
