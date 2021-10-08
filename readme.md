1. Deploy this Azure Function to a function app, the config settings on the app required is:

- `STORAGE_CONNECTION` with the value as the connection string for the storage account to trigger on uploads
- `SCAN_SERVER_IP` with the IP address of the Container Instance to send the scans to. Public or private if inside the same VNET

The scan container name is hardcoded as `clamcontainer`

1. Deploy a Container Instance from this container https://hub.docker.com/r/tiredofit/clamav

For large files the following Container Instance environment variables are required:

- MAX_SCAN_SIZE
- STREAM_MAX_LENGTH
- MAX_FILE_SIZE

For 2GB set the values to
2000M
