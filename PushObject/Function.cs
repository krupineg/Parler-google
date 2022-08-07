using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Google.Cloud.BigQuery.V2;
using Google.Cloud.Storage.V1;
using Google.Apis.Bigquery.v2.Data;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PushObject
{
    /// <summary>
    /// A function that can be triggered in responses to changes in Google Cloud Storage.
    /// The type argument (StorageObjectData in this case) determines how the event payload is deserialized.
    /// The function must be deployed so that the trigger matches the expected payload type. (For example,
    /// deploying a function expecting a StorageObject payload will not work for a trigger that provides
    /// a FirestoreEvent.)
    /// </summary>
    public class Function : ICloudEventFunction<StorageObjectData>
    {
        private readonly ILogger<Function> _logger;
        private readonly BigQueryClient _client;        
        private readonly CreateLoadJobOptions _jobOptions;

        public Function(ILogger<Function> logger) {
            _logger = logger;
            var projectId = System.Environment.GetEnvironmentVariable("GCP_PROJECT"); //"parlr-342110"
             _logger.LogInformation($"Project id : {projectId}");
            _client = BigQueryClient.Create(projectId);
            _jobOptions = new CreateLoadJobOptions
            {
                SourceFormat = FileFormat.NewlineDelimitedJson,
                IgnoreUnknownValues = true,
                MaxBadRecords = 100000
            };     
        }

        public async Task HandleAsync(CloudEvent cloudEvent, StorageObjectData data, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"CloudEvent type: {cloudEvent.Type}");
            _logger.LogDebug($"Storage bucket: {data.Bucket}");
            _logger.LogInformation($"Object being handled: {data.Name}");   

            var dataset = _client.GetOrCreateDataset("verbs_dataset");
            var table = dataset.GetTableReference("verbs_table");
            var gcsUri = $"gs://parlr-raw-data-groups/{data.Name}";    
            var loadJob = await _client.CreateLoadJobAsync(gcsUri, table, null, _jobOptions).ConfigureAwait(false);
            loadJob.PollUntilCompleted();
            loadJob.ThrowOnAnyError();
            _logger.LogInformation($"Object was handled successfully: {data.Name}");   
        }
    }
}
