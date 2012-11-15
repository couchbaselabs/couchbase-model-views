#region [ License information          ]
/* ************************************************************
 * 
 *    @author Couchbase <info@couchbase.com>
 *    @copyright 2012 Couchbase, Inc.
 *    
 *    Licensed under the Apache License, Version 2.0 (the "License");
 *    you may not use this file except in compliance with the License.
 *    You may obtain a copy of the License at
 *    
 *        http://www.apache.org/licenses/LICENSE-2.0
 *    
 *    Unless required by applicable law or agreed to in writing, software
 *    distributed under the License is distributed on an "AS IS" BASIS,
 *    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *    See the License for the specific language governing permissions and
 *    limitations under the License.
 *    
 * ************************************************************/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CouchbaseModelViews.Framework.Attributes;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace CouchbaseModelViews.Framework
{
	public class ViewBuilder
	{
		private const string VIEW_MAP_TEMPLATE = "function(doc, meta) {{ \r\n\t if ({0}) {{ \r\n\t\t emit({1}, null); \r\n\t }} \r\n }}";
		private const string SPATIAL_VIEW_MAP_TEMPLATE = "function (doc, meta) {{ if ({0}) {{ emit({{ \"type\": \"Point\", \"coordinates\": {1}}}, null); }} }}";

		private IList<Assembly> _assemblies = new List<Assembly>();
		private IDictionary<string, string> _designDocs = new Dictionary<string, string>();

		public void AddAssembly(Assembly assembly)
		{
			_assemblies.Add(assembly);
		}

		public void AddAssemblies(IList<Assembly> assemblies)
		{
			_assemblies = assemblies;
		}

		public IDictionary<string, string> Build()
		{
			buildTypes();
			return _designDocs;
		}

		private void buildTypes()
		{
			foreach (var assembly in _assemblies)
			{
				foreach (var type in assembly.GetTypes())
				{
					var designDocDefinition = new DesignDocDefinition();

					var designDoc = "";
					var typeName = "";

					//design doc definition
					var designDocAttr = type.GetCustomAttributes(true).Where(a => a is CouchbaseDesignDocAttribute).FirstOrDefault() as CouchbaseDesignDocAttribute;

					if (designDocAttr != null)
					{
						designDoc = string.IsNullOrEmpty(designDocAttr.Name) ? type.Name.ToLower() : designDocAttr.Name;
						typeName = string.IsNullOrEmpty(designDocAttr.Type) ? type.Name.ToLower() : designDocAttr.Type;

						designDocDefinition.Name = designDoc;
						designDocDefinition.Type = typeName;
					}
					else
					{
						continue;
					}

					//get all view definition
					designDocDefinition.ShouldIncludeAllView = type.GetCustomAttributes(true).Where(a => a is CouchbaseAllView).FirstOrDefault() != null;

					handleViews<CouchbaseViewKeyAttribute>(typeName, designDoc, type, designDocDefinition);

					handleViews<CouchbaseSpatialViewKeyAttribute>(typeName, designDoc, type, designDocDefinition);

					buildJson(designDocDefinition);
				}
			}
		}

		private void handleViews<T>(string typeName, string designDoc, Type type, DesignDocDefinition designDocDefinition) 
			where T : CouchbaseViewKeyAttributeBase
		{
			//spaial views
			var key = Tuple.Create(typeName, designDoc);
			var orderedViewNames = new List<Tuple<string, T>>();

			foreach (var prop in type.GetProperties())
			{
				foreach (T attr in prop.GetCustomAttributes(typeof(T), true))
				{
					var propName = string.IsNullOrEmpty(attr.PropertyName) ? prop.Name : attr.PropertyName;
					orderedViewNames.Add(Tuple.Create(propName, attr));

					if (typeof(T).IsAssignableFrom(typeof(CouchbaseViewKeyAttribute)))
					{
						if (designDocDefinition.Views.FirstOrDefault(v => v.Name == attr.ViewName) == null)
						{
							designDocDefinition.Views.Add(new ViewDefinition() { Name = attr.ViewName });
						}
					}
					else if (typeof(T).IsAssignableFrom(typeof(CouchbaseSpatialViewKeyAttribute)))
					{
						if (designDocDefinition.SpatialViews.FirstOrDefault(v => v.Name == attr.ViewName) == null)
						{
							designDocDefinition.SpatialViews.Add(new SpatialViewDefinition() { Name = attr.ViewName });
						}
					}
				}
			}

			foreach (var attr in orderedViewNames.OrderBy(a => a.Item2.ViewName).ThenBy(a => a.Item2.Order))
			{
				if (typeof(T).IsAssignableFrom(typeof(CouchbaseViewKeyAttribute)))
				{
					designDocDefinition.Views.FirstOrDefault(v => v.Name == attr.Item2.ViewName).KeyProperties.Add(attr.Item1);
				}
				else if (typeof(T).IsAssignableFrom(typeof(CouchbaseSpatialViewKeyAttribute)))
				{
					designDocDefinition.SpatialViews.FirstOrDefault(v => v.Name == attr.Item2.ViewName).CoordinateProperties.Add(attr.Item1);
				}
			}
		}

		private void buildJson(DesignDocDefinition designDocDefinition)
		{
			var jObject = new JObject();
			jObject["views"] = new JObject();
			if (designDocDefinition.SpatialViews.Count > 0) jObject["spatial"] = new JObject();

			if (designDocDefinition.ShouldIncludeAllView)
			{
				var map = new JObject();
				map["map"] = getViewFunction(designDocDefinition.Type, new List<string> { "null" }, VIEW_MAP_TEMPLATE);
				jObject["views"]["all"] = map;
			}

			foreach (var view in designDocDefinition.Views)
			{
				var map = new JObject();
				map["map"] = getViewFunction(designDocDefinition.Type, view.KeyProperties, VIEW_MAP_TEMPLATE, "doc.");
				jObject["views"][view.Name] = map;
			}

			foreach (var view in designDocDefinition.SpatialViews)
			{
				var func = getViewFunction(designDocDefinition.Type, view.CoordinateProperties, SPATIAL_VIEW_MAP_TEMPLATE, "doc.");
				jObject["spatial"][view.Name] = func;
			}

			_designDocs[designDocDefinition.Name] = jObject.ToString();
		}

		private string getViewFunction(string type, IList<string> values, string template, string docPrefix = null)
		{
			var keysToEmit = "[{0}]";
			var keysToCheck = "{0}";

			if (values.Count == 1)
			{
				keysToCheck = keysToEmit = docPrefix + values[0];
			}
			else
			{
				var keys = string.Join(" && ", values.Select(s => docPrefix + s));
				keysToCheck = string.Format(keysToCheck, keys);

				keys = string.Join(", ", values.Select(s => docPrefix + s));
				keysToEmit = string.Format(keysToEmit, keys);
			}

			var condition = "doc.type == \"" + type + "\"";
			if (docPrefix != null) condition += " && " + keysToCheck;
			return string.Format(template, condition, keysToEmit);
		}
	}
}
