using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Interfaces
{
    public interface ISearchable
    {
        public List<SearchContent> GetSearchContents(Module module, DateTime startTime);
    }
}
