﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Conventions.Files
{
    /// <summary>
    /// Represents a category of job outputs, such as the main task output, or a preview of the
    /// task output, or a log of the task processing.
    /// </summary>
    public sealed class TaskOutputKind : IEquatable<TaskOutputKind>
    {
        /// <summary>
        /// A <see cref="TaskOutputKind"/> representing the main output of a task.
        /// </summary>
        public static readonly TaskOutputKind TaskOutput = new TaskOutputKind("TaskOutput");

        /// <summary>
        /// A <see cref="TaskOutputKind"/> representing a preview of the task output.
        /// </summary>
        public static readonly TaskOutputKind TaskPreview = new TaskOutputKind("TaskPreview");

        /// <summary>
        /// A <see cref="TaskOutputKind"/> representing a log of the task processing.
        /// </summary>
        public static readonly TaskOutputKind TaskLog = new TaskOutputKind("TaskLog");

        /// <summary>
        /// A <see cref="TaskOutputKind"/> representing an intermediate file, for example being
        /// persisted for diagnostic or checkpointing purposes.
        /// </summary>
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

        /// <summary>
        /// Gets a <see cref="TaskOutputKind"/> representing a custom category of task outputs.
        /// </summary>
        /// <param name="text">A text identifier for the custom TaskOutputKind.</param>
        /// <returns>A TaskOutputKind with the specified text.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="text"/> is empty.</exception>
        public static TaskOutputKind Custom(string text)
        {
            return new TaskOutputKind(text);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A textual representation of the <see cref="TaskOutputKind"/>.</returns>
        public override string ToString()
        {
            return _text;
        }

        /// <summary>
        /// Determinates whether this instance and another specified <see cref="TaskOutputKind"/>
        /// have the same value.
        /// </summary>
        /// <param name="other">The TaskOutputKind to compare to this instance.</param>
        /// <returns>true if the value of the <paramref name="other"/> parameter is the same as
        /// the value of this instance; otherwise, false.</returns>
        public bool Equals(TaskOutputKind other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }
            return TextComparer.Equals(other._text, _text);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as TaskOutputKind);
        }

        /// <summary>
        /// Returns the hash code for this <see cref="TaskOutputKind"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            Debug.Assert(_text != null);

            return TextComparer.GetHashCode(_text);
        }

        /// <summary>
        /// Determines whether two specified <see cref="TaskOutputKind"/> instances have the same value.
        /// </summary>
        /// <param name="x">The first TaskOutputKind to compare.</param>
        /// <param name="y">The second TaskOutputKind to compare.</param>
        /// <returns>true if the value of <paramref name="x"/> is the same as the value of <paramref name="y"/>; otherwise, false.</returns>
        public static bool operator ==(TaskOutputKind x, TaskOutputKind y)
        {
            if (ReferenceEquals(x, null))
            {
                return ReferenceEquals(y, null);
            }
            return x.Equals(y);
        }

        /// <summary>
        /// Determines whether two specified <see cref="TaskOutputKind"/> instances have different values.
        /// </summary>
        /// <param name="x">The first TaskOutputKind to compare.</param>
        /// <param name="y">The second TaskOutputKind to compare.</param>
        /// <returns>true if the value of <paramref name="x"/> is different from the value of <paramref name="y"/>; otherwise, false.</returns>
        public static bool operator !=(TaskOutputKind x, TaskOutputKind y)
        {
            return !(x == y);
        }
    }
}
