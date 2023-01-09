using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IPageModuleRepository
    {
        IEnumerable<PageModule> GetPageModules(int siteId);
        PageModule AddPageModule(PageModule pageModule);
        PageModule UpdatePageModule(PageModule pageModule);
        PageModule GetPageModule(int pageModuleId);
        PageModule GetPageModule(int pageModuleId, bool tracking);
        PageModule GetPageModule(int pageId, int moduleId);
        void DeletePageModule(int pageModuleId);
    }
}
