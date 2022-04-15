gcloud pubsub subscriptions delete parlr-increment-sub
gcloud pubsub topics delete parlr-increment;
gcloud pubsub subscriptions delete parlr-increment-response-sub
gcloud pubsub topics delete parlr-increment-response;
gcloud pubsub topics create parlr-increment;
gcloud pubsub subscriptions create parlr-increment-sub --topic=parlr-increment --expiration-period=10m;
gcloud pubsub topics create parlr-increment-response;
gcloud pubsub subscriptions create parlr-increment-response-sub --topic=parlr-increment-response --expiration-period=10m;

gcloud functions deploy CounterPubSub \
--source https://source.developers.google.com/projects/parlr-342110/repos/github_krupineg_parler-google/moveable-aliases/master/paths/CounterPubSub \
--trigger-topic parlr-increment \
--entry-point CounterPubSub.Function \
--runtime=dotnet3 \
--set-env-vars GCP_PROJECT=parlr-342110 \
--max-instances=1;