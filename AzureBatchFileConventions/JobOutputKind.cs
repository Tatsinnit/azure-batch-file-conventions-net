using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Conventions.Files
{
    public sealed class JobOutputKind : IEquatable<JobOutputKind>
    {
        public static readonly JobOutputKind JobOutput = new JobOutputKind("JobOutput");
        public static readonly JobOutputKind JobPreview = new JobOutputKind("JobPreview");

        private static readonly StringComparer TextComparer = StringComparer.Ordinal;  // case sensitive, since we preserve case when stringising

        private readonly string _text;

        private JobOutputKind(string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            if (String.IsNullOrEmpty(text))
            {
                throw new ArgumentException("Text must not be empty", nameof(text));
            }

            _text = text;
        }

        public static JobOutputKind Custom(string text)
        {
            return new JobOutputKind(text);
        }

        public override string ToString()
        {
            return _text;
        }

        public bool Equals(JobOutputKind other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            return TextComparer.Equals(other._text, _text);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as JobOutputKind);
        }

        public override int GetHashCode()
        {
            Debug.Assert(_text != null);

            return TextComparer.GetHashCode(_text);
        }

        public static bool operator ==(JobOutputKind x, JobOutputKind y)
        {
            if (ReferenceEquals(x, null))
            {
                return ReferenceEquals(y, null);
            }
            return x.Equals(y);
        }

        public static bool operator !=(JobOutputKind x, JobOutputKind y)
        {
            return !(x == y);
        }
    }
}
