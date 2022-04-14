gcloud functions deploy CreateSchema \
--source https://source.developers.google.com/projects/parlr-342110/repos/github_krupineg_parler-google/moveable-aliases/master/paths/CreateSchema \
--trigger-http \
--entry-point CreateSchema.Function \
--runtime=dotnet3 \
--set-env-vars GCP_PROJECT=parlr-342110 \
--allow-unauthenticated;

curl https://us-central1-parlr-342110.cloudfunctions.net/CreateSchema