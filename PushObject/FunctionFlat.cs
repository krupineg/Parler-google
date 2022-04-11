using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Google.Cloud.BigQuery.V2;
using Google.Cloud.Storage.V1;
using Google.Apis.Bigquery.v2.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PushObject.Model;
using PushObject.Model.V2;

namespace PushObject
{
    /// <summary>
    /// A function that can be triggered in responses to changes in Google Cloud Storage.
    /// The type argument (StorageObjectData in this case) determines how the event payload is deserialized.
    /// The function must be deployed so that the trigger matches the expected payload type. (For example,
    /// deploying a function expecting a StorageObject payload will not work for a trigger that provides
    /// a FirestoreEvent.)
    /// </summary>
    public class FunctionFlat : ICloudEventFunction<StorageObjectData>
    {
        private readonly ILogger<FunctionFlat> _logger;
        private readonly HandlerFlat _handler;

        public FunctionFlat(ILogger<FunctionFlat> logger, ILogger<HandlerFlat> handlerLogger) {
            _logger = logger;
            _handler = new HandlerFlat(handlerLogger, new ProjectIdProvider());
        }

        public Task HandleAsync(CloudEvent cloudEvent, StorageObjectData data, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Storage bucket: {data.Bucket}");
            _logger.LogInformation($"Object being handled: {data.Name}");

            return _handler.HandleAsync(data, cancellationToken);
        }
    }

    public class HandlerFlat
    {
        private readonly ILogger<HandlerFlat> _logger;
        private readonly BigQueryClient _bigQueryClient;
        private readonly StorageClient _storageClient;

        public HandlerFlat(ILogger<HandlerFlat> logger, IProjectIdProvider projectIdProvider) {
            _logger = logger;
            _logger.LogInformation($"Project id : {projectIdProvider.Id}");
            _bigQueryClient = BigQueryClient.Create(projectIdProvider.Id);
            _storageClient = StorageClient.Create(GoogleCredential.GetApplicationDefault());
        }

        public async Task HandleAsync(StorageObjectData data, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Storage bucket: {data.Bucket}");
            _logger.LogInformation($"Object being handled: {data.Name}");   

            var dataset = _bigQueryClient.GetOrCreateDataset("verbs_dataset");
            var table = dataset.GetTableReference("conjugation_flat");
            var count = await _bigQueryClient
                .ExecuteQueryAsync($"SELECT DISTINCT Infinitive FROM `{table.DatasetId}.{table.TableId}`",
                    ArraySegment<BigQueryParameter>.Empty,
                    cancellationToken: cancellationToken).ConfigureAwait(false);
            using (var stream = new MemoryStream())
            {
                await _storageClient.DownloadObjectAsync(data.Bucket, data.Name, stream, cancellationToken:cancellationToken);
                stream.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(stream);
                var str = reader.ReadToEnd();
                var verb = Newtonsoft.Json.JsonConvert.DeserializeObject<Verb>(str);
                var rows = Flatify(verb, (int)count.TotalRows.Value);
                var loadJob = await _bigQueryClient.InsertRowsAsync(table, rows, cancellationToken: cancellationToken);
                loadJob.ThrowOnAnyError();
                _logger.LogInformation($"Object was handled successfully: {data.Name}");
            }
        }

        IEnumerable<BigQueryInsertRow> Flatify(Verb verb, int verbIndex)
        {
            var conjugationIndex = 0;
            foreach (var timeConjugation in verb.TimeConjugations)
            {
                foreach (var conjugation in timeConjugation.Conjugations)
                {

                    var conjugationFlat = new ConjugationFlat()
                    {
                        Id = $"{verb.Infinitive}.{timeConjugation.Time}.{conjugation.Value}",
                        Infinitive = verb.Infinitive,
                        Combined = conjugation.Combined,
                        Female = conjugation.Female,
                        Male = conjugation.Male,
                        Party = conjugation.Party,
                        Time = timeConjugation.Time,
                        Value = conjugation.Value,
                        ConjugationIndex = conjugationIndex++,
                        VerbIndex = verbIndex
                    };
                    yield return new BigQueryInsertRow() {ToDictionary(conjugationFlat)};
                }
            }
        }

        IDictionary<string, object> ToDictionary<T>(T someObject)
        {
            return someObject.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(someObject, null));
        }
    }
    
    public interface IProjectIdProvider
    {
        string Id { get; }
    }

    class ProjectIdProvider : IProjectIdProvider
    {
        public string Id =>  Environment.GetEnvironmentVariable("GCP_PROJECT"); //"parlr-342110"
    }
}
