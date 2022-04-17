using System;
using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using PushObject.Flat;
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
    internal class FunctionFlat : ICloudEventFunction<MessagePublishedData>
    {
        private readonly ILogger<FunctionFlat> _logger;
        private readonly HandlerFlat _handler;

        public FunctionFlat(
            ILogger<FunctionFlat> logger, 
            ILogger<HandlerFlat> handlerLogger) {
            _logger = logger;
            _handler = new HandlerFlat(handlerLogger, new ProjectIdProvider());
        }

        public async Task HandleAsync(CloudEvent cloudEvent, MessagePublishedData message, CancellationToken cancellationToken)
        {
            try
            {
                var bucket = message.Message.Attributes["data.Bucket"];
                var objectName = message.Message.Attributes["data.Name"];
                var index = int.Parse(message.Message.Attributes["data.Index"]);
                _logger.LogDebug($"Storage bucket: {bucket}");
                _logger.LogInformation($"Object being handled: {bucket} {objectName} {index}");
                await _handler.HandleAsync(bucket, objectName, index, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"this function was interrupted by an error: {e.Message} {e.InnerException?.Message}");
                throw;
            }
        }
    }
}
