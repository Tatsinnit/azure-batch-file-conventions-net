using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Conventions.Files
{
    /// <summary>
    /// Provides methods for working with the outputs of a <see cref="CloudJob"/>.
    /// </summary>
    public static class CloudJobExtensions
    {
        /// <summary>
        /// Gets the <see cref="JobOutputStorage"/> for a specified <see cref="CloudJob"/>.
        /// </summary>
        /// <param name="job">The job for which to get output storage.</param>
        /// <param name="storageAccount">The storage account linked to the Azure Batch account.</param>
        /// <returns>A JobOutputStorage for the specified job.</returns>
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
