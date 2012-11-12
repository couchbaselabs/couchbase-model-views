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
		private const string MAP_TEMPLATE = "function(doc, meta) {{ \r\n\t if ({0}) {{ \r\n\t\t emit({1}, null); \r\n\t }} \r\n }}";

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

					foreach (CouchbaseDesignDocAttribute attribute in type.GetCustomAttributes(true).Where(a => a is CouchbaseDesignDocAttribute))
					{
						designDoc = string.IsNullOrEmpty(attribute.Name) ? type.Name.ToLower() : attribute.Name;
						typeName = string.IsNullOrEmpty(attribute.Type) ? type.Name.ToLower() : attribute.Type;

						designDocDefinition.Name = designDoc;
						designDocDefinition.Type = typeName;
						designDocDefinition.Views = new List<ViewDefinition>();
					}

					designDocDefinition.ShouldIncludeAllView = type.GetCustomAttributes(true).Where(a => a is CouchbaseAllView).FirstOrDefault() != null;

					var key = Tuple.Create(typeName, designDoc);
					var orderedViewNames = new List<Tuple<string, CouchbaseViewKeyAttribute>>();

					foreach (var prop in type.GetProperties())
					{
						foreach (CouchbaseViewKeyAttribute attr in prop.GetCustomAttributes(typeof(CouchbaseViewKeyAttribute), true))
						{
							var propName = string.IsNullOrEmpty(attr.PropertyName) ? prop.Name : attr.PropertyName;
							orderedViewNames.Add(Tuple.Create(propName, attr));

							if (designDocDefinition.Views.FirstOrDefault(v => v.Name == attr.ViewName) == null)
							{
								designDocDefinition.Views.Add(new ViewDefinition() { Name = attr.ViewName });
							}
						}						
					}

					foreach (var attr in orderedViewNames.OrderBy(a => a.Item2.ViewName).ThenBy(a => a.Item2.Order))
					{
						designDocDefinition.Views.FirstOrDefault(v => v.Name == attr.Item2.ViewName).KeyProperties.Add(attr.Item1);						
					}

					buildJson(designDocDefinition);
				}
			} 
        }

        private void buildJson(DesignDocDefinition designDocDefinition)
        {
            var jObject = new JObject();
            jObject["views"] = new JObject();

			if (designDocDefinition.ShouldIncludeAllView)
			{
				var map = new JObject();
				map["map"] = getFunction(designDocDefinition.Type, new List<string>{"null"});
				jObject["views"]["all"] = map;
			}

            foreach (var view in designDocDefinition.Views)
            {
                var map = new JObject();
                map["map"] = getFunction(designDocDefinition.Type, view.KeyProperties, "doc.");
                jObject["views"][view.Name] = map;
            }

			_designDocs[designDocDefinition.Name] = jObject.ToString();
        }

        private string getFunction(string type, IList<string> values, string docPrefix = null)
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
            return string.Format(MAP_TEMPLATE, condition, keysToEmit);
        }
    }
}
