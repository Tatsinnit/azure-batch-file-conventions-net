using Microsoft.Azure.Batch.Conventions.Files.IntegrationTests.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using System.Threading;
using Microsoft.Azure.Batch.Conventions.Files.IntegrationTests.Xunit;
using Microsoft.WindowsAzure.Storage;

namespace Microsoft.Azure.Batch.Conventions.Files.IntegrationTests
{
    public class TaskOutputStorageTests : IntegrationTest, IClassFixture<TaskOutputStorageTests.JobIdFixtureImpl>
    {
        private readonly string _jobId;
        private readonly string _taskId = "test-task";

        public TaskOutputStorageTests(JobIdFixtureImpl jobIdFixture, ITestOutputHelper output)
            : base(jobIdFixture, output)
        {
            _jobId = jobIdFixture.JobId;
        }

        public class JobIdFixtureImpl : JobIdFixture
        {
            protected override string TestId { get; } = "taskoutput";
        }

        [Fact]
        public async Task IfAFileIsSaved_ThenItAppearsInTheList()
        {
            var taskOutputStorage = new TaskOutputStorage(StorageAccount, _jobId, _taskId);
            await taskOutputStorage.SaveAsyncImpl(TaskOutputKind.TaskPreview, FileBase, "TestText1.txt");

            var blobs = taskOutputStorage.ListOutputs(TaskOutputKind.TaskPreview).ToList();
            Assert.NotEqual(0, blobs.Count);
            Assert.Contains(blobs, b => b.Uri.AbsoluteUri.EndsWith($"{_jobId}/{_taskId}/$TaskPreview/TestText1.txt"));
        }

        [Fact]
        public async Task IfAFileIsSaved_UsingThePublicMethod_ThenTheCurrentDirectoryIsInferred()
        {
            // To avoid needing to mess with the process working directory, relative path tests
            // normally go through the internal SaveAsyncImpl method.  This test verifies that
            // the public SaveAsync method forwards the appropriate directory to SaveAsyncImpl.

            Assert.True(File.Exists(FilePath("TestText1.txt")), "Current directory is not what was expected - cannot verify current directory inference");

            var taskOutputStorage = new TaskOutputStorage(StorageAccount, _jobId, _taskId);
            await taskOutputStorage.SaveAsync(TaskOutputKind.TaskPreview, FilePath("TestText1.txt"));

            var blobs = taskOutputStorage.ListOutputs(TaskOutputKind.TaskPreview).ToList();
            Assert.NotEqual(0, blobs.Count);
            Assert.Contains(blobs, b => b.Uri.AbsoluteUri.EndsWith($"{_jobId}/{_taskId}/$TaskPreview/TestText1.txt"));
        }

        [Fact]
        public async Task IfAFileIsSavedWithAnExplicitPath_ThenItAppearsInTheList()
        {
            var taskOutputStorage = new TaskOutputStorage(StorageAccount, _jobId, _taskId);
            await taskOutputStorage.SaveAsync(TaskOutputKind.TaskPreview, FilePath("TestText1.txt"), "RenamedTestText1.txt");

            var blobs = taskOutputStorage.ListOutputs(TaskOutputKind.TaskPreview).ToList();
            Assert.NotEqual(0, blobs.Count);
            Assert.Contains(blobs, b => b.Uri.AbsoluteUri.EndsWith($"{_jobId}/{_taskId}/$TaskPreview/RenamedTestText1.txt"));
        }

        [Fact]
        public async Task IfAFileWithAMultiLevelPathIsSaved_ThenItAppearsInTheList()
        {
            var taskOutputStorage = new TaskOutputStorage(StorageAccount, _jobId, _taskId);
            await taskOutputStorage.SaveAsyncImpl(TaskOutputKind.TaskPreview, FileBase, "File\\Under\\TestText2.txt");

            var blobs = taskOutputStorage.ListOutputs(TaskOutputKind.TaskPreview).ToList();
            Assert.NotEqual(0, blobs.Count);
            Assert.Contains(blobs, b => b.Uri.AbsoluteUri.EndsWith($"{_jobId}/{_taskId}/$TaskPreview/File/Under/TestText2.txt"));
        }

