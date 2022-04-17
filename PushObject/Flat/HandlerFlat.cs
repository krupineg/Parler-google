using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.BigQuery.V2;
using Google.Cloud.Storage.V1;
using Google.Events.Protobuf.Cloud.Storage.V1;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PushObject.Model;
using PushObject.Model.V2;

namespace PushObject.Flat
{
    public class HandlerFlat
    {
        private readonly BigQueryClient _bigQueryClient;
        private readonly ILogger<HandlerFlat> _logger;
        private readonly StorageClient _storageClient;

        public HandlerFlat(ILogger<HandlerFlat> logger, IProjectIdProvider projectIdProvider)
        {
            _logger = logger;
            _logger.LogInformation($"Project id : {projectIdProvider.Id}");
            _bigQueryClient = BigQueryClient.Create(projectIdProvider.Id);
            _storageClient = StorageClient.Create(GoogleCredential.GetApplicationDefault());
        }

        public async Task HandleAsync(string bucket, string name, long verbIndex, CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Storage bucket: {bucket}");
            _logger.LogInformation($"Object being handled: {name}");

            var dataset = _bigQueryClient.GetOrCreateDataset("verbs_dataset");
            var table = dataset.GetTableReference("conjugation_flat");

            await using (var stream = new MemoryStream())
            {
                await _storageClient.DownloadObjectAsync(bucket, name, stream,
                    cancellationToken: cancellationToken);
                stream.Seek(0, SeekOrigin.Begin);
                using var reader = new StreamReader(stream);
                var str = reader.ReadToEnd();
                var verb = JsonConvert.DeserializeObject<Verb>(str);
                var rows = Flatify(verb, verbIndex);
                var loadJob = await _bigQueryClient.InsertRowsAsync(table, rows, cancellationToken: cancellationToken);
                loadJob.ThrowOnAnyError();
                _logger.LogInformation($"Object was handled successfully: {name}");
            }
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