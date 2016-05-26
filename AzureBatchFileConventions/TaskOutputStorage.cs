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
    public class TaskOutputStorage
    {
        private readonly StoragePath _storagePath;

        public TaskOutputStorage(Uri jobOutputContainerUri, string taskId)
            : this(CloudBlobContainerUtils.GetContainerReference(jobOutputContainerUri), taskId)
        {
        }

        public TaskOutputStorage(CloudStorageAccount storageAccount, string jobId, string taskId)
            : this(CloudBlobContainerUtils.GetContainerReference(storageAccount, jobId), taskId)
        {
        }

        private TaskOutputStorage(CloudBlobContainer jobOutputContainer, string taskId)
        {
            if (jobOutputContainer == null)
            {
                throw new ArgumentNullException(nameof(jobOutputContainer));
            }
            if (taskId == null)
            {
                throw new ArgumentNullException(nameof(taskId));
            }
            if (taskId.Length == 0)
            {
                throw new ArgumentException("taskId must not be empty", nameof(taskId));
            }

            _storagePath = new StoragePath.TaskStoragePath(jobOutputContainer, taskId);
        }

        public async Task SaveAsync(TaskOutputKind kind, string relativePath, CancellationToken cancellationToken = default(CancellationToken))
            => await SaveAsyncImpl(kind, new DirectoryInfo(Environment.CurrentDirectory), relativePath, cancellationToken);

        internal async Task SaveAsyncImpl(TaskOutputKind kind, DirectoryInfo baseFolder, string relativePath, CancellationToken cancellationToken = default(CancellationToken))
            => await _storagePath.SaveAsync(kind, baseFolder, relativePath, cancellationToken);

        public async Task SaveAsync(TaskOutputKind kind, string sourcePath, string destinationRelativePath, CancellationToken cancellationToken = default(CancellationToken))
            => await _storagePath.SaveAsync(kind, sourcePath, destinationRelativePath, cancellationToken);

        public IEnumerable<IListBlobItem> ListOutputs(TaskOutputKind kind)
            => _storagePath.List(kind);

        public async Task<ICloudBlob> GetOutputAsync(TaskOutputKind kind, string filePath, CancellationToken cancellationToken = default(CancellationToken))
            => await _storagePath.GetBlobAsync(kind, filePath, cancellationToken);

        public async Task<IDisposable> SaveTrackedAsync(string relativePath)
            => await _storagePath.SaveTrackedAsync(TaskOutputKind.TaskLog, relativePath, TrackedFile.DefaultFlushInterval);

        public async Task<IDisposable> SaveTrackedAsync(TaskOutputKind kind, string sourcePath, string destinationRelativePath, TimeSpan flushInterval)
            => await _storagePath.SaveTrackedAsync(kind, sourcePath, destinationRelativePath, flushInterval);
    }
}
