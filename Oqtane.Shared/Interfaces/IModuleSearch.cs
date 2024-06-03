using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oqtane.Models;

namespace Oqtane.Interfaces
{
    public interface IModuleSearch
    {
        public IList<SearchDocument> GetSearchDocuments(Module module, DateTime startTime);
    }
}
