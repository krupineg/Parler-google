## gcloud functions deploy PushObject \
## --source https://source.developers.google.com/projects/parlr-342110/repos/github_krupineg_parler-google/moveable-aliases/master/paths/PushObject \
## --trigger-event=google.storage.object.finalize \
## --trigger-resource=parlr-raw-data \
## --entry-point PushObject.Function \
## --runtime=dotnet3 \
## --set-env-vars GCP_PROJECT=parlr-342110 \
## --retry \
## --allow-unauthenticated;
gcloud functions delete PushObjectFirestore;
gcloud functions deploy PushObjectFlat \
--source https://source.developers.google.com/projects/parlr-342110/repos/github_krupineg_parler-google/moveable-aliases/master/paths/PushObject \
--trigger-topic parlr-increment \
--entry-point PushObject.FunctionFlat \
--runtime=dotnet3 \
--set-env-vars GCP_PROJECT=parlr-342110 \
--max-instances=1;