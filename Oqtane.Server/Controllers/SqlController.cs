using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Oqtane.Models;
using System.Collections.Generic;
using Oqtane.Shared;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Enums;
using System;
using System.Data;

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
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

            if (string.IsNullOrEmpty(sqlquery.DBType) || string.IsNullOrEmpty(sqlquery.DBConnectionString))
            {
                Tenant tenant = _tenants.GetTenant(sqlquery.TenantId);
                if (tenant != null)
                {
                    sqlquery.DBType = tenant.DBType;
                    sqlquery.DBConnectionString = tenant.DBConnectionString;
                }
            }

            try
            {
                foreach (string query in sqlquery.Query.Split("GO", StringSplitOptions.RemoveEmptyEntries))
                {
                    IDataReader dr = _sql.ExecuteReader(sqlquery.DBType, sqlquery.DBConnectionString, query);
                    _logger.Log(LogLevel.Information, this, LogFunction.Other, "Sql Query {Query} Executed on Database {DBType} and Connection {DBConnectionString}", query, sqlquery.DBType, sqlquery.DBConnectionString);
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
                results.Add(new Dictionary<string, string>() { { "Error", ex.Message } });
                _logger.Log(LogLevel.Warning, this, LogFunction.Other, "Sql Query {Query} Executed on Database {DBType} and Connection {DBConnectionString} Resulted In An Error {Error}", sqlquery.Query, sqlquery.DBType, sqlquery.DBConnectionString, ex.Message);
            }
            sqlquery.Results = results;
            return sqlquery;
        }

    }
}
