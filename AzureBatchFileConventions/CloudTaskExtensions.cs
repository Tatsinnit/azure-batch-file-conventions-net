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
            // Workaround for CloudTask not knowing its parent job ID - TODO: if we have to keep
            // this then refactor the URL part extraction code, but want to review idea of adding
            // JobId to CloudTask as a cleaner option.

            if (task.Url == null)
            {
                throw new ArgumentException("Task Url property must be populated", nameof(task));
            }

            // URL is of the form "acct/jobs/jobId/tasks/taskId"

            var url = task.Url;
            var separatorIndex_tasks_taskId = url.LastIndexOf('/');
            var separatorIndex_jobId_tasks = url.LastIndexOf('/', separatorIndex_tasks_taskId - 1);
            var separatorIndex_job_jobId = url.LastIndexOf('/', separatorIndex_jobId_tasks - 1);

            var jobIdLength = separatorIndex_jobId_tasks - separatorIndex_job_jobId - 1;

            return url.Substring(separatorIndex_job_jobId + 1, jobIdLength);
        }
    }
}
