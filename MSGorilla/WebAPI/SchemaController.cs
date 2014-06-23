﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using MSGorilla.Library;
using MSGorilla.Filters;
using MSGorilla.Library.Models;
using MSGorilla.Library.Exceptions;
using MSGorilla.Library.Models.SqlModels;

namespace MSGorilla.WebApi
{
    public class SchemaController : BaseController
    {
        SchemaManager _schemaManager = new SchemaManager();

        /// <summary>
        /// Return schema.
        /// </summary>
        /// <param name="schemaID">ID of the schema</param>
        /// <returns></returns>
        [HttpGet]
        public Schema GetSchema(string schemaID)
        {
            return _schemaManager.GetSchema(schemaID);
        }

        /// <summary>
        /// Return list of all schemas.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public List<Schema> GetSchema()
        {
            return _schemaManager.GetSchema();
        }

        /// <summary>
        /// Post a new schema
        /// </summary>
        /// <param name="schemaID">ID of the schema</param>
        /// <param name="schemaContent">content of a schema</param>
        /// <returns></returns>
        [HttpGet,HttpPost]
        public Schema PostSchema(string schemaID, string schemaContent)
        {
            Schema schema = new Schema(schemaID, schemaContent);
            _schemaManager.PostSchema(schema);
            return schema;
        }
    }
}