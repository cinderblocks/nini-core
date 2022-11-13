using System;

namespace Nini.Ini
{
		/// <include file='IniItem.xml' path='//Class[@name="IniItem"]/docs/*' />
		public class IniItem
		{
			#region Private variables

            #endregion
			
			#region Public properties
			/// <include file='IniItem.xml' path='//Property[@name="Type"]/docs/*' />
			public IniType Type { get; set; } = IniType.Empty;

            /// <include file='IniItem.xml' path='//Property[@name="Value"]/docs/*' />
			public string Value { get; set; } = "";

            /// <include file='IniItem.xml' path='//Property[@name="Name"]/docs/*' />
			public string Name { get; } = "";

            /// <include file='IniItem.xml' path='//Property[@name="Comment"]/docs/*' />
			public string Comment { get; set; } = null;

            #endregion
			
			/// <include file='IniItem.xml' path='//Constructor[@name="Constructor"]/docs/*' />
			protected internal IniItem (string name, string value, IniType type, string comment)
			{
				Name = name;
				Value = value;
				Type = type;
				Comment = comment;
			}
		}
}

