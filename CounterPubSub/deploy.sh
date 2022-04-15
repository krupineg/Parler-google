gcloud functions deploy CounterPubSub \
--source https://source.developers.google.com/projects/parlr-342110/repos/github_krupineg_parler-google/moveable-aliases/master/paths/CreateSchema \
--trigger-topic parlr-increment \
--entry-point CounterPubSub.Function \
--runtime=dotnet3 \
--set-env-vars GCP_PROJECT=parlr-342110 \
--max-instances=1 \
--allow-unauthenticated;