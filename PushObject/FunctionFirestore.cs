using System;
using System.Threading;
using System.Threading.Tasks;
using CloudNative.CloudEvents;
using Google.Cloud.Firestore;
using Google.Cloud.Functions.Framework;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using Microsoft.Extensions.Logging;
using PushObject.Flat;
using PushObject.Flat.Local;
using PushObject.Model;

namespace PushObject
{
    internal class FunctionFirestore : ICloudEventFunction<MessagePublishedData>
    {
        private readonly ILogger<FunctionFirestore> _logger;
        private readonly HandlerFlat _handler;

        public FunctionFirestore(
            ILogger<FunctionFirestore> logger, 
            ILogger<HandlerFlat> handlerLogger) {
            _logger = logger;
            _handler = new HandlerFlat(handlerLogger, new FirestorePusher(new ProjectIdProvider()));
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

    internal class FirestorePusher : IPusher
    {
        private readonly IProjectIdProvider _projectIdProvider;

        internal FirestorePusher(IProjectIdProvider projectIdProvider)
        {
            _projectIdProvider = projectIdProvider;
        }
        
        public async Task PushAsync(Verb verb, long verbIndex, CancellationToken cancellationToken)
        {
            var db = FirestoreDb.Create(_projectIdProvider.Id);
            // [START fs_add_simple_doc_as_entity]
            
            var docRef = db.Collection("verbs").Document(verb.Infinitive);
            await docRef.SetAsync(verb).ConfigureAwait(false);
            // [END fs_add_simple_doc_as_entity]
            Console.WriteLine("Added custom City object to the cities collection.");
        }
    }
}