using System;
using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Google.Apis.Bigquery.v2.Data;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.Functions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PushObject.Flat;
using PushObject.Flat.External;
using PushObject.Flat.Local;

namespace PushObject
{
    /// <summary>
    /// A function that can be triggered in responses to changes in Google Cloud Storage.
    /// The type argument (StorageObjectData in this case) determines how the event payload is deserialized.
    /// The function must be deployed so that the trigger matches the expected payload type. (For example,
    /// deploying a function expecting a StorageObject payload will not work for a trigger that provides
    /// a FirestoreEvent.)
    /// </summary>
    internal class FunctionFlat : ICloudEventFunction<StorageObjectData>
    {
        private readonly ILogger<FunctionFlat> _logger;
        private readonly IIndexRepository _indexRepository;
        private readonly HandlerFlat _handler;

        public FunctionFlat(
            ILogger<FunctionFlat> logger, 
            ILogger<HandlerFlat> handlerLogger, 
            ILogger<IndexRepository> repositoryLogger) {
            _logger = logger;
            _indexRepository = new IndexRepository(new ProjectIdProvider(), repositoryLogger);
            _handler = new HandlerFlat(handlerLogger, new ProjectIdProvider());
        }

        public async Task HandleAsync(CloudEvent cloudEvent, StorageObjectData data, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug($"Storage bucket: {data.Bucket}");
                _logger.LogInformation($"Object being handled: {data.Name}");
                var index = await _indexRepository.ObtainIndex();
                await _handler.HandleAsync(data, index, cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "this function was interrupted by an error");
                throw;
            }
        }
    }
}
