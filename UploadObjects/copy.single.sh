export GCP_PROJECT=parlr-342110
export GOOGLE_APPLICATION_CREDENTIALS=~/key.json
gsutil -m cp -r gs://parlr-raw-data/prendre.json gs://parlr-raw-data-flat/