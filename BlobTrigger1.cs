using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using nClam;
using System.Linq;
using System.Text;
using Azure.Storage.Blobs;
using System.Threading.Tasks;

namespace ClamAvScan
{
    public static class BlobTrigger1
    {
        static readonly int serverPort = 3310;

        [FunctionName("BlobTrigger1")]
        public static async Task RunAsync([BlobTrigger("clamcontainer/{name}", Connection = "STORAGE_CONNECTION")] Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var serverName = Environment.GetEnvironmentVariable("SCAN_SERVER_IP");

            // Create client
            var clam = new ClamClient(serverName, serverPort)
            {
                MaxStreamSize = 21474836480,
                MaxChunkSize = 26214400
            };

            // Scanning for viruses...
            var scanResult = await clam.SendAndScanFileAsync(myBlob);

            switch (scanResult.Result)
            {
                case ClamScanResults.Clean:
                    log.LogInformation("The file is clean!");
                    break;
                case ClamScanResults.VirusDetected:
                    {
                        log.LogInformation("Virus Found!");
                        log.LogInformation("Virus name: {0}", scanResult.InfectedFiles.First().VirusName);
                        await ReplaceBlobAsync(name);
                        break;
                    }
                case ClamScanResults.Error:
                    log.LogInformation("Error scanning file: {0}", scanResult.RawResult);
                    break;
            }
        }

        private static async Task ReplaceBlobAsync(string blobName)
        {
            var connectionString = Environment.GetEnvironmentVariable("STORAGE_CONNECTION");
            var srcContainer = new BlobContainerClient(connectionString, "clamcontainer");

            var srcBlob = srcContainer.GetBlobClient(blobName);

            await srcBlob.UploadAsync(GenerateStream("This blob was found to contain malware and has been removed."), true);
        }

        private static Stream GenerateStream(string value)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(value));
        }
    }
}
