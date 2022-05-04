gcloud functions deploy CounterPubSub \
--source https://source.developers.google.com/projects/parlr-342110/repos/github_krupineg_parler-google/moveable-aliases/master/paths/CounterPubSub \
--trigger-event=google.storage.object.finalize \
--trigger-resource=parlr-raw-data-flat \
--entry-point CounterPubSub.Function \
--runtime=dotnet3 \
--set-env-vars GCP_PROJECT=parlr-342110 \
--max-instances=1;

gcloud functions deploy CounterPubSub2 \
--source https://source.developers.google.com/projects/parlr-342110/repos/github_krupineg_parler-google/moveable-aliases/master/paths/CounterPubSub \
--trigger-event=google.storage.object.finalize \
--trigger-resource=parlr-raw-data-firestore \
--entry-point CounterPubSub.Function \
--runtime=dotnet3 \
--set-env-vars GCP_PROJECT=parlr-342110 \
--max-instances=1;

