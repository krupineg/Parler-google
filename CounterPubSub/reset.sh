gcloud functions call CounterPubSub --data '{"topic":"parlr-increment", "attributes": { "reset":"0" }}'