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
        private readonly TableSchema _schemaVerbs;
        private readonly ILogger<Function> _logger;
        private readonly BigQueryClient _client;
        private readonly TableSchema _schemaConjugations;
        private readonly TableSchema _infinitives;

        public Function(ILogger<Function> logger) {
            _logger = logger;
            var projectId = System.Environment.GetEnvironmentVariable("GCP_PROJECT"); //"parlr-342110"
            _logger.LogInformation(projectId);
            _client = Google.Cloud.BigQuery.V2.BigQueryClient.Create(projectId);
             _logger.LogDebug($"Prepare schema");
             _infinitives = new TableSchemaBuilder()
             {
                 new TableFieldSchema() {Name = "Infinitive", Type = "STRING"},
                 new TableFieldSchema() {Name = "Index", Type = "INTEGER"},
                 new TableFieldSchema() { Name = "Group", Type="INTEGER"}
             }.Build();
            _schemaVerbs = new TableSchemaBuilder 
            {
                new TableFieldSchema() { Name = "Infinitive", Type="STRING"},
                new TableFieldSchema() { Name = "Group", Type="INTEGER"},
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
            _schemaConjugations = new TableSchemaBuilder 
            {
                new TableFieldSchema() { Name = "Infinitive", Type="STRING"},
                new TableFieldSchema() { Name = "Group", Type="INTEGER"},
                new TableFieldSchema() { Name = "Id", Type="STRING"},
                new TableFieldSchema() { Name = "VerbIndex", Type="INTEGER"},
                new TableFieldSchema() { Name = "ConjugationIndex", Type="INTEGER"},
                new TableFieldSchema() { Name = "Time", Type="STRING"},
                new TableFieldSchema() { Name = "Male", Type="STRING"},
                new TableFieldSchema() { Name = "Female", Type="STRING"},
                new TableFieldSchema() { Name = "Combined", Type="STRING"},
                new TableFieldSchema() { Name = "Party", Type="INTEGER"},
                new TableFieldSchema() { Name = "Value", Type="STRING"},
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
                
                try
                {
                    _logger.LogDebug($"Drop existing infinitives table if exists");
                    await _client.DeleteTableAsync("verbs_dataset", "infinitives").ConfigureAwait(false);   

                }
                catch (Exception e)
                {
                    _logger.LogError(e, "unable to drop infinitives table");
                }
                
                try
                {
                    _logger.LogDebug($"Drop existing conjugation_flat table if exists"); 
                    await _client.DeleteTableAsync("verbs_dataset", "conjugation_flat").ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "unable to drop conjugation_flat table");
                }

                _logger.LogDebug($"Creating table verbs_table (ignored)");
                 await dataset.CreateTableAsync("verbs_table", _schemaVerbs).ConfigureAwait(false);   

                _logger.LogDebug($"Creating table infinitives");
                await dataset.CreateTableAsync("infinitives", _infinitives).ConfigureAwait(false); 
                
                _logger.LogDebug($"Creating table conjugation_flat");
                await dataset.CreateTableAsync("conjugation_flat", _schemaConjugations).ConfigureAwait(false);   
                
                _logger.LogInformation($"Schema was created successfully");
                await context.Response.WriteAsync(JsonSerializer.Serialize(new { Success = true}));
            }
            catch(Exception ex) {
                _logger.LogError(ex, $"Unable to craete schema {ex.Message}");
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
            } 
        }
    }
}
