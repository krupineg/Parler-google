using Google.Apis.Bigquery.v2.Data;
using Google.Cloud.BigQuery.V2;
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace CreateSchema
{
    public class Function : IHttpFunction
    {
        private readonly TableSchema _schema;
        private readonly ILogger<Function> _logger;
        private readonly Google.Cloud.BigQuery.V2.BigQueryClient _client;

        public Function(ILogger<Function> logger) {
            _logger = logger;
            var projectId = System.Environment.GetEnvironmentVariable("GCP_PROJECT"); //"parlr-342110"
            _logger.LogInformation(projectId);
            _client = Google.Cloud.BigQuery.V2.BigQueryClient.Create(projectId);
             _logger.LogDebug($"Prepare schema");
            _schema = new TableSchemaBuilder 
            {
                new TableFieldSchema() { Name = "Infinitive", Type="STRING"},
                new TableFieldSchema() { Name = "Id", Type="STRING"},
                new TableFieldSchema() {  Name="TimeConjugations", Type="STRUCT", Mode = "REPEATED", Fields = new List<TableFieldSchema> {
                        new TableFieldSchema() { Name = "Id", Type="STRING"},
                        new TableFieldSchema() { Name = "Time", Type="STRING"},
                        new TableFieldSchema() { Name="Conjugations", Type="STRUCT", Mode = "REPEATED", Fields = new List<TableFieldSchema> { 
                            new TableFieldSchema() { Name = "Id", Type="STRING"},
                            new TableFieldSchema() { Name = "Male", Type="STRING"},
                            new TableFieldSchema() { Name = "Female", Type="STRING"},
                            new TableFieldSchema() { Name = "Combined", Type="STRING"},
                            new TableFieldSchema() { Name = "Party", Type="INTEGER"},
                            new TableFieldSchema() { Name = "Value", Type="STRING"},
                        }}
                    }
                }
            }.Build();
        }

        /// <summary>
        /// Logic for your function goes here.
        /// </summary>
        /// <param name="context">The HTTP context, containing the request and the response.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleAsync(HttpContext context)
        {
            try{
                _logger.LogInformation($"Creating schema");
                var dataset = _client.GetOrCreateDataset("verbs_dataset");

                if(context.Request.Query.ContainsKey("drop")) {
                    _logger.LogDebug($"Drop existing table if exists");
                    await _client.DeleteTableAsync("verbs_dataset", "verbs_table").ConfigureAwait(false);   
                }                

                 _logger.LogDebug($"Creating table verbs_table");
                await dataset.CreateTableAsync("verbs_table", _schema).ConfigureAwait(false);   
            
                _logger.LogInformation($"Schema was craeted successfully");
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { Success = true}));
            }
            catch(Exception ex) {
                _logger.LogError("Unable to craete schema", ex);
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            } 
        }
    }
}
