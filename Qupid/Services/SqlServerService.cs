﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Data.SqlClient;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;
using DbExtensions;
using Qupid.Configuration;
using Qupid.Services;

namespace Qupid.Services
{
    public class SqlServerService
    {
        public readonly string ConnectionString;

        public readonly RouteConfiguration Route;

        public SqlServerService(string connectionString, RouteConfiguration route)
        {
            ConnectionString = connectionString;
            Route = route;
        }

        public DataTable ExecuteQuery(string sqlQuery)
        {
            DataTable resultSetDataTable = new DataTable();

            using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
            {
                sqlConnection.Open();

                using (SqlCommand sqlCommand = new SqlCommand(sqlQuery, sqlConnection))
                {
                    using (SqlDataReader sqlDataReader = sqlCommand.ExecuteReader())
                    {
                        // if route columns are configured
                        // then apply potential column mapping from database to api property
                        // else load the query results using the table column names
                        if (Route.Columns.Any())
                        {
                            foreach (ColumnConfiguration column in Route.Columns)
                            {
                                resultSetDataTable.Columns.Add(column.PropertyName);
                            }

                            while (sqlDataReader.Read())
                            {
                                DataRow row = resultSetDataTable.NewRow();
                                foreach (ColumnConfiguration column in Route.Columns)
                                {
                                    row[column.PropertyName] = sqlDataReader.GetValue(column.ColumnName);
                                }
                                resultSetDataTable.Rows.Add(row);
                            }
                        }
                        else
                        {
                            resultSetDataTable.Load(sqlDataReader);
                        }

                        sqlDataReader.Close();
                    }
                }

                sqlConnection.Close();
            }

            return resultSetDataTable;
        }

        //public DataTable GetColumnSchema(string columnName)
        //{
        //    DataTable tableColumnSchema;

        //    using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
        //    {
        //        sqlConnection.Open();

        //        string[] restrictionsColumns = new string[4];
        //        restrictionsColumns[2] = Route.Table;
        //        restrictionsColumns[3] = columnName;
        //        tableColumnSchema = sqlConnection.GetSchema("Columns", restrictionsColumns);

        //        sqlConnection.Close();
        //    }

        //    return tableColumnSchema;
        //}
    }
}
