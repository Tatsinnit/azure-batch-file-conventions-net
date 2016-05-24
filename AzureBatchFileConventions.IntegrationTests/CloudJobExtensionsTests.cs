using Microsoft.Azure.Batch.Conventions.Files.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Azure.Batch.Conventions.Files.IntegrationTests
{
    public class CloudJobExtensionsTests : IntegrationTest, IClassFixture<CloudJobExtensionsTests.JobIdFixtureImpl>
    {
        private readonly string _jobId;

        public CloudJobExtensionsTests(JobIdFixtureImpl jobIdFixture, ITestOutputHelper output)
            : base(jobIdFixture, output)
        {
            _jobId = jobIdFixture.JobId;
        }

        public class JobIdFixtureImpl : JobIdFixture
        {
            protected override string TestId { get; } = "cloudjobext";
        }

        [Fact]
        public async Task CloudJobOutputStorageExtensionSavesToCorrectContainer()
        {
            using (var batchClient = await BatchClient.OpenAsync(new FakeBatchServiceClient()))
            {
                var job = batchClient.JobOperations.CreateJob(_jobId, null);

                await job.OutputStorage(StorageAccount).SaveAsync(JobOutputKind.JobOutput, FilePath("TestText1.txt"));

                var blobs = job.OutputStorage(StorageAccount).ListOutputs(JobOutputKind.JobOutput).ToList();
                Assert.NotEqual(0, blobs.Count);
                Assert.Contains(blobs, b => b.Uri.AbsoluteUri.EndsWith($"{_jobId}/$JobOutput/Files/TestText1.txt"));
            }
        }
    }
}
