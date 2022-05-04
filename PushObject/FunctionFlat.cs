using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CloudNative.CloudEvents;
using Google.Cloud.Functions.Framework;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using Google.Cloud.BigQuery.V2;
using Google.Events.Protobuf.Cloud.PubSub.V1;
using PushObject.Flat;
using PushObject.Flat.Local;
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
    internal class FunctionFlat : ICloudEventFunction<MessagePublishedData>
    {
        private readonly ILogger<FunctionFlat> _logger;
        private readonly HandlerFlat _handler;

        public FunctionFlat(
            ILogger<FunctionFlat> logger, 
            ILogger<HandlerFlat> handlerLogger) {
            _logger = logger;
            _handler = new HandlerFlat(handlerLogger, new BigQueryPusher(new ProjectIdProvider()));
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

    internal class BigQueryPusher : IPusher
    {
        private readonly BigQueryClient _bigQueryClient;

        internal BigQueryPusher(IProjectIdProvider projectIdProvider)
        {
            _bigQueryClient = BigQueryClient.Create(projectIdProvider.Id);
        }
        
        public async Task PushAsync(Verb verb, long verbIndex, CancellationToken cancellationToken)
        {  
            var dataset = _bigQueryClient.GetOrCreateDataset("verbs_dataset");
            var table = dataset.GetTableReference("conjugation_flat");
            var rows = Flatify(verb, verbIndex);
            var loadJob = await _bigQueryClient.InsertRowsAsync(table, rows, cancellationToken: cancellationToken);
            loadJob.ThrowOnAnyError();
        }
        
        private IEnumerable<BigQueryInsertRow> Flatify(Verb verb, long verbIndex)
        {
            var conjugationIndex = 0;
            foreach (var timeConjugation in verb.TimeConjugations)
            foreach (var conjugation in timeConjugation.Conjugations)
            {
                var conjugationFlat = new ConjugationFlat
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
                yield return new BigQueryInsertRow {ToDictionary(conjugationFlat)};
            }
        }

        private IDictionary<string, object> ToDictionary<T>(T someObject)
        {
            return someObject.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToDictionary(prop => prop.Name, prop => prop.GetValue(someObject, null));
        }
    }
}
