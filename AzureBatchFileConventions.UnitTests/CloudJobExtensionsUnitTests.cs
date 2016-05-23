using Microsoft.Azure.Batch.Conventions.Files.UnitTests.Utilities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Batch.Conventions.Files.UnitTests
{
    public class CloudJobExtensionsUnitTests
    {
        [Fact]
        public void CannotCreateOutputStorageForNullJob()
        {
            CloudJob job = null;
            CloudStorageAccount storageAccount = new CloudStorageAccount(new StorageCredentials("fake", new byte[] { 65, 66, 67, 68 }), true);
            var ex = Assert.Throws<ArgumentNullException>(() => job.OutputStorage(storageAccount));
            Assert.Equal("job", ex.ParamName);
        }

        [Fact]
        public async Task CannotCreateOutputStorageForNullStorageAccount()
        {
            using (var batchClient = await BatchClient.OpenAsync(new FakeBatchServiceClient()))
            {
                CloudJob job = batchClient.JobOperations.CreateJob();
                job.Id = "fakejob";
                CloudStorageAccount storageAccount = null;
                var ex = Assert.Throws<ArgumentNullException>(() => job.OutputStorage(storageAccount));
                Assert.Equal("storageAccount", ex.ParamName);
            }
        }
    }
}
