gcloud functions deploy CreateSchema \
--source https://source.developers.google.com/projects/parlr-342110/repos/github_krupineg_parler-google/moveable-aliases/main/paths/CreateSchema \
--trigger-http \
--entry-point CreateSchema.Function \
--runtime=dotnet3 \
--allow-unauthenticated;