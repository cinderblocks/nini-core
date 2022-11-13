#region Copyright
//
// Nini Configuration Project.
// Copyright (C) 2006 Brent R. Matzelle.  All rights reserved.
//
// This software is published under the terms of the MIT X11 license, a copy of 
// which has been included with this distribution in the LICENSE.txt file.
// 
#endregion

using System;
using System.IO;
using Nini.Config;
using NUnit.Framework;

namespace Nini.Test.Config
{
	[TestFixture]
	public class ConfigBaseTests
	{
		[Test]
		public void GetConfig ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[Pets]");
			writer.WriteLine (" cat = muffy");
			writer.WriteLine (" dog = rover");
			writer.WriteLine (" bird = tweety");
			var source = new IniConfigSource 
									(new StringReader (writer.ToString ()));
			
			var config = source.Configs["Pets"];
			Assert.AreEqual ("Pets", config.Name);
			Assert.AreEqual (3, config.GetKeys ().Length);
			Assert.AreEqual (source, config.ConfigSource);
		}
		
		[Test]
		public void GetString ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" cat = muffy");
			writer.WriteLine (" dog = rover");
			writer.WriteLine (" bird = tweety");
			var source = 
				new IniConfigSource (new StringReader (writer.ToString ()));
			var config = source.Configs["Test"];
			
			Assert.AreEqual ("muffy", config.Get ("cat"));
			Assert.AreEqual ("rover", config.Get ("dog"));
			Assert.AreEqual ("muffy", config.GetString ("cat"));
			Assert.AreEqual ("rover", config.GetString ("dog"));
			Assert.AreEqual ("my default", config.Get ("Not Here", "my default"));
			Assert.IsNull (config.Get ("Not Here 2"));
		}
		
		[Test]
		public void GetInt ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" value 1 = 49588");
			var source = new IniConfigSource 
									(new StringReader (writer.ToString ()));
			var config = source.Configs["Test"];
			
			Assert.AreEqual (49588, config.GetInt ("value 1"));
			Assert.AreEqual (12345, config.GetInt ("Not Here", 12345));
			
			try
			{
				config.GetInt ("Not Here Also");
				Assert.Fail ();
			}
			catch
			{
			}
		}
		
		[Test]
		public void GetLong ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" value 1 = 4000000000");
			var source = new IniConfigSource 
										(new StringReader (writer.ToString ()));
			var config = source.Configs["Test"];
			
			Assert.AreEqual (4000000000, config.GetLong ("value 1"));
			Assert.AreEqual (5000000000, config.GetLong ("Not Here", 5000000000));
			
			try
			{
				config.GetLong ("Not Here Also");
				Assert.Fail ();
			}
			catch
			{
			}
		}
		
		[Test]
		public void GetFloat ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" value 1 = 494.59");
			var source = new IniConfigSource (new StringReader (writer.ToString ()));
			var config = source.Configs["Test"];
			
			Assert.AreEqual ((float)494.59, config.GetFloat ("value 1"));
			Assert.AreEqual ((float)5656.2853, config.GetFloat ("Not Here", (float)5656.2853));
		}

		[Test]
		public void BooleanAlias ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" bool 1 = TrUe");
			writer.WriteLine (" bool 2 = FalSe");
			writer.WriteLine (" bool 3 = ON");
			writer.WriteLine (" bool 4 = OfF");
			var source = new IniConfigSource 
									(new StringReader (writer.ToString ()));

			var config = source.Configs["Test"];
			config.Alias.AddAlias ("true", true);
			config.Alias.AddAlias ("false", false);
			config.Alias.AddAlias ("on", true);
			config.Alias.AddAlias ("off", false);
			
			Assert.IsTrue (config.GetBoolean ("bool 1"));
			Assert.IsFalse (config.GetBoolean ("bool 2"));
			Assert.IsTrue (config.GetBoolean ("bool 3"));
			Assert.IsFalse (config.GetBoolean ("bool 4"));
			Assert.IsTrue (config.GetBoolean ("Not Here", true));
		}
		
		[Test]
        public void BooleanAliasNoDefault ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" bool 1 = TrUe");
			writer.WriteLine (" bool 2 = FalSe");
			var source = new IniConfigSource (new StringReader (writer.ToString ()));
			
			var config = source.Configs["Test"];
			config.Alias.AddAlias ("true", true);
			config.Alias.AddAlias ("false", false);
			
			Assert.IsTrue (config.GetBoolean ("Not Here", true));

			Assert.Throws<ArgumentException>(() => config.GetBoolean ("Not Here Also"));
		}
		
		[Test]
		public void NonBooleanParameter ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" bool 1 = not boolean");
			var source = new IniConfigSource (new StringReader (writer.ToString ()));

			var config = source.Configs["Test"];
			config.Alias.AddAlias ("true", true);
			config.Alias.AddAlias ("false", false);
			
			Assert.Throws<ArgumentException>(() => config.GetBoolean ("bool 1"));
		}
		
		[Test]
		public void GetIntAlias ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" node type = TEXT");
			writer.WriteLine (" error code = WARN");
			var source = new IniConfigSource (new StringReader (writer.ToString ()));
			
			const int WARN = 100, ERROR = 200;
			var config = source.Configs["Test"];
			config.Alias.AddAlias ("error code", "waRn", WARN);
			config.Alias.AddAlias ("error code", "eRRor", ERROR);
			config.Alias.AddAlias ("node type", new System.Xml.XmlNodeType ());
			config.Alias.AddAlias ("default", "age", 31);
			
			Assert.AreEqual (WARN, config.GetInt ("error code", true));
			Assert.AreEqual ((int)System.Xml.XmlNodeType.Text, 
							 config.GetInt ("node type", true));
			Assert.AreEqual (31, config.GetInt ("default", 31, true));
		}
		
		[Test]
		public void GetKeys ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" bool 1 = TrUe");
			writer.WriteLine (" bool 2 = FalSe");
			writer.WriteLine (" bool 3 = ON");
			var source = new IniConfigSource (new StringReader (writer.ToString ()));
			
			var config = source.Configs["Test"];
			Assert.AreEqual (3, config.GetKeys ().Length);
			Assert.AreEqual ("bool 1", config.GetKeys ()[0]);
			Assert.AreEqual ("bool 2", config.GetKeys ()[1]);
			Assert.AreEqual ("bool 3", config.GetKeys ()[2]);
		}

		[Test]
		public void GetValues ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[Test]");
			writer.WriteLine (" key 1 = value 1");
			writer.WriteLine (" key 2 = value 2");
			writer.WriteLine (" key 3 = value 3");
			var source = 
					new IniConfigSource (new StringReader (writer.ToString ()));
			
			var config = source.Configs["Test"];
			Assert.AreEqual (3, config.GetValues ().Length);
			Assert.AreEqual ("value 1", config.GetValues ()[0]);
			Assert.AreEqual ("value 2", config.GetValues ()[1]);
			Assert.AreEqual ("value 3", config.GetValues ()[2]);
		}
		
		[Test]
		public void SetAndRemove ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[Pets]");
			writer.WriteLine (" cat = muffy");
			writer.WriteLine (" dog = rover");
			writer.WriteLine (" bird = tweety");
			var source = new IniConfigSource 
									(new StringReader (writer.ToString ()));
			
			var config = source.Configs["Pets"];
			Assert.AreEqual ("Pets", config.Name);
			Assert.AreEqual (3, config.GetKeys ().Length);
			
			config.Set ("snake", "cobra");
			Assert.AreEqual (4, config.GetKeys ().Length);

			// Test removing			
			Assert.IsNotNull (config.Get ("dog"));
			config.Remove ("dog");
			Assert.AreEqual (3, config.GetKeys ().Length);
			Assert.IsNull (config.Get ("dog"));
			Assert.IsNotNull (config.Get ("snake"));
		}

		[Test]
		public void Rename ()
		{
			var source = new IniConfigSource ();

			var config = source.AddConfig ("Pets");
			config.Set ("cat", "Muffy");
			config.Set ("dog", "Rover");

			config.Name = "MyPets";
			Assert.AreEqual ("MyPets", config.Name);
			
			Assert.IsNull (source.Configs["Pets"]);
			var newConfig = source.Configs["MyPets"];

			Assert.AreEqual (config, newConfig);

			Assert.AreEqual (2, newConfig.GetKeys ().Length);
		}

		[Test]
		public void Contains ()
		{
			var source = new IniConfigSource ();

			var config = source.AddConfig ("Pets");
			config.Set ("cat", "Muffy");
			config.Set ("dog", "Rover");

			Assert.IsTrue (config.Contains ("cat"));
			Assert.IsTrue (config.Contains ("dog"));

			config.Remove ("cat");
			Assert.IsFalse (config.Contains ("cat"));
			Assert.IsTrue (config.Contains ("dog"));
		}

		[Test]
		public void ExpandString ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[web]");
			writer.WriteLine (" apache = Apache implements ${protocol}");
			writer.WriteLine (" protocol = http");
			writer.WriteLine ("[server]");
			writer.WriteLine (" domain = ${web|protocol}://nini.sf.net/");
			var source = new IniConfigSource 
									(new StringReader (writer.ToString ()));

			var config = source.Configs["web"];
			Assert.AreEqual ("http", config.Get ("protocol"));
			Assert.AreEqual ("Apache implements ${protocol}", config.Get ("apache"));
			Assert.AreEqual ("Apache implements http", config.GetExpanded ("apache"));
			Assert.AreEqual ("Apache implements ${protocol}", config.Get ("apache"));
			config = source.Configs["server"];
			Assert.AreEqual ("http://nini.sf.net/", config.GetExpanded ("domain"));
			Assert.AreEqual ("${web|protocol}://nini.sf.net/", config.Get ("domain"));
		}

		[Test]
		public void ExpandWithEndBracket ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[web]");
			writer.WriteLine (" apache = } Apache implements ${protocol}");
			writer.WriteLine (" protocol = http");
			var source = new IniConfigSource 
									(new StringReader (writer.ToString ()));

			var config = source.Configs["web"];
			Assert.AreEqual ("} Apache implements http", config.GetExpanded ("apache"));
		}

		[Test]
		public void ExpandBackToBack ()
		{
			var writer = new StringWriter ();
			writer.WriteLine ("[web]");
			writer.WriteLine (" apache = Protocol: ${protocol}${version}");
			writer.WriteLine (" protocol = http");
			writer.WriteLine (" version = 1.1");
			var source = new IniConfigSource 
									(new StringReader (writer.ToString ()));

			var config = source.Configs["web"];
			Assert.AreEqual ("Protocol: http1.1", config.GetExpanded ("apache"));
		}
	}
}