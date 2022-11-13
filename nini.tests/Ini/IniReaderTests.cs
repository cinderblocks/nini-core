#region Copyright

//
// Nini Configuration Project.
// Copyright (C) 2006 Brent R. Matzelle.  All rights reserved.
//
// This software is published under the terms of the MIT X11 license, a copy of 
// which has been included with this distribution in the LICENSE.txt file.
// 

#endregion

using System.IO;
using Nini.Ini;
using NUnit.Framework;

namespace Nini.Test.Ini
{
    [TestFixture]
    public class IniReaderTests
    {
        #region General Tests

        [Test]
        public void NormalComment()
        {
            var writer = new StringWriter();
            writer.WriteLine("");
            writer.WriteLine(" ; Something");
            writer.WriteLine(" ;   Some comment  ");
            writer.WriteLine(" ;");
            var reader = new IniReader(new StringReader(writer.ToString()));

            Assert.AreEqual(IniReadState.Initial, reader.ReadState);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(IniReadState.Interactive, reader.ReadState);
            Assert.AreEqual(IniType.Empty, reader.Type);
            Assert.AreEqual("", reader.Name);
            Assert.AreEqual(null, reader.Comment);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(IniType.Empty, reader.Type);
            Assert.AreEqual("Something", reader.Comment);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(IniType.Empty, reader.Type);
            Assert.AreEqual("Some comment", reader.Comment);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("", reader.Comment);

            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void NormalSectionAndKey()
        {
            var writer = new StringWriter();
            writer.WriteLine("[Logging]");
            writer.WriteLine(" great logger =   log4net  ");
            writer.WriteLine("  [Pets] ; pets comment  ");
            var reader = new IniReader(new StringReader(writer.ToString()));

            Assert.AreEqual(IniReadState.Initial, reader.ReadState);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(IniReadState.Interactive, reader.ReadState);
            Assert.AreEqual(IniType.Section, reader.Type);
            Assert.AreEqual("Logging", reader.Name);
            Assert.AreEqual("", reader.Value);
            Assert.IsNull(reader.Comment);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(IniType.Key, reader.Type);
            Assert.AreEqual("great logger", reader.Name);
            Assert.AreEqual("log4net", reader.Value);
            Assert.AreEqual(null, reader.Comment);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(IniType.Section, reader.Type);
            Assert.AreEqual("Pets", reader.Name);
            Assert.AreEqual("", reader.Value);
            Assert.IsNull(reader.Comment);
        }

        [Test]
        public void KeyWithQuotes()
        {
            var writer = new StringWriter();
            writer.WriteLine("[Nini]");
            writer.WriteLine("  whitespace = \"  remove thing\"  ");
            var reader = new IniReader(new StringReader(writer.ToString()));

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(IniType.Key, reader.Type);
            Assert.AreEqual("whitespace", reader.Name);
            Assert.AreEqual("  remove thing", reader.Value);
            Assert.AreEqual(null, reader.Comment);

            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void SectionWithNoEndBracket()
        {
            Assert.Throws<IniException>(() =>
            {
                var writer = new StringWriter();
                writer.WriteLine("[Nini");
                writer.WriteLine("");
                var reader = new IniReader(new StringReader(writer.ToString()));

                Assert.IsTrue(reader.Read());
            });
        }

        [Test]
        public void LinePositionAndNumber()
        {
            var writer = new StringWriter();
            writer.WriteLine("; Test");
            writer.WriteLine("; Test 1");
            writer.WriteLine("[Nini Thing");
            var reader = new IniReader(new StringReader(writer.ToString()));

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());

            try
            {
                reader.Read();
            }
            catch (IniException e)
            {
                Assert.AreEqual(3, e.LineNumber);
                Assert.AreEqual(13, e.LinePosition);
            }
        }

        [Test]
        public void KeysWithSameName()
        {
            var writer = new StringWriter();
            writer.WriteLine("[Nini]");
            writer.WriteLine(" superkey = legal ");
            writer.WriteLine("[Pets]");
            writer.WriteLine(" superkey = legal ");
            writer.WriteLine(" superkey = overrides original ");
            var reader = new IniReader(new StringReader(writer.ToString()));

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());

            reader.Read();
        }

        [Test]
        public void SectionsWithSameName()
        {
            var writer = new StringWriter();
            writer.WriteLine("[Nini]");
            writer.WriteLine(" some key = something");
            writer.WriteLine("[Nini]");
            var reader = new IniReader(new StringReader(writer.ToString()));

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            try
            {
                reader.Read();
            }
            catch (IniException e)
            {
                Assert.AreEqual(3, e.LineNumber);
                Assert.AreEqual(6, e.LinePosition);
            }
        }

        [Test]
        public void IgnoreComments()
        {
            StringWriter writer = new StringWriter();
            writer.WriteLine("[Nini]");
            writer.WriteLine(" some key = something ; my comment 1");
            IniReader reader = new IniReader(new StringReader(writer.ToString()));

            Assert.IsTrue(reader.Read());
            reader.IgnoreComments = true;
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(null, reader.Comment);
        }

        [Test]
        public void NoEndingQuote()
        {
            Assert.Throws<IniException>(() =>
            {
                var writer = new StringWriter();
                writer.WriteLine("[Nini]");
                writer.WriteLine(" some key = \" something ");
                var reader = new IniReader(new StringReader(writer.ToString()));

                Assert.IsTrue(reader.Read());
                Assert.IsTrue(reader.Read());
            });
        }

        [Test]
        public void KeyWithNoEquals()
        {
            Assert.Throws<IniException>(() =>
            {
                var writer = new StringWriter();
                writer.WriteLine("[Nini]");
                writer.WriteLine(" some key ");
                var reader = new IniReader(new StringReader(writer.ToString()));

                Assert.IsTrue(reader.Read());
                Assert.IsTrue(reader.Read());
            });
        }

        [Test]
        public void MoveToNextSection()
        {
            var writer = new StringWriter();
            writer.WriteLine("; Test");
            writer.WriteLine("; Test 1");
            writer.WriteLine("[Nini Thing]");
            var reader = new IniReader(new StringReader(writer.ToString()));

            Assert.IsTrue(reader.MoveToNextSection());
            Assert.AreEqual(4, reader.LineNumber);
            Assert.AreEqual(IniType.Section, reader.Type);
            Assert.IsFalse(reader.MoveToNextSection());
        }

        [Test]
        public void MoveToNextKey()
        {
            var writer = new StringWriter();
            writer.WriteLine("; Test");
            writer.WriteLine("; Test 1");
            writer.WriteLine("[Nini Thing]");
            writer.WriteLine("; Test");
            writer.WriteLine(" my key = new key");
            var reader = new IniReader(new StringReader(writer.ToString()));

            Assert.IsFalse(reader.MoveToNextKey());
            Assert.AreEqual(4, reader.LineNumber);
            Assert.IsTrue(reader.MoveToNextKey());
            Assert.AreEqual(6, reader.LineNumber);
            Assert.AreEqual(IniType.Key, reader.Type);
            Assert.AreEqual("my key", reader.Name);
        }

        [Test]
        public void NoSectionsOrKeys()
        {
            var writer = new StringWriter();
            writer.WriteLine("");

            var reader = new IniReader(new StringReader(writer.ToString()));
            reader.Read();
            Assert.IsTrue(true);
        }

        [Test]
        public void CommentCharInString()
        {
            var writer = new StringWriter();
            writer.WriteLine("Value = \"WEB;www.google.com|WEB;www.yahoo.com\"");
            var reader = new IniReader(new StringReader(writer.ToString()));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("Value", reader.Name);
            Assert.AreEqual("WEB;www.google.com|WEB;www.yahoo.com", reader.Value);
        }

        [Test]
        public void ConsumeAllKeyText()
        {
            var writer = new StringWriter();
            writer.WriteLine("email = \"John Smith\"; <jsmith@something.com>");
            var reader = new IniReader(new StringReader(writer.ToString())) {ConsumeAllKeyText = true};

            Assert.IsTrue(reader.Read());
            Assert.AreEqual("email", reader.Name);
            Assert.AreEqual("\"John Smith\"; <jsmith@something.com>", reader.Value);
        }

        [Test]
        public void AcceptNoKeyEndings()
        {
            var writer = new StringWriter();
            writer.WriteLine("[Mysql]");
            writer.WriteLine("quick");
            writer.WriteLine(" my key = new key");
            var reader = new IniReader(new StringReader(writer.ToString())) {AcceptNoAssignmentOperator = true};


            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual("quick", reader.Name);
            Assert.AreEqual("", reader.Value);
        }

        #endregion

        #region No end of line tests

        [Test]
        public void NoEndOfLineComment()
        {
            var writer = new StringWriter();
            writer.Write(" ;   Some comment  ");

            var reader = new IniReader(new StringReader(writer.ToString()));
            reader.Read();
            Assert.IsTrue(true);
        }

        [Test]
        public void NoEndOfLineKey()
        {
            StringWriter writer = new StringWriter();
            writer.WriteLine("[Nini Thing]");
            writer.Write(" somekey = key ");

            IniReader reader = new IniReader(new StringReader(writer.ToString()));
            reader.Read();
            Assert.IsTrue(true);
        }

        [Test]
        public void NoEndOfLineKeyNoValue()
        {
            StringWriter writer = new StringWriter();
            writer.WriteLine("[Nini Thing]");
            writer.Write(" somekey = ");

            IniReader reader = new IniReader(new StringReader(writer.ToString()));
            reader.Read();
            Assert.IsTrue(true);
        }

        [Test]
        public void NoEndOfLineSection()
        {
            var writer = new StringWriter();
            writer.Write("[Nini Thing]");

            var reader = new IniReader(new StringReader(writer.ToString()));
            reader.Read();
            Assert.IsTrue(true);
        }

        [Test]
        public void EndCommentUnix()
        {
            var writer = new StringWriter();
            writer.WriteLine("[Test]");
            writer.WriteLine("; Test");
            writer.WriteLine(" float1 = 1.0 ;"); // no space after comment
            writer.WriteLine(" float2 = 2.0");

            var reader = new IniReader(new StringReader(ConvertToUnix(writer.ToString())));

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual("float1", reader.Name, "float1 not found");
            Assert.AreEqual("1.0", reader.Value, "float1 value not found");
            Assert.IsTrue(reader.Read(), "Could not find last float");
            Assert.AreEqual("float2", reader.Name);
            Assert.AreEqual("2.0", reader.Value);
        }

        [Test]
        public void NoLineContinuation()
        {
            Assert.Throws<IniException>(() =>
            {
                var writer = new StringWriter();
                writer.WriteLine("[Test]");
                writer.WriteLine(" option = this will be \\ ");
                writer.WriteLine("continued later");

                var reader = new IniReader(new StringReader(writer.ToString()));

                Assert.IsTrue(reader.Read());
                Assert.IsTrue(reader.Read());
                Assert.IsTrue(reader.Read());
            });
        }

        [Test]
        public void LineContinuation()
        {
            var writer = new StringWriter();
            writer.WriteLine("[Test]");
            writer.WriteLine(" option = this will be \\ ");
            writer.WriteLine("continued later");

            var reader = new IniReader(new StringReader(writer.ToString())) {LineContinuation = true};

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual("this will be continued later", reader.Value);
            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void LineContinuationMoreSpace()
        {
            var writer = new StringWriter();
            writer.WriteLine("[Test]");
            writer.WriteLine(" option = this will be \\ ");
            writer.WriteLine("     continued later");

            var reader = new IniReader(new StringReader(writer.ToString())) {LineContinuation = true};

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual("this will be      continued later", reader.Value);
            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void LineContinuationAnotherChar()
        {
            var writer = new StringWriter();
            writer.WriteLine("[Test]");
            writer.WriteLine(" option1 = this will be \\ continued");
            writer.WriteLine(" option2 = this will be continued");

            var reader = new IniReader(new StringReader(writer.ToString())) {LineContinuation = true};

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual("this will be \\ continued", reader.Value);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual("this will be continued", reader.Value);
            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void LineContinuationNoSpace()
        {
            var writer = new StringWriter();
            writer.WriteLine("[Test]");
            writer.WriteLine(" option = this will be \\");
            writer.WriteLine("continued later");

            var reader = new IniReader(new StringReader(writer.ToString())) {LineContinuation = true};

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual("this will be continued later", reader.Value);
            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void CommentAfterKey()
        {
            var writer = new StringWriter();
            writer.WriteLine("[Test]");
            writer.WriteLine(" option = someValue ; some comment");
            writer.WriteLine("");

            var reader = new IniReader(new StringReader(writer.ToString())) {AcceptCommentAfterKey = true};

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual("someValue", reader.Value);
            Assert.AreEqual("some comment", reader.Comment);
            Assert.IsTrue(reader.Read());
        }

        [Test]
        public void NoCommentAfterKey()
        {
            var writer = new StringWriter();
            writer.WriteLine("[Test]");
            writer.WriteLine(" option = someValue ; some comment");
            writer.WriteLine("");

            var reader = new IniReader(new StringReader(writer.ToString())) {AcceptCommentAfterKey = false};

            Assert.IsTrue(reader.Read());
            Assert.IsTrue(reader.Read());
            Assert.AreEqual("someValue ; some comment", reader.Value);
            Assert.IsTrue(reader.Read());
        }

        [Test]
        public void GetAndSetDelimiters()
        {
            var writer = new StringWriter();
            writer.WriteLine("[Test]");
            writer.WriteLine(" option = someValue ; some comment");

            var reader = new IniReader(new StringReader(writer.ToString()));

            Assert.AreEqual('=', reader.GetAssignDelimiters()[0]);
            reader.SetAssignDelimiters(new [] {':', '='});
            Assert.AreEqual(':', reader.GetAssignDelimiters()[0]);
            Assert.AreEqual('=', reader.GetAssignDelimiters()[1]);

            Assert.AreEqual(';', reader.GetCommentDelimiters()[0]);
            reader.SetCommentDelimiters(new [] {'#', ';'});
            Assert.AreEqual('#', reader.GetCommentDelimiters()[0]);
            Assert.AreEqual(';', reader.GetCommentDelimiters()[1]);
        }

        #endregion

        #region Private methods

        private static string ConvertToUnix(string text)
        {
            return text.Replace("\r\n", "\n");
        }

        #endregion
    }
}