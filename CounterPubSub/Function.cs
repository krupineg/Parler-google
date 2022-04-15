using System;
using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.PubSub.V1;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CounterPubSub
{
    /// <summary>
    /// A function that can be triggered in responses to changes in Google Cloud Storage.
    /// The type argument (StorageObjectData in this case) determines how the event payload is deserialized.
    /// The function must be deployed so that the trigger matches the expected payload type. (For example,
    /// deploying a function expecting a StorageObject payload will not work for a trigger that provides
    /// a FirestoreEvent.)
    /// </summary>
    public class Function : ICloudEventFunction<MessagePublishedData>
    {
        private static long _counter;
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
        public async Task HandleAsync(CloudEvent cloudEvent, MessagePublishedData data, CancellationToken cancellationToken)
        {
            if (data.Message.Attributes.ContainsKey("reset"))
            {
                var value = long.TryParse(data.Message.Attributes["reset"], out long val) ? val : 0L;
                _logger.LogInformation($"try to reset the counter to {value}");
                while (Interlocked.Exchange(ref _counter, value) != value)
                {
                    _logger.LogInformation("failed to reset the counter");
                }
                _logger.LogInformation($"counter was reset to {value}");
            }
            else
            {
                var projectId = Environment.GetEnvironmentVariable("GCP_PROJECT");
                _logger.LogInformation("increment the counter");
                var origin = data.Message.Attributes["origin"];
                var topicName = TopicName.FromProjectTopic(projectId, "parlr-increment-response");
                var publisher = await PublisherClient.CreateAsync(topicName);
                
                var value = Interlocked.Increment(ref _counter);
                
                var message = new Google.Cloud.PubSub.V1.PubsubMessage()
                {
                    Attributes = {{"origin", origin}, {"value", value.ToString()}}
                };
                await publisher.PublishAsync(message);
                await publisher.ShutdownAsync(cancellationToken);
                
                _logger.LogInformation($"response is sent back to the origin: {origin}, {value}");
            }
        }
    }
}
