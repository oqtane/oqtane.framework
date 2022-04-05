using System;
using System.Collections.Generic;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public interface IVisitorRepository
    {
        IEnumerable<Visitor> GetVisitors(int siteId, DateTime fromDate);
        Visitor AddVisitor(Visitor visitor);
        Visitor UpdateVisitor(Visitor visitor);
        Visitor GetVisitor(int visitorId);
        Visitor GetVisitor(int siteId, string IPAddress);
        void DeleteVisitor(int visitorId);
        int DeleteVisitors(int siteId, int age);
    }
}
