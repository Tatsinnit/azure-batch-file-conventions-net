using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.IO;

namespace Microsoft.Azure.Batch.Conventions.Files
{
    internal class TrackedFile : IDisposable
    {
        public static readonly TimeSpan DefaultFlushInterval = TimeSpan.FromMinutes(1);

        private readonly Timer _timer;
        private readonly CloudAppendBlob _blob;
        private readonly string _filePath;
        private long _flushPointer = 0;
        private readonly object _lock = new object();

        public TrackedFile(string filePath, CloudAppendBlob blob, TimeSpan interval)
        {
            _filePath = filePath;
            _blob = blob;
            _timer = new Timer(OnTimer, null, TimeSpan.FromMilliseconds(1), interval);
        }

        public void OnTimer(object state)
        {
            Flush(FlushMode.IfIdle);
        }

        private void Flush(FlushMode flushMode)
        {
            // If this is the forced flush on Dispose, wait until we acquire the lock.  Otherwise,
            // just check to see if the lock is available, and if not, we are still processing the
            // last tranche of appends, so bail out and wait for the next flush interval.
            var lockTimeout = (flushMode == FlushMode.IfIdle ? 1 : Timeout.Infinite);
            bool acquiredLock = false;
            Monitor.TryEnter(_lock, lockTimeout, ref acquiredLock);

            if (!acquiredLock)
            {
                return;
            }

            try
            {
                var file = new FileInfo(_filePath);

                if (!file.Exists)
                {
                    return;
                }

                var uploadPointer = file.Length;

                if (uploadPointer <= _flushPointer)
                {
                    return;
                }

                using (var stm = new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    stm.Seek(_flushPointer, SeekOrigin.Begin);
                    _blob.AppendFromStream(stm, uploadPointer - _flushPointer);
                    _flushPointer = uploadPointer;
                }
            }
            finally
            {
                Monitor.Exit(_lock);
            }
        }

        private enum FlushMode
        {
            IfIdle,
            Force,
        }

        public void Dispose()
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            _timer.Dispose();

            Flush(FlushMode.Force);
        }
    }
}

