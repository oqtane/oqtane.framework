using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using System.Collections.Generic;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Enums;
using System.Data.SqlClient;
using System.Data;
using System.Dynamic;
using Newtonsoft.Json;
using System;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.Default)]
    public class SqlController : Controller
    {
        private readonly ITenantRepository _tenants;
        private readonly ISqlRepository _sql;
        private readonly ILogManager _logger;

        public SqlController(ITenantRepository tenants, ISqlRepository sql, ILogManager logger)
        {
            _tenants = tenants;
            _sql = sql;
            _logger = logger;
        }

        // POST: api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Host)]
        public SqlQuery Post([FromBody] SqlQuery sqlquery)
        {
            var results = new List<Dictionary<string, string>>();
            Dictionary<string, string> row;
            Tenant tenant = _tenants.GetTenant(sqlquery.TenantId);
            try
            {
                foreach (string query in sqlquery.Query.Split("GO", StringSplitOptions.RemoveEmptyEntries))
                {
                    SqlDataReader dr = _sql.ExecuteReader(tenant, query);
                    _logger.Log(LogLevel.Information, this, LogFunction.Other, "Sql Query {Query} Executed on Tenant {TenantId}", query, sqlquery.TenantId);
                    while (dr.Read())
                    {
                        row = new Dictionary<string, string>();
                        for (var field = 0; field < dr.FieldCount; field++)
                        {
                            row[dr.GetName(field)] = dr.IsDBNull(field) ? "" : dr.GetValue(field).ToString();
                        }
                        results.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Other, "Sql Query {Query} Executed on Tenant {TenantId} Results In An Error {Error}", sqlquery.Query, sqlquery.TenantId, ex.Message);
            }
            sqlquery.Results = results;
            return sqlquery;
        }

    }
}
