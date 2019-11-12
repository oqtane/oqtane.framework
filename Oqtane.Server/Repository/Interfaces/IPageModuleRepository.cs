using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IPageModuleRepository
    {
        IEnumerable<PageModule> GetPageModules();
        IEnumerable<PageModule> GetPageModules(int SiteId);
        PageModule AddPageModule(PageModule PageModule);
        PageModule UpdatePageModule(PageModule PageModule);
        PageModule GetPageModule(int PageModuleId);
        PageModule GetPageModule(int PageId, int ModuleId);
        void DeletePageModule(int PageModuleId);
    }
}
