gcloud functions delete PushObjectFlat;

gcloud functions deploy PushObjectFirestore \
--source https://source.developers.google.com/projects/parlr-342110/repos/github_krupineg_parler-google/moveable-aliases/master/paths/PushObject \
--trigger-topic parlr-increment \
--entry-point PushObject.FunctionFirestore \
--runtime=dotnet3 \
--set-env-vars GCP_PROJECT=parlr-342110 \
--max-instances=1;