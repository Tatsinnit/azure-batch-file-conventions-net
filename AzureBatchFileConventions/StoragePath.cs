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
            if (kind == null)
            {
                throw new ArgumentNullException(nameof(kind));
            }
            if (relativePath == null)
            {
                throw new ArgumentNullException(nameof(relativePath));
            }
            if (relativePath.Length == 0)
            {
                throw new ArgumentException($"{nameof(relativePath)} must not be empty", nameof(relativePath));
            }
            if (Path.IsPathRooted(relativePath))
            {
                throw new ArgumentException($"{nameof(relativePath)} must not be a relative path", nameof(relativePath));
            }

            var destinationPath = relativePath.Replace('\\', '/');
            await SaveAsync(kind, relativePath, destinationPath, cancellationToken);
        }

        public async Task SaveAsync(object kind, string sourcePath, string destinationRelativePath, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (kind == null)
            {
                throw new ArgumentNullException(nameof(kind));
            }
            if (sourcePath == null)
            {
                throw new ArgumentNullException(nameof(sourcePath));
            }
            if (sourcePath.Length == 0)
            {
                throw new ArgumentException($"{nameof(sourcePath)} must not be empty", nameof(sourcePath));
            }
            if (destinationRelativePath == null)
            {
                throw new ArgumentNullException(nameof(destinationRelativePath));
            }
            if (destinationRelativePath.Length == 0)
            {
                throw new ArgumentException($"{nameof(destinationRelativePath)} must not be empty", nameof(destinationRelativePath));
            }

            var blobName = BlobName(kind, destinationRelativePath);
            var blob = _jobOutputContainer.GetBlockBlobReference(blobName);
            await blob.UploadFromFileAsync(sourcePath, FileMode.Open, cancellationToken);
        }

        public IEnumerable<IListBlobItem> List(object kind)
        {
            if (kind == null)
            {
                throw new ArgumentNullException(nameof(kind));
            }

            return _jobOutputContainer.ListBlobs(BlobNamePrefix(kind), useFlatBlobListing: true);
        }

        public async Task<ICloudBlob> GetBlobAsync(object kind, string filePath, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (kind == null)
            {
                throw new ArgumentNullException(nameof(kind));
            }
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }
            if (filePath.Length == 0)
            {
                throw new ArgumentException($"{nameof(filePath)} must not be empty", nameof(filePath));
            }

            return await _jobOutputContainer.GetBlobReferenceFromServerAsync(BlobName(kind, filePath), cancellationToken);
        }

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
