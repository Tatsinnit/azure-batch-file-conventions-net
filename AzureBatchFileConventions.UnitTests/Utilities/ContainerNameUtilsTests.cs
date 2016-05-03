using FsCheck;
using FsCheck.Xunit;
using Microsoft.Azure.Batch.Conventions.Files.UnitTests.Generators;
using Microsoft.Azure.Batch.Conventions.Files.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Azure.Batch.Conventions.Files.UnitTests.Utilities
{
    [Arbitrary(typeof(BatchIdGenerator))]
    public class ContainerNameUtilsTests
    {
        // Verify generated container names against the Azure blob container naming rules
        // from https://msdn.microsoft.com/en-us/library/azure/dd135715.aspx

        [Property]
        public void ContainerNamesMustStartWithALetterOrNumber(BatchId jobId)
        {
            var containerName = ContainerNameUtils.GetSafeContainerName(jobId.ToString());
            Assert.True(Char.IsLetterOrDigit(containerName[0]));
        }

        [Property]
        public void ContainerNamesCanContainOnlyLettersNumbersAndDashes(BatchId jobId)
        {
            var containerName = ContainerNameUtils.GetSafeContainerName(jobId.ToString());
            Assert.All(containerName,
                ch => Assert.True(Char.IsLetterOrDigit(ch) || ch == '-'));
        }

        private static IEnumerable<int> IndexesOf(string str, char ch)
        {
            return Enumerable.Range(0, str.Length)
                             .Where(i => str[i] == ch);
        }

        [Property]
        public void EveryDashInAContainerNameMustBeImmediatelyPrecededAndFollowedByALetterOrNumber(BatchId jobId)
        {
            var containerName = ContainerNameUtils.GetSafeContainerName(jobId.ToString());

            var dashIndexesInContainerName = IndexesOf(containerName, '-');

            Assert.All(dashIndexesInContainerName,
                i => Assert.True(Char.IsLetterOrDigit(containerName[i - 1]) && Char.IsLetterOrDigit(containerName[i + 1])));
        }

        [Property]
        public void ConsecutiveDashesAreNotPermittedInContainerNames(BatchId jobId)
        {
            var containerName = ContainerNameUtils.GetSafeContainerName(jobId.ToString());
            Assert.DoesNotContain("--", containerName);
        }

        [Property]
        public void AllLettersInAContainerNameMustBeLowercase(BatchId jobId)
        {
            var containerName = ContainerNameUtils.GetSafeContainerName(jobId.ToString());
            Assert.All(containerName,
                ch => Assert.True(!Char.IsLetter(ch) || Char.IsLower(ch)));
        }

        [Property]
        public void ContainerNamesMustBeFrom3Through63CharactersLong(BatchId jobId)
        {
            var containerName = ContainerNameUtils.GetSafeContainerName(jobId.ToString());
            Assert.InRange(containerName.Length, 3, 63);
        }

        [Property]
        public void ValidContainerNamesAreNotMunged(BatchIdThatIsValidContainerName jobId)
        {
            var containerName = ContainerNameUtils.GetSafeContainerName(jobId.ToString());
            Assert.Equal(jobId.ToString(), containerName);
        }
    }
}
