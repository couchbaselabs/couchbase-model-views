using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CouchbaseModelViews.Framework
{
	public static class ViewTemplates
	{
		public const string VIEW_MAP_TEMPLATE =
@"function(doc, meta) {{
	if ({0}) {{
		emit({1}, {2});
	}}
}}";
		public const string COLLATED_VIEW_MAP_TEMPLATE =
@"function(doc, meta) {{
	switch(doc.type) {{
		case ""{0}"":
			emit([meta.id, 0]);
			break;
		case ""{1}"":
			if ({2}) {{
				emit([{3}, 1], null);
			}}
	}}
}}";
		public const string SPATIAL_VIEW_MAP_TEMPLATE =
@"function (doc, meta) {{
	if ({0}) {{
		emit({{ ""type"": ""Point"", ""coordinates"": {1}}}, null);
	}}
}}";

	}
}
