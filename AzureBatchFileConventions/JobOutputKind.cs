﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Conventions.Files
{
    /// <summary>
    /// Represents a category of job outputs, such as the main job output, or a preview of the
    /// job output.
    /// </summary>
    public sealed class JobOutputKind : IEquatable<JobOutputKind>
    {
        /// <summary>
        /// A <see cref="JobOutputKind"/> representing the main output of a job.
        /// </summary>
        public static readonly JobOutputKind JobOutput = new JobOutputKind("JobOutput");

        /// <summary>
        /// A <see cref="JobOutputKind"/> representing a preview of the job output.
        /// </summary>
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

        /// <summary>
        /// Gets a <see cref="JobOutputKind"/> representing a custom category of job outputs.
        /// </summary>
        /// <param name="text">A text identifier for the custom JobOutputKind.</param>
        /// <returns>A JobOutputKind with the specified text.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="text"/> is empty.</exception>
        public static JobOutputKind Custom(string text)
        {
            return new JobOutputKind(text);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A textual representation of the <see cref="JobOutputKind"/>.</returns>
        public override string ToString()
        {
            return _text;
        }

        /// <summary>
        /// Determinates whether this instance and another specified <see cref="JobOutputKind"/>
        /// have the same value.
        /// </summary>
        /// <param name="other">The JobOutputKind to compare to this instance.</param>
        /// <returns>true if the value of the <paramref name="other"/> parameter is the same as
        /// the value of this instance; otherwise, false.</returns>
        public bool Equals(JobOutputKind other)
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
            return Equals(obj as JobOutputKind);
        }

        /// <summary>
        /// Returns the hash code for this <see cref="JobOutputKind"/>.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            Debug.Assert(_text != null);

            return TextComparer.GetHashCode(_text);
        }

        /// <summary>
        /// Determines whether two specified <see cref="JobOutputKind"/> instances have the same value.
        /// </summary>
        /// <param name="x">The first JobOutputKind to compare.</param>
        /// <param name="y">The second JobOutputKind to compare.</param>
        /// <returns>true if the value of <paramref name="x"/> is the same as the value of <paramref name="y"/>; otherwise, false.</returns>
        public static bool operator ==(JobOutputKind x, JobOutputKind y)
        {
            if (ReferenceEquals(x, null))
            {
                return ReferenceEquals(y, null);
            }
            return x.Equals(y);
        }

        /// <summary>
        /// Determines whether two specified <see cref="JobOutputKind"/> instances have different values.
        /// </summary>
        /// <param name="x">The first JobOutputKind to compare.</param>
        /// <param name="y">The second JobOutputKind to compare.</param>
        /// <returns>true if the value of <paramref name="x"/> is different from the value of <paramref name="y"/>; otherwise, false.</returns>
        public static bool operator !=(JobOutputKind x, JobOutputKind y)
        {
            return !(x == y);
        }
    }
}
