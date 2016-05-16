using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Conventions.Files
{
    internal abstract class StoragePath
    {
        private readonly CloudBlobContainer _jobOutputContainer;

        public StoragePath(CloudBlobContainer jobOutputContainer)
        {
            Debug.Assert(jobOutputContainer != null);
            _jobOutputContainer = jobOutputContainer;
        }

        public async Task SaveAsync(object kind, string relativePath, CancellationToken cancellationToken = default(CancellationToken))
        {
            var destinationPath = relativePath.Replace('\\', '/');
            await SaveAsync(kind, relativePath, destinationPath, cancellationToken);
        }

        public async Task SaveAsync(object kind, string sourcePath, string destinationRelativePath, CancellationToken cancellationToken = default(CancellationToken))
        {
            var blobName = BlobName(kind, destinationRelativePath);
            var blob = _jobOutputContainer.GetBlockBlobReference(blobName);
            await blob.UploadFromFileAsync(sourcePath, FileMode.Open, cancellationToken);
        }

        public IEnumerable<IListBlobItem> List(object kind)
            => _jobOutputContainer.ListBlobs(BlobNamePrefix(kind), useFlatBlobListing: true);

        public async Task<ICloudBlob> GetBlobAsync(object kind, string filePath, CancellationToken cancellationToken = default(CancellationToken))
            => await _jobOutputContainer.GetBlobReferenceFromServerAsync(BlobName(kind, filePath), cancellationToken);

        internal abstract string BlobNamePrefix(object kind);

        internal string BlobName(object kind, string relativePath)
            => $"{BlobNamePrefix(kind)}{relativePath}";

        internal sealed class JobStoragePath : StoragePath
        {
            internal JobStoragePath(CloudBlobContainer jobOutputContainer)
                : base(jobOutputContainer)
            {
            }

            internal override string BlobNamePrefix(object kind)
                => $"${kind.ToString()}/";
        }

        internal sealed class TaskStoragePath : StoragePath
        {
            private readonly string _taskId;

            internal TaskStoragePath(CloudBlobContainer jobOutputContainer, string taskId)
                : base(jobOutputContainer)
            {
                Debug.Assert(taskId != null);
                _taskId = taskId;
            }

            internal override string BlobNamePrefix(object kind)
                => $"{_taskId}/${kind.ToString()}/";
        }
    }
}
