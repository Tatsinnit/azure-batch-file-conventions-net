using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Conventions.Files
{
    public sealed class TaskOutputKind : IEquatable<TaskOutputKind>
    {
        public static readonly TaskOutputKind TaskOutput = new TaskOutputKind("TaskOutput");
        public static readonly TaskOutputKind TaskPreview = new TaskOutputKind("TaskPreview");
        public static readonly TaskOutputKind TaskLog = new TaskOutputKind("TaskLog");
        public static readonly TaskOutputKind TaskIntermediate = new TaskOutputKind("TaskIntermediate");

        private static readonly StringComparer TextComparer = StringComparer.Ordinal;  // case sensitive, since we preserve case when stringising

        private readonly string _text;

        private TaskOutputKind(string text)
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

        public static TaskOutputKind Custom(string text)
        {
            return new TaskOutputKind(text);
        }

        public override string ToString()
        {
            return _text;
        }

        public bool Equals(TaskOutputKind other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            return TextComparer.Equals(other._text, _text);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TaskOutputKind);
        }

        public override int GetHashCode()
        {
            Debug.Assert(_text != null);

            return TextComparer.GetHashCode(_text);
        }

        public static bool operator ==(TaskOutputKind x, TaskOutputKind y)
        {
            if (ReferenceEquals(x, null))
            {
                return ReferenceEquals(y, null);
            }
            return x.Equals(y);
        }

        public static bool operator !=(TaskOutputKind x, TaskOutputKind y)
        {
            return !(x == y);
        }
    }
}
