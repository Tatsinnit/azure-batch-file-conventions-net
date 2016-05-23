using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Conventions.Files
{
    public static class CloudJobExtensions
    {
        public static JobOutputStorage OutputStorage(this CloudJob job, CloudStorageAccount storageAccount)
        {
            if (job == null)
            {
                throw new ArgumentNullException(nameof(job));
            }
            if (storageAccount == null)
            {
                throw new ArgumentNullException(nameof(storageAccount));
            }

            return new JobOutputStorage(storageAccount, job.Id);
        }
    }
}
