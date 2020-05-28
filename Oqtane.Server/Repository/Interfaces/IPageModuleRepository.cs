using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IPageModuleRepository
    {
        IEnumerable<PageModule> GetPageModules(int siteId);
        IEnumerable<PageModule> GetPageModules(int pageId, string pane);
        PageModule AddPageModule(PageModule pageModule);
        PageModule UpdatePageModule(PageModule pageModule);
        PageModule GetPageModule(int pageModuleId);
        PageModule GetPageModule(int pageId, int moduleId);
        void DeletePageModule(int pageModuleId);
    }
}
