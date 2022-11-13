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
	public class IniDocumentTests
	{
		[Test]
		public void GetSection ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("; Test");
			writer.WriteLine ("[Nini Thing]");
			var doc = new IniDocument (new StringReader (writer.ToString ()));
			
			Assert.AreEqual (1, doc.Sections.Count);
			Assert.AreEqual ("Nini Thing", doc.Sections["Nini Thing"].Name);
			Assert.AreEqual ("Nini Thing", doc.Sections[0].Name);
			Assert.IsNull (doc.Sections["Non Existant"]);
		}
		
		[Test]
		public void GetKey ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[Nini]");
			writer.WriteLine (" my key = something");
			var doc = new IniDocument (new StringReader (writer.ToString ()));
			
			var section = doc.Sections["Nini"];
			Assert.IsTrue (section.Contains ("my key"));
			Assert.AreEqual ("something", section.GetValue ("my key"));
			Assert.IsFalse (section.Contains ("not here"));
		}

		[Test]
		public void SetSection ()
		{
			var doc = new IniDocument ();

			var section = new IniSection ("new section");
			doc.Sections.Add (section);
			Assert.AreEqual ("new section", doc.Sections[0].Name);
			Assert.AreEqual ("new section", doc.Sections["new section"].Name);
			
			section = new IniSection ("a section", "a comment");
			doc.Sections.Add (section);
			Assert.AreEqual ("a comment", doc.Sections[1].Comment);
		}

		[Test]
		public void SetKey ()
		{
			var doc = new IniDocument ();
			
			var section = new IniSection ("new section");
			doc.Sections.Add (section);

			section.Set ("new key", "some value");
			
			Assert.IsTrue (section.Contains ("new key"));
			Assert.AreEqual ("some value", section.GetValue ("new key"));
		}

		[Test]
		public void ParserError ()
		{
		    Assert.Throws<IniException>(() =>
		    {
		        var writer = new StringWriter();
		        writer.WriteLine("[Nini Thing");
		        writer.WriteLine(" my key = something");
                var doc = new IniDocument(new StringReader(writer.ToString()));
		    });
		    
		}

		[Test]
		public void RemoveSection ()
		{
		    var writer = new StringWriter ();
			writer.WriteLine ("[Nini Thing]");
			writer.WriteLine (" my key = something");
			writer.WriteLine ("[Parser]");
		    var doc = new IniDocument (new StringReader (writer.ToString ()));
			
			Assert.IsNotNull (doc.Sections["Nini Thing"]);
			doc.Sections.Remove ("Nini Thing");
			Assert.IsNull (doc.Sections["Nini Thing"]);
		}

		[Test]
		public void RemoveKey ()
		{
		    var writer = new StringWriter ();
			writer.WriteLine ("[Nini]");
			writer.WriteLine (" my key = something");
		    var doc = new IniDocument (new StringReader (writer.ToString ()));
			
			Assert.IsTrue (doc.Sections["Nini"].Contains ("my key"));
			doc.Sections["Nini"].Remove ("my key");
			Assert.IsFalse (doc.Sections["Nini"].Contains ("my key"));
		}

		[Test]
		public void GetAllKeys ()
		{
		    var writer = new StringWriter ();
			writer.WriteLine ("[Nini]");
			writer.WriteLine (" ; a comment");
			writer.WriteLine (" my key = something");
			writer.WriteLine (" dog = rover");
			writer.WriteLine (" cat = muffy");
		    var doc = new IniDocument (new StringReader (writer.ToString ()));
			
			var section = doc.Sections["Nini"];
			
			Assert.AreEqual (4, section.ItemCount);
			Assert.AreEqual (3, section.GetKeys ().Length);
			Assert.AreEqual ("my key", section.GetKeys ()[0]);
			Assert.AreEqual ("dog", section.GetKeys ()[1]);
			Assert.AreEqual ("cat", section.GetKeys ()[2]);
		}

		[Test]
		public void SaveDocumentWithComments ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("; some comment");
			writer.WriteLine (""); // empty line
			writer.WriteLine ("[new section]");
			writer.WriteLine (" dog = rover");
			writer.WriteLine (""); // Empty line
			writer.WriteLine ("; a comment");
			writer.WriteLine (" cat = muffy");
			IniDocument doc = new IniDocument (new StringReader (writer.ToString ()));
			
			StringWriter newWriter = new StringWriter ();
			doc.Save (newWriter);

			StringReader reader = new StringReader (newWriter.ToString ());
			Assert.AreEqual ("; some comment", reader.ReadLine ());
			Assert.AreEqual ("", reader.ReadLine ());
			Assert.AreEqual ("[new section]", reader.ReadLine ());
			Assert.AreEqual ("dog = rover", reader.ReadLine ());
			Assert.AreEqual ("", reader.ReadLine ());
			Assert.AreEqual ("; a comment", reader.ReadLine ());
			Assert.AreEqual ("cat = muffy", reader.ReadLine ());
			
			writer.Close ();
		}

		[Test]
		public void SaveToStream ()
		{
			const string filePath = "SaveToStream.ini";
			var stream = new FileStream (filePath, FileMode.Create);

			// Create a new document and save to stream
			var doc = new IniDocument ();
			var section = new IniSection ("Pets");
			section.Set ("dog", "rover");
			section.Set ("cat", "muffy");
			doc.Sections.Add (section);
			doc.Save (stream);
			stream.Close ();

			var newDoc = new IniDocument (new FileStream (filePath, 
																  FileMode.Open));
			section = newDoc.Sections["Pets"];
			Assert.IsNotNull (section);
			Assert.AreEqual (2, section.GetKeys ().Length);
			Assert.AreEqual ("rover", section.GetValue ("dog"));
			Assert.AreEqual ("muffy", section.GetValue ("cat"));
			
			stream.Close ();

			File.Delete (filePath);
		}

		[Test]
		public void SambaStyleDocument ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("; some comment");
			writer.WriteLine ("# another comment"); // empty line
			writer.WriteLine ("[test]");
			writer.WriteLine (" cat = cats are not tall\\ ");
			writer.WriteLine (" animals ");
			writer.WriteLine (" dog = dogs \\ ");
			writer.WriteLine ("        do not eat cats ");
			var doc = new IniDocument (new StringReader (writer.ToString ()),
												IniFileType.SambaStyle);

			Assert.AreEqual ("cats are not tall animals",
							doc.Sections["test"].GetValue ("cat"));
			Assert.AreEqual ("dogs         do not eat cats",
							doc.Sections["test"].GetValue ("dog"));
		}

		[Test]
		public void PythonStyleDocument ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("; some comment");
			writer.WriteLine ("# another comment"); // empty line
			writer.WriteLine ("[test]");
			writer.WriteLine (" cat: cats are not tall animals ");
			writer.WriteLine (" dog : dogs bark");
			var doc = new IniDocument (new StringReader (writer.ToString ()),
												IniFileType.PythonStyle);

			Assert.AreEqual ("cats are not tall animals",
							doc.Sections["test"].GetValue ("cat"));
			Assert.AreEqual ("dogs bark", doc.Sections["test"].GetValue ("dog"));
		}

		[Test]
		public void DuplicateSections ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" my key = something");
			writer.WriteLine ("[Test]");
			writer.WriteLine (" another key = something else");
			writer.WriteLine ("[Test]");
			writer.WriteLine (" value 0 = something 0");
			writer.WriteLine (" value 1 = something 1");
			var doc = new IniDocument (new StringReader (writer.ToString ()));
			
			Assert.IsNotNull (doc.Sections["Test"]);
			Assert.AreEqual (1, doc.Sections.Count);
			Assert.AreEqual (2, doc.Sections["Test"].ItemCount);
			Assert.IsNull (doc.Sections["Test"].GetValue ("my key"));
			Assert.IsNotNull (doc.Sections["Test"].GetValue ("value 0"));
			Assert.IsNotNull (doc.Sections["Test"].GetValue ("value 1"));
		}

		[Test]
		public void DuplicateKeys ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" a value = something 0");
			writer.WriteLine (" a value = something 1");
			writer.WriteLine (" a value = something 2");
			var doc = new IniDocument (new StringReader (writer.ToString ()));
			
			Assert.IsNotNull (doc.Sections["Test"]);
			Assert.AreEqual (1, doc.Sections.Count);
			Assert.AreEqual (1, doc.Sections["Test"].ItemCount);
			Assert.IsNotNull (doc.Sections["Test"].GetValue ("a value"));
			Assert.AreEqual ("something 0", doc.Sections["Test"].GetValue ("a value"));
		}

		[Test]
		public void MysqlStyleDocument ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("# another comment"); // empty line
			writer.WriteLine ("[test]");
			writer.WriteLine (" quick ");
			writer.WriteLine (" cat = cats are not tall animals ");
			writer.WriteLine (" dog : dogs bark");
			var doc = new IniDocument (new StringReader (writer.ToString ()),
												IniFileType.MysqlStyle);

			Assert.IsTrue (doc.Sections["test"].Contains ("quick"));
			Assert.AreEqual ("", doc.Sections["test"].GetValue ("quick"));
			Assert.AreEqual ("cats are not tall animals",
							doc.Sections["test"].GetValue ("cat"));
			Assert.AreEqual ("dogs bark", doc.Sections["test"].GetValue ("dog"));
		}

		[Test]
		public void WindowsStyleDocument ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("; another comment"); // empty line
			writer.WriteLine ("[test]");
			writer.WriteLine (" cat = cats are not ; tall ");
			writer.WriteLine (" dog = dogs \"bark\"");
			var doc = new IniDocument (new StringReader (writer.ToString ()),
												IniFileType.WindowsStyle);

			var section = doc.Sections["test"];
			Assert.AreEqual ("cats are not ; tall", section.GetValue ("cat"));
			Assert.AreEqual ("dogs \"bark\"", section.GetValue ("dog"));
		}

		[Test]
		public void SaveAsPythonStyle ()
		{
			const string filePath = "Save.ini";
			var stream = new FileStream (filePath, FileMode.Create);

			// Create a new document and save to stream
		    var doc = new IniDocument {FileType = IniFileType.PythonStyle};
		    var section = new IniSection ("Pets");
			section.Set ("my comment");
			section.Set ("dog", "rover");
			doc.Sections.Add (section);
			doc.Save (stream);
			stream.Close ();

			var writer = new StringWriter ();
			writer.WriteLine ("[Pets]");
			writer.WriteLine ("# my comment");
			writer.WriteLine ("dog : rover");

			var reader = new StreamReader (filePath);
			Assert.AreEqual (writer.ToString (), reader.ReadToEnd ());
			reader.Close ();

			File.Delete (filePath);
		}

		[Test]
		public void SaveAsMysqlStyle ()
		{
			const string filePath = "Save.ini";
			var stream = new FileStream (filePath, FileMode.Create);

			// Create a new document and save to stream
		    var doc = new IniDocument {FileType = IniFileType.MysqlStyle};
		    var section = new IniSection ("Pets");
			section.Set ("my comment");
			section.Set ("dog", "rover");
			doc.Sections.Add (section);
			doc.Save (stream);
			stream.Close ();

			var writer = new StringWriter ();
			writer.WriteLine ("[Pets]");
			writer.WriteLine ("# my comment");
			writer.WriteLine ("dog = rover");

			var reader = new StreamReader (filePath);
			Assert.AreEqual (writer.ToString (), reader.ReadToEnd ());
			reader.Close ();

		    var iniDoc = new IniDocument {FileType = IniFileType.MysqlStyle};
		    iniDoc.Load (filePath);

			File.Delete (filePath);
		}

		[Test]
		public void SambaLoadAsStandard ()
		{
		    IniDocument doc = new IniDocument();
		    Assert.Throws<IniException>(() =>
		    {
		        var filePath = "Save.ini";
		        var stream = new FileStream(filePath, FileMode.Create);

		        // Create a new document and save to stream
		        doc.FileType = IniFileType.SambaStyle;
		        var section = new IniSection("Pets");
		        section.Set("my comment");
		        section.Set("dog", "rover");
		        doc.Sections.Add(section);
		        doc.Save(stream);
		        stream.Close();

		        var iniDoc = new IniDocument {FileType = IniFileType.Standard};
		        iniDoc.Load(filePath);

		        File.Delete(filePath);
            });
		}
	}
}