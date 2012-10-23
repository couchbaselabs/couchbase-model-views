using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CouchbaseModelViews.Framework.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CouchbaseDesignDocAttribute : Attribute
    {
        public string Name { get; set; }

		public string Type { get; set; }

        public CouchbaseDesignDocAttribute(string name, string type = "")
        {
            Name = name;
			Type = type;
        }
    }
}
