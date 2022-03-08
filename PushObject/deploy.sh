gcloud functions deploy PushObject \
--source https://source.developers.google.com/projects/parlr-342110/repos/github_krupineg_parler-google/moveable-aliases/master/paths/PushObject \
--trigger-event=google.storage.object.finalize \
--trigger-resource=parlr-raw-data \
--entry-point PushObject.Function \
--runtime=dotnet3 \
--set-env-vars GCP_PROJECT=parlr-342110 \
--allow-unauthenticated;