export GCP_PROJECT=parlr-342110
export GOOGLE_APPLICATION_CREDENTIALS=~/key.json
gsutil cp -r ../../Parler-data/jsons/* gs://parlr-raw-data/