﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Microsoft.Azure.Batch.Conventions.Files.IntegrationTests.Utilities;
using Microsoft.Azure.Batch.Conventions.Files.IntegrationTests.Xunit;

namespace Microsoft.Azure.Batch.Conventions.Files.IntegrationTests
{
    public class JobOutputStorageTests : IntegrationTest, IClassFixture<JobOutputStorageTests.JobIdFixtureImpl>
    {
        private readonly string _jobId;

        public JobOutputStorageTests(JobIdFixtureImpl jobIdFixture, ITestOutputHelper output)
            : base(jobIdFixture, output)
        {
            _jobId = jobIdFixture.JobId;
        }

        public class JobIdFixtureImpl : JobIdFixture
        {
            protected override string TestId { get; } = "joboutput";
        }

        [Fact]
        public async Task IfAFileIsSaved_ThenItAppearsInTheList()
        {
            var jobOutputStorage = new JobOutputStorage(StorageAccount, _jobId);
            await jobOutputStorage.SaveAsync(JobOutputKind.JobOutput, "TestText1.txt");

            var blobs = jobOutputStorage.ListOutputs(JobOutputKind.JobOutput).ToList();
            Assert.NotEqual(0, blobs.Count);
            Assert.Contains(blobs, b => b.Uri.AbsoluteUri.EndsWith($"{_jobId}/$JobOutput/TestText1.txt"));
        }

        [Fact]
        public async Task IfAFileIsSavedWithAnExplicitPath_ThenItAppearsInTheList()
        {
            var jobOutputStorage = new JobOutputStorage(StorageAccount, _jobId);
            await jobOutputStorage.SaveAsync(JobOutputKind.JobOutput, "TestText1.txt", "RenamedTestText1.txt");

            var blobs = jobOutputStorage.ListOutputs(JobOutputKind.JobOutput).ToList();
            Assert.NotEqual(0, blobs.Count);
            Assert.Contains(blobs, b => b.Uri.AbsoluteUri.EndsWith($"{_jobId}/$JobOutput/RenamedTestText1.txt"));
        }

        [Fact]
        public async Task IfAFileWithAMultiLevelPathIsSaved_ThenItAppearsInTheList()
        {
            var jobOutputStorage = new JobOutputStorage(StorageAccount, _jobId);
            await jobOutputStorage.SaveAsync(JobOutputKind.JobOutput, "File\\Under\\TestText2.txt");

            var blobs = jobOutputStorage.ListOutputs(JobOutputKind.JobOutput).ToList();
            Assert.NotEqual(0, blobs.Count);
            Assert.Contains(blobs, b => b.Uri.AbsoluteUri.EndsWith($"{_jobId}/$JobOutput/File/Under/TestText2.txt"));
        }

        [Fact]
        public async Task IfAFileIsSavedWithAnExplicitMultiLevelPath_ThenItAppearsInTheList()
        {
            var jobOutputStorage = new JobOutputStorage(StorageAccount, _jobId);
            await jobOutputStorage.SaveAsync(JobOutputKind.JobOutput, "TestText1.txt", "File/In/The/Depths/TestText3.txt");

            var blobs = jobOutputStorage.ListOutputs(JobOutputKind.JobOutput).ToList();
            Assert.NotEqual(0, blobs.Count);
            Assert.Contains(blobs, b => b.Uri.AbsoluteUri.EndsWith($"{_jobId}/$JobOutput/File/In/The/Depths/TestText3.txt"));
        }

        [Fact]
        public async Task IfAFileIsSaved_ThenItCanBeGot()
        {
            var jobOutputStorage = new JobOutputStorage(StorageAccount, _jobId);
            await jobOutputStorage.SaveAsync(JobOutputKind.JobOutput, "TestText1.txt", "Gettable.txt");

            var blob = await jobOutputStorage.GetOutputAsync(JobOutputKind.JobOutput, "Gettable.txt");

            var blobContent = await blob.ReadAsByteArrayAsync();
            var originalContent = File.ReadAllBytes("TestText1.txt");

            Assert.Equal(originalContent, blobContent);
        }

        [Fact]
        public async Task IfAFileIsSavedWithAMultiLevelPath_ThenItCanBeGot()
        {
            var jobOutputStorage = new JobOutputStorage(StorageAccount, _jobId);
            await jobOutputStorage.SaveAsync(JobOutputKind.JobOutput, "TestText1.txt", "This/File/Is/Gettable.txt");

            var blob = await jobOutputStorage.GetOutputAsync(JobOutputKind.JobOutput, "This/File/Is/Gettable.txt");

            var blobContent = await blob.ReadAsByteArrayAsync();
            var originalContent = File.ReadAllBytes("TestText1.txt");

            Assert.Equal(originalContent, blobContent);
        }
    }
}
