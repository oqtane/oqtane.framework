using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Interfaces
{
    public interface ISearchable
    {
        public IList<SearchContent> GetSearchContent(Module module, DateTime startTime);
    }
}
