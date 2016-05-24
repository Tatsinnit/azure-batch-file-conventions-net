# Azure Batch File Conventions

A convention-based library for saving and retrieving Azure Batch task output files.

## Purpose

When you run a task in Azure Batch, the files created by that task are on the
compute node where the task ran.  As long as the compute node remains up, you can
retrieve those files via the Batch API.  However, if you need the files to remain
available even if the compute node is taken down (for example, as part of a pool
resize), you must persist those files to a durable store.

This library encapsulates a convention for persisting job and task outputs in Azure blob
storage.  This allows client code to easily locate the outputs for a given job or
task, allowing those outputs to be listed or retrieved by ID and purpose.  For example,
a client can use the library to request 'list all the intermediate files for task 7'
or 'get me the preview for job "mymovie"' without needing to know names or locations.

The categorisation of persisted files as 'output', 'preview', etc. is done using the
JobOutputKind and TaskOutputKind types.  For job output files, the predefined kinds
are "JobOutput" and "JobPreview"; for task output files, "TaskOutput", "TaskPreview",
"TaskLog" and "TaskIntermediate".  You can also define custom kinds if that these
are useful in your workflow.

## Prerequisites

The library uses the storage account linked to your Batch account.  If your Batch account
doesn't have a linked storage account, you can configure one using the Azure portal.

## Usage

The library is intended for use in both task code and client code -- in task code to
persist files, in client code to list and retrieve them.

### Persisting Files in Task Code

To persist a file from task code, use the JobOutputStorage and TaskOutputStorage
constructors that take a job output container URL, and call the SaveAsync method:

    var jobOutputContainerUri = new Uri(Environment.GetEnvironmentVariable("AZ_BATCH_JOB_OUTPUT_CONTAINER"));  // TODO: confirm environment variable
    var taskId = Environment.GetEnvironmentVariable("AZ_BATCH_TASK_ID");
    
    var taskOutputStorage = new TaskOutputStorage(jobOutputContainerUri, taskId);
    
    await taskOutputStorage.SaveAsync(TaskOutputKind.TaskOutput, "frame_full_res.jpg");
    await taskOutputStorage.SaveAsync(TaskOutputKind.TaskPreview, "frame_low_res.jpg");
    
**TODO:** document helpers in C# task processor template generated code

### Listing and Retrieving Files in Client Code

To access persisted files from client code, you must configure the client with
the details of the linked storage account.  Then use the JobOutputStorage and
TaskOutputStorage constructors that take a CloudStorageAccount, or the extension
methods on CloudJob and CloudTask.

    var job = await batchClient.JobOperations.GetJobAsync(jobId);
    var jobOutputStorage = job.OutputStorage(linkedStorageAccount);

    var jobOutputBlob = jobOutputStorage.ListOutputs(JobOutputKind.JobOutput)
                                        .SingleOrDefault()
	    								as CloudBlockBlob;

    if (jobOutputBlob != null)
    {
        await jobOutputBlob.DownloadToFileAsync("movie.mp4", FileMode.Create);
    }