        [Fact]
        public async Task IfAFileIsSavedWithAnExplicitMultiLevelPath_ThenItAppearsInTheList()
        {
            var taskOutputStorage = new TaskOutputStorage(StorageAccount, _jobId, _taskId);
            await taskOutputStorage.SaveAsync(TaskOutputKind.TaskPreview, FilePath("TestText1.txt"), "File/In/The/Depths/TestText3.txt");

            var blobs = taskOutputStorage.ListOutputs(TaskOutputKind.TaskPreview).ToList();
            Assert.NotEqual(0, blobs.Count);
            Assert.Contains(blobs, b => b.Uri.AbsoluteUri.EndsWith($"{_jobId}/{_taskId}/$TaskPreview/File/In/The/Depths/TestText3.txt"));
        }

        [Fact]
        public async Task IfAFileIsSaved_ThenItCanBeGot()
        {
            var taskOutputStorage = new TaskOutputStorage(StorageAccount, _jobId, _taskId);
            await taskOutputStorage.SaveAsync(TaskOutputKind.TaskPreview, FilePath("TestText1.txt"), "Gettable.txt");

            var blob = await taskOutputStorage.GetOutputAsync(TaskOutputKind.TaskPreview, "Gettable.txt");

            var blobContent = await blob.ReadAsByteArrayAsync();
            var originalContent = File.ReadAllBytes(FilePath("TestText1.txt"));

            Assert.Equal(originalContent, blobContent);
        }

        [Fact]
        public async Task IfAFileIsSavedWithAMultiLevelPath_ThenItCanBeGot()
        {
            var taskOutputStorage = new TaskOutputStorage(StorageAccount, _jobId, _taskId);
            await taskOutputStorage.SaveAsync(TaskOutputKind.TaskPreview, FilePath("TestText1.txt"), "This/File/Is/Gettable.txt");

            var blob = await taskOutputStorage.GetOutputAsync(TaskOutputKind.TaskPreview, "This/File/Is/Gettable.txt");

            var blobContent = await blob.ReadAsByteArrayAsync();
            var originalContent = File.ReadAllBytes(FilePath("TestText1.txt"));

            Assert.Equal(originalContent, blobContent);
        }

        [Fact]
        public async Task IfAFileIsSavedWithAPathOutsideTheWorkingDirectory_ThenTheUpPartsOfThePathAreStripped()
        {
            var taskOutputStorage = new TaskOutputStorage(StorageAccount, _jobId, _taskId);
            await taskOutputStorage.SaveAsyncImpl(TaskOutputKind.TaskIntermediate, FileSubfolder("File"), @"..\TestTextForOutsideWorkingDirectory.txt");

            var blob = await taskOutputStorage.GetOutputAsync(TaskOutputKind.TaskIntermediate, "TestTextForOutsideWorkingDirectory.txt");

            var blobContent = await blob.ReadAsByteArrayAsync();
            var originalContent = File.ReadAllBytes(FilePath("TestTextForOutsideWorkingDirectory.txt"));

            Assert.Equal(originalContent, blobContent);
        }

        [Fact]
        public async Task IfAUserAttemptsToWriteOutsideTheContainerByBypassingTheUpChecker_ThenTheWriteFails()
        {
            var taskOutputStorage = new TaskOutputStorage(StorageAccount, _jobId, _taskId);
            var ex = await Assert.ThrowsAsync<StorageException>(async () =>
                await taskOutputStorage.SaveAsyncImpl(TaskOutputKind.TaskIntermediate, FileSubfolder("File\\Under\\Further"), @"Under\..\..\..\..\TestTextForFarOutsideWorkingDirectory.txt")
            );

            Assert.Equal(404, ex.RequestInformation.HttpStatusCode);
        }
    }
}
