gcloud pubsub topics delete parlr-increment;
gcloud pubsub topics delete parlr-increment-response;
gcloud pubsub topics create parlr-increment;
gcloud pubsub topics create parlr-increment-response;

gcloud functions deploy CounterPubSub \
--source https://source.developers.google.com/projects/parlr-342110/repos/github_krupineg_parler-google/moveable-aliases/master/paths/CounterPubSub \
--trigger-topic parlr-increment \
--entry-point CounterPubSub.Function \
--runtime=dotnet3 \
--set-env-vars GCP_PROJECT=parlr-342110 \
--max-instances=1;