using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FunctionApp2
{
    public static class Function1
    {
        // read and copy files from blob to other
        [FunctionName("BlobSample")]
        public static void Run([BlobTrigger("testcontainer/{name}", Connection = "AzureWebJobsStorage")]
                                Stream myBlob,
                                string name,[Blob("eventcontainer/{name}", FileAccess.Write, Connection = "AzureWebJobsStorage")]
                                Stream outBlob,
                                ILogger log)
        {

            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            for(int i=0; i<10;i++)
            {
                myBlob.CopyTo(outBlob);            }
           
        }

        private static void WriteOutputName(string blobPath, string toAdd)
        {
            CloudStorageAccount account = CloudStorageAccount.DevelopmentStorageAccount;
            CloudBlobClient blobClient = account.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("names-out");

            CloudBlockBlob cloudBlockBlob = container.GetBlockBlobReference(blobPath);
            cloudBlockBlob.UploadTextAsync(toAdd);
        }

    }
}
