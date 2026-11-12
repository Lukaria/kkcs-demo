1.1 
  curl -v -X POST \
  http://localhost:8080/realms/kkcs-demo/protocol/openid-connect/token \
  -u kkcs-server:jMV605SRfmeG92cZFg8XOQyRCgogAAJE \
  -d grant_type=client_credentials \
  -d scope=kkcs-api