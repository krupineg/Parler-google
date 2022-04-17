gcloud pubsub subscriptions delete parlr-increment-sub
gcloud pubsub topics delete parlr-increment;
gcloud pubsub subscriptions delete parlr-increment-response-sub
gcloud pubsub topics delete parlr-increment-response;
gcloud pubsub topics create parlr-increment;
gcloud pubsub subscriptions create parlr-increment-sub --topic=parlr-increment --expiration-period=10m;
gcloud pubsub topics create parlr-increment-response;
gcloud pubsub subscriptions create parlr-increment-response-sub --topic=parlr-increment-response --expiration-period=10m;