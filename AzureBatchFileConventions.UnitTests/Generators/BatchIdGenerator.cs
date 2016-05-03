﻿using FsCheck;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Batch.Conventions.Files.UnitTests.Generators
{
    public static class BatchIdGenerator
    {
        private static readonly IEnumerable<char> IdChars =
            Enumerable.Range((int)'A', 26).Concat(
            Enumerable.Range((int)'a', 26)).Concat(
            Enumerable.Range((int)'0', 10)).Concat(
            new int[] { '-', '_' })
            .Select(i => (char)i)  // Need to use Select because Cast<char> fails with InvalidCastException
            .ToList().AsReadOnly();

        private static readonly IEnumerable<char> ContainerNameChars =
            Enumerable.Range((int)'a', 26).Concat(
            Enumerable.Range((int)'0', 10)).Concat(
            new int[] { '-' })
            .Select(i => (char)i)  // Need to use Select because Cast<char> fails with InvalidCastException
            .ToList().AsReadOnly();

        private static readonly IEnumerable<char> ContainerNameTerminalChars =
            Enumerable.Range((int)'a', 26).Concat(
            Enumerable.Range((int)'0', 10))
            .Select(i => (char)i)  // Need to use Select because Cast<char> fails with InvalidCastException
            .ToList().AsReadOnly();

        public static Arbitrary<BatchId> BatchId => Arb.From(BatchIdGen, BatchIdShrink);
        public static Arbitrary<BatchIdThatIsValidContainerName> BatchIdThatIsValidContainerName => Arb.From(BatchIdThatIsValidContainerNameGen, BatchIdThatIsValidContainerNameShrink);

        private static readonly Gen<BatchId> BatchIdGen =
            from len in Gen.Choose(1, 64)
            from chars in Gen.ArrayOf(len, Gen.Elements(IdChars))
            select new BatchId(new string(chars));

        private static IEnumerable<BatchId> BatchIdShrink(BatchId batchId)
        {
            var id = batchId.ToString();
            return (id == null || id.Length <= 1) ?
                Enumerable.Empty<BatchId>() :
                Enumerable.Range(0, id.Length - 1).Select(index => new BatchId(id.Remove(index, 1)));
        }

        private static string ConcatChars(char startChar, char[] midChars, char endChar)
        {
            return startChar + new string(midChars) + endChar;
        }

        private static readonly Gen<BatchIdThatIsValidContainerName> BatchIdThatIsValidContainerNameGen =
            from len in Gen.Choose(1, 61)
            from startChar in Gen.Elements(ContainerNameTerminalChars)
            from midChars in Gen.ArrayOf(len, Gen.Elements(ContainerNameChars))
            from endChar in Gen.Elements(ContainerNameTerminalChars)
            let str = ConcatChars(startChar, midChars, endChar)
            where !str.Contains("--")
            select new BatchIdThatIsValidContainerName(str);

        private static IEnumerable<BatchIdThatIsValidContainerName> BatchIdThatIsValidContainerNameShrink(BatchIdThatIsValidContainerName batchId)
        {
            var id = batchId.ToString();
            return (id == null || id.Length <= 3) ?
                Enumerable.Empty<BatchIdThatIsValidContainerName>() :
                Enumerable.Range(0, id.Length - 1)
                          .Select(index => id.Remove(index, 1))
                          .Where(s => !s.Contains("--"))
                          .Select(s => new BatchIdThatIsValidContainerName(s));
        }
    }

    public struct BatchId
    {
        private readonly string _id;

        public BatchId(string id)
        {
            _id = id;
        }

        public override string ToString()
        {
            return _id;
        }
    }

    public struct BatchIdThatIsValidContainerName
    {
        private readonly string _id;

        public BatchIdThatIsValidContainerName(string id)
        {
            _id = id;
        }

        public override string ToString()
        {
            return _id;
        }
    }
}
