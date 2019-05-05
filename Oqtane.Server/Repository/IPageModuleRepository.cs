using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IPageModuleRepository
    {
        IEnumerable<PageModule> GetPageModules();
        IEnumerable<PageModule> GetPageModules(int PageId);
        void AddPageModule(PageModule PageModule);
        void UpdatePageModule(PageModule PageModule);
        PageModule GetPageModule(int PageModuleId);
        void DeletePageModule(int PageModuleId);
    }
}
