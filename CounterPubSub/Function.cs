using System;
using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;

namespace CounterPubSub
{
    /// <summary>
    /// A function that can be triggered in responses to changes in Google Cloud Storage.
    /// The type argument (StorageObjectData in this case) determines how the event payload is deserialized.
    /// The function must be deployed so that the trigger matches the expected payload type. (For example,
    /// deploying a function expecting a StorageObject payload will not work for a trigger that provides
    /// a FirestoreEvent.)
    /// </summary>
    public class Function :ICloudEventFunction<StorageObjectData>
    {
        private static long _counter = -1;
        private readonly ILogger _logger;

        public Function(ILogger<Function> logger) {
            _logger = logger;
        }
        
        /// <summary>
        /// Logic for your function goes here. Note that a CloudEvent function just consumes an event;
        /// it doesn't provide any response.
        /// </summary>
        /// <param name="cloudEvent">The CloudEvent your function should consume.</param>
        /// <param name="data">The deserialized data within the CloudEvent.</param>
        /// <param name="cancellationToken">A cancellation token that is notified if the request is aborted.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task HandleAsync(CloudEvent cloudEvent, StorageObjectData data, CancellationToken cancellationToken)
        {
            var projectId = Environment.GetEnvironmentVariable("GCP_PROJECT");
            _logger.LogInformation("increment the counter");
            var bucket = data.Bucket;
            var objectName = data.Name;
            var topicName = TopicName.FromProjectTopic(projectId, "parlr-increment");
            var value = Interlocked.Increment(ref _counter);
            var publisher = await PublisherClient.CreateAsync(topicName).ConfigureAwait(false);
            var message = new PubsubMessage()
            {
                Attributes = {{"data.Bucket", bucket}, {"data.Name", objectName}, { "data.Index", value.ToString()}}
            };
            await publisher.PublishAsync(message).ConfigureAwait(false);
            await publisher.ShutdownAsync(cancellationToken).ConfigureAwait(false);
                
            _logger.LogInformation($"response is sent: {bucket}/{objectName}, index: {value}");
        }
    }
}
