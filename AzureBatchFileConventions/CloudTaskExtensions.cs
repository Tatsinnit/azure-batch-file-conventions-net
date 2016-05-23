using Microsoft.Azure.Batch.Conventions.Files.Utilities;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Conventions.Files
{
    public static class CloudTaskExtensions
    {
        public static TaskOutputStorage OutputStorage(this CloudTask task, CloudStorageAccount storageAccount)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }
            if (storageAccount == null)
            {
                throw new ArgumentNullException(nameof(storageAccount));
            }

            return new TaskOutputStorage(storageAccount, task.JobId(), task.Id);
        }

        private static string JobId(this CloudTask task)
        {
            // Workaround for CloudTask not knowing its parent job ID.

            if (task.Url == null)
            {
                throw new ArgumentException("Task Url property must be populated", nameof(task));
            }

            var jobId = UrlUtils.GetUrlValueSegment(task.Url, "jobs");

            if (jobId == null)
            {
                throw new ArgumentException($"Task URL is malformed: unable to obtain job ID from URL '{task.Url}'", nameof(task));
            }

            return jobId;
        }
    }
}
