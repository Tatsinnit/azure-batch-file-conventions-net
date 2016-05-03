using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Conventions.Files.Utilities
{
    internal static class ContainerNameUtils
    {
        private static readonly Regex PermittedContainerNameChars = new Regex("^[-a-z0-9]{3,63}$");
        private static readonly HashAlgorithm hasher = new SHA1CryptoServiceProvider();
        private static readonly int MaxJobIdLengthInMungedContainerName = 15;  // must be <= 63 - 1 - length of hash string (40 for SHA1)
        private static readonly char[] ForbiddenLeadingTrailingContainerNameChars = new[] { '-' };
        private static readonly Regex UnderscoresAndMultipleDashes = new Regex("[_-]+");

        internal static string GetSafeContainerName(string jobId)
        {
            jobId = jobId.ToLowerInvariant();  // it's safe to do this early as job ids cannot differ only by case, so the lower case job id is still a unique identifier

            if (!PermittedContainerNameChars.IsMatch(jobId))
            {
                return MungeToContainerName(jobId);
            }

            if (!Char.IsLetterOrDigit(jobId[0]) || !Char.IsLetterOrDigit(jobId[jobId.Length - 1]) || jobId.Contains("--"))
            {
                return MungeToContainerName(jobId);
            }

            return jobId;
        }

        private static string MungeToContainerName(string str)
        {
            var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(str));
            var hashText = ToHex(hash);

            var safeStr = MakeDashSafe(str);

            safeStr = safeStr.Trim(ForbiddenLeadingTrailingContainerNameChars);

            if (safeStr.Length > MaxJobIdLengthInMungedContainerName)
            {
                safeStr = safeStr.Substring(0, MaxJobIdLengthInMungedContainerName)
                                 .Trim(ForbiddenLeadingTrailingContainerNameChars);  // do this again as truncation may have unleashed a trailing dash
            }
            else if (safeStr.Length == 0)
            {
                safeStr = "job";
            }

            return safeStr + "-" + hashText;
        }

        private static string MakeDashSafe(string rawString)
        {
            return UnderscoresAndMultipleDashes.Replace(rawString, "-");
        }

        private static string ToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes)
            {
                sb.AppendFormat(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
