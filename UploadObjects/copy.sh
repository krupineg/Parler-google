export GCP_PROJECT=parlr-342110
export GOOGLE_APPLICATION_CREDENTIALS=~/key.json
gsutil cp -r gs://parlr-raw-data/* gs://parlr-raw-data-flat/