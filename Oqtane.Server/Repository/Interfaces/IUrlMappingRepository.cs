using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IUrlMappingRepository
    {
        IEnumerable<UrlMapping> GetUrlMappings(int siteId, bool isMapped);
        UrlMapping AddUrlMapping(UrlMapping urlMapping);
        UrlMapping UpdateUrlMapping(UrlMapping urlMapping);
        UrlMapping GetUrlMapping(int urlMappingId);
        UrlMapping GetUrlMapping(int urlMappingId, bool tracking);
        UrlMapping GetUrlMapping(int siteId, string url);
        void DeleteUrlMapping(int urlMappingId);
    }
}
