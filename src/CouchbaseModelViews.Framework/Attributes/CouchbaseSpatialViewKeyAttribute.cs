using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CouchbaseModelViews.Framework.Attributes
{
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
	public class CouchbaseSpatialViewKeyAttribute : CouchbaseViewKeyAttributeBase
	{
		public CouchbaseSpatialViewKeyAttribute(string viewName, string propertyName = "", int order = 0)
		{
			PropertyName = propertyName;
			ViewName = viewName;
			Order = order;
		}
	}
}
