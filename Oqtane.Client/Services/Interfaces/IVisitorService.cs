using Oqtane.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage <see cref="Visitor"/>s on a <see cref="Site"/>
    /// </summary>
    public interface IVisitorService
    {
        /// <summary>
        /// Get all <see cref="Visitor"/>s of this <see cref="Site"/>.
        ///
        /// </summary>
        /// <param name="siteId">ID-reference of a <see cref="Site"/></param>
        /// <returns></returns>
        Task<List<Visitor>> GetVisitorsAsync(int siteId, DateTime fromDate);

        /// <summary>
        /// Get a specific <see cref="Visitor"/> of this <see cref="Site"/>.
        ///
        /// </summary>
        /// <param name="visitorId">ID-reference of a <see cref="Visitor"/></param>
        /// <returns></returns>
        Task<Visitor> GetVisitorAsync(int visitorId);
    }
}
