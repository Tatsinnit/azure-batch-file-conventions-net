using Microsoft.Azure.Batch.Conventions.Files.Utilities;
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

        public async Task SaveAsync(object kind, DirectoryInfo baseFolder, string relativePath, CancellationToken cancellationToken = default(CancellationToken))
        {
            Debug.Assert(baseFolder != null);

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

            string sourcePath = Path.Combine(baseFolder.FullName, relativePath);
            string destinationPath = GetDestinationBlobPath(relativePath);

            await SaveAsync(kind, sourcePath, destinationPath, cancellationToken);
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

        public async Task<TrackedFile> SaveTrackedAsync(object kind, string relativePath, TimeSpan flushInterval)
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

            var destinationPath = GetDestinationBlobPath(relativePath);
            return await SaveTrackedAsync(kind, relativePath, destinationPath, flushInterval);
        }

        public async Task<TrackedFile> SaveTrackedAsync(object kind, string sourcePath, string destinationRelativePath, TimeSpan flushInterval)
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
            var blob = _jobOutputContainer.GetAppendBlobReference(blobName);
            await blob.EnsureExistsAsync();
            return new TrackedFile(sourcePath, blob, flushInterval);
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

        private static string GetDestinationBlobPath(string relativeSourcePath)
        {
            const string up = "../";

            var destinationPath = relativeSourcePath.Replace('\\', '/');

            // If we are given a path that traverses up from the working directory,
            // treat it as though it were rooted at the working directory for blob naming
            // purposes. This is intended to support files such as ..\stdout.txt, which
            // is stored above the task working directory.
            //
            // A user can intentionally try to defeat this simple flattening by using a path
            // such as "temp\..\..\stdout.txt" - this may result in the file being
            // stored in the 'wrong' part of the job container, but they can't write
            // outside the job container this way, so the only damage they can do is
            // to themselves.
            while (destinationPath.StartsWith(up))
            {
                destinationPath = relativeSourcePath.Substring(up.Length);
            }

            return destinationPath;
        }

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
