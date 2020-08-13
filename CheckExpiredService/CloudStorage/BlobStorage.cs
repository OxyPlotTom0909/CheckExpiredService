using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace CheckExpiredService.CloudStorage
{
    public class BlobStorage
    {
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;

        public BlobStorage(string connectionString)
        {
            var strageAccount = CloudStorageAccount.Parse(connectionString);
            _blobClient = strageAccount.CreateCloudBlobClient();
        }

        public async Task InitializeAsync(string containerName)
        {
            _blobContainer = _blobClient.GetContainerReference(containerName);
            await _blobContainer.CreateIfNotExistsAsync();
            await _blobContainer.SetPermissionsAsync(
                new BlobContainerPermissions { PublicAccess = BlobContainerPublicAccessType.Blob });
        }

        public static async Task<BlobStorage> CreateAsync(string connectionString, string containerName)
        {
            var instance = new BlobStorage(connectionString);
            await instance.InitializeAsync(containerName);
            return instance;
        }

        public async Task<Uri> UploadFromStreamAsync(Stream stream, string directoryName, string blobName)
        {
            var directory = _blobContainer.GetDirectoryReference(directoryName);
            var blob = directory.GetBlockBlobReference(blobName);
            await blob.UploadFromStreamAsync(stream);
            return blob.Uri;
        }

        public async Task DeleteImageAsync(string directoryName, string blobName)
        {
            var directory = _blobContainer.GetDirectoryReference(directoryName);
            var blob = directory.GetBlockBlobReference(blobName);
            await blob.DeleteIfExistsAsync();
        }

        public async Task DeleteDictionaryAsync(string directoryName)
        {
            var directory = _blobContainer.GetDirectoryReference(directoryName);
            foreach (CloudBlob blob in (await directory.ListBlobsSegmentedAsync(new BlobContinuationToken())).Results)
            {
                await blob.DeleteIfExistsAsync();
            }
        }

        public async Task<IEnumerable<Uri>> ListBlobUri(string directoryName)
        {
            var directory = _blobContainer.GetDirectoryReference(directoryName);
            return (await directory.ListBlobsSegmentedAsync(new BlobContinuationToken())).Results.Select(blob => blob.Uri);
        }

        public Uri GetContainerUri(string directoryName, string blobName)
        {
            var directory = _blobContainer.GetDirectoryReference(directoryName);
            var blob = directory.GetBlobReference(blobName);
            return blob.Uri;
        }

        public Uri GetDirectoryUri(string directoryName)
        {
            var directory = _blobContainer.GetDirectoryReference(directoryName);
            return directory.Uri;
        }

        public async Task<string> TextFromUriFile(string directoryName, string blobName)
        {
            var path = GetContainerUri(directoryName, blobName);

            string text = null;

            var req = WebRequest.Create(path);
            var response = req.GetResponse();

            Stream stream = response.GetResponseStream();
            StreamReader file = new StreamReader(stream, System.Text.Encoding.UTF8);

            while (!file.EndOfStream)
            {
                var str = await file.ReadLineAsync();
                text += (str + "\n");
            }

            file.Close();

            return text;
        }
    }
}
