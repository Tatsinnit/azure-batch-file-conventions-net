using Microsoft.Azure.Batch.Conventions.Files.IntegrationTests.Utilities;
using Microsoft.Azure.Batch.Conventions.Files.Utilities;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

[assembly: TestFramework("Microsoft.Azure.Batch.Conventions.Files.IntegrationTests.Xunit.ExtendedTestFramework", "Microsoft.Azure.Batch.Conventions.Files.IntegrationTests")]

namespace Microsoft.Azure.Batch.Conventions.Files.IntegrationTests
{
    public class IntegrationTest
    {
        private readonly JobIdFixture _jobIdFixture;

        public IntegrationTest(JobIdFixture jobIdFixture, ITestOutputHelper output)
        {
            Output = output;
            StorageAccount = StorageConfiguration.GetAccount(output);
            _jobIdFixture = jobIdFixture;

            // For relative path tests to work, we want to be in the directory that contains
            // our test upload files.  However, if we rerun the tests in an interactive runner
            // then it may reuse the process, in which case we will already be in the Files
            // directory.
            if (!Environment.CurrentDirectory.EndsWith("\\Files"))
            {
                Environment.CurrentDirectory = Path.Combine(Environment.CurrentDirectory, "Files");
            }
        }

        protected ITestOutputHelper Output { get; }

        protected CloudStorageAccount StorageAccount { get; }

        public void CancelTeardown() => _jobIdFixture.CancelTeardown();
    }
}
