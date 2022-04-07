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
        private readonly BigQueryClient _bigQueryClient;
        private readonly StorageClient _storageClient;

        public FunctionFlat(ILogger<FunctionFlat> logger) {
            _logger = logger;
            var projectId = System.Environment.GetEnvironmentVariable("GCP_PROJECT"); //"parlr-342110"
             _logger.LogInformation($"Project id : {projectId}");
            _bigQueryClient = BigQueryClient.Create(projectId);
            _storageClient = StorageClient.Create(GoogleCredential.GetApplicationDefault());
        }

        public async Task HandleAsync(CloudEvent cloudEvent, StorageObjectData data, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"CloudEvent type: {cloudEvent.Type}");
            _logger.LogDebug($"Storage bucket: {data.Bucket}");
            _logger.LogInformation($"Object being handled: {data.Name}");   

            var dataset = _bigQueryClient.GetOrCreateDataset("verbs_dataset");
            var table = dataset.GetTableReference("conjugation_flat");
            var gcsUri = $"gs://parlr-raw-data/{data.Name}";
            using (var stream = new MemoryStream())
            {
                await _storageClient.DownloadObjectAsync(data.Bucket, data.Name, stream, cancellationToken:cancellationToken);
                using var reader = new StreamReader(stream);
                using var jsonReader = new JsonTextReader(reader);
                var serializer = new Newtonsoft.Json.JsonSerializer();
                var verb = serializer.Deserialize<Verb>(jsonReader);
                foreach (var timeConjugation in verb.TimeConjugations)
                {
                    var conjugationIndex = 0;
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
                            ConjugationIndex = conjugationIndex++
                        };

                       var loadJob = await _bigQueryClient.InsertRowAsync(
                           table,
                           new BigQueryInsertRow() {ToDictionary(conjugationFlat)},
                           cancellationToken: cancellationToken);
                       loadJob.ThrowOnAnyError();
                       
                       _logger.LogInformation($"Object was handled successfully: {data.Name}");  
                    }   
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
}
