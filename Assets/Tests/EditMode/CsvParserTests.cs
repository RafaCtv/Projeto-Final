using System;
using NUnit.Framework;
using ProjetoFinal.Localization;

namespace ProjetoFinal.Tests
{
    public class CsvParserTests
    {
        [Test]
        public void Parse_SimpleRows_SplitsByCommaAndLine()
        {
            var rows = CsvParser.Parse("a,b\nc,d");

            Assert.AreEqual(2, rows.Count);
            CollectionAssert.AreEqual(new[] { "a", "b" }, rows[0]);
            CollectionAssert.AreEqual(new[] { "c", "d" }, rows[1]);
        }

        [Test]
        public void Parse_QuotedField_KeepsCommasNewlinesAndEscapedQuotes()
        {
            var rows = CsvParser.Parse("k,\"Olá, \"\"mundo\"\"\nlinha 2\"");

            Assert.AreEqual(1, rows.Count);
            Assert.AreEqual("Olá, \"mundo\"\nlinha 2", rows[0][1]);
        }

        [Test]
        public void Parse_CrLfBomAndBlankLines_AreHandled()
        {
            var rows = CsvParser.Parse("﻿a,b\r\n\r\nc,d\r\n");

            Assert.AreEqual(2, rows.Count);
            Assert.AreEqual("a", rows[0][0]);
        }

        [Test]
        public void Parse_UnterminatedQuote_Throws()
        {
            Assert.Throws<FormatException>(() => CsvParser.Parse("a,\"b"));
        }
    }
}
