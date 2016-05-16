using Microsoft.Azure.Batch.Conventions.Files.Utilities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Conventions.Files
{
    public class JobOutputStorage
    {
        private readonly StoragePath _storagePath;

        public JobOutputStorage(Uri jobOutputContainerUri)
            : this(CloudBlobContainerUtils.GetContainerReference(jobOutputContainerUri))
        {
        }

        public JobOutputStorage(CloudStorageAccount storageAccount, string jobId)
            : this(CloudBlobContainerUtils.GetContainerReference(storageAccount, jobId))
        {
        }

        private JobOutputStorage(CloudBlobContainer jobOutputContainer)
        {
            if (jobOutputContainer == null)
            {
                throw new ArgumentNullException(nameof(jobOutputContainer));
            }

            _storagePath = new StoragePath.JobStoragePath(jobOutputContainer);
        }

        public async Task SaveAsync(JobOutputKind kind, string relativePath, CancellationToken cancellationToken = default(CancellationToken))
            => await _storagePath.SaveAsync(kind, relativePath, cancellationToken);

        public async Task SaveAsync(JobOutputKind kind, string sourcePath, string destinationRelativePath, CancellationToken cancellationToken = default(CancellationToken))
            => await _storagePath.SaveAsync(kind, sourcePath, destinationRelativePath, cancellationToken);

        public IEnumerable<IListBlobItem> ListOutputs(JobOutputKind kind)
            => _storagePath.List(kind);

        public async Task<ICloudBlob> GetOutputAsync(JobOutputKind kind, string filePath, CancellationToken cancellationToken = default(CancellationToken))
            => await _storagePath.GetBlobAsync(kind, filePath, cancellationToken);
    }
}
