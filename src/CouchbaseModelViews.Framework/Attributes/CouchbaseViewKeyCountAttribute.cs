using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CouchbaseModelViews.Framework.Attributes
{
	public class CouchbaseViewKeyCountAttribute : CouchbaseViewKeyReduceAttributeBase
	{
		public CouchbaseViewKeyCountAttribute(string viewName, string propertyName = "", int order = 0, string value = "1")
			: base(viewName, propertyName, order, value) {}
	}
}
