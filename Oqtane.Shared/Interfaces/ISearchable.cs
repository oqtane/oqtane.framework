using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Interfaces
{
    public interface ISearchable
    {
        public List<SearchContent> GetSearchContents(PageModule pageModule, DateTime startTime);
    }
}
