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
					var typesAndViews = new Dictionary<Tuple<string, string>, Dictionary<string, List<string>>>();

					var designDoc = "";
					var typeName = "";
					foreach (CouchbaseDesignDocAttribute attribute in type.GetCustomAttributes(true).Where(a => a is CouchbaseDesignDocAttribute))
					{
						designDoc = string.IsNullOrEmpty(attribute.Name) ? type.Name.ToLower() : attribute.Name;
						typeName = string.IsNullOrEmpty(attribute.Type) ? type.Name.ToLower() : attribute.Type;
						typesAndViews[Tuple.Create(typeName, designDoc)] = new Dictionary<string, List<string>>();
					}

					var key = Tuple.Create(typeName, designDoc);
					var orderedViewNames = new List<Tuple<string, CouchbaseViewKeyAttribute>>();

					foreach (var prop in type.GetProperties())
					{
						foreach (CouchbaseViewKeyAttribute attr in prop.GetCustomAttributes(typeof(CouchbaseViewKeyAttribute), true))
						{
							if (!typesAndViews[key].ContainsKey(attr.ViewName))
							{
								typesAndViews[key][attr.ViewName] = new List<string>();
							}

							var propName = string.IsNullOrEmpty(attr.PropertyName) ? prop.Name : attr.PropertyName;
							orderedViewNames.Add(Tuple.Create(propName, attr));
						}						
					}

					foreach (var attr in orderedViewNames.OrderBy(a => a.Item2.ViewName).ThenBy(a => a.Item2.Order))
					{
						typesAndViews[key][attr.Item2.ViewName].Add(attr.Item1);
					}
					
					buildJson(typesAndViews);
				}
			} 
        }

        private void buildJson(Dictionary<Tuple<string, string>, Dictionary<string, List<string>>> typesAndViews)
        {
            foreach (var type in typesAndViews.Keys)
            {
                var jObject = new JObject();
                jObject["views"] = new JObject();

                foreach (var value in typesAndViews.Values)
                {
                    foreach (var key in value.Keys)
                    {
                        var map = new JObject();
                        map["map"] = getFunction(type.Item1, value[key]);
                        jObject["views"][key] = map;
                    }
                }
                
                _designDocs[type.Item2] = jObject.ToString();
            }
        }

        private string getFunction(string type, List<string> values)
        {
            var template = "function(doc, meta) {{ \r\n\t if ({0}) {{ \r\n\t\t emit({1}, null); \r\n\t }} \r\n }}";

            var keysToEmit = "[{0}]";
			var keysToCheck = "{0}";
            if (values.Count == 1)
            {
                keysToCheck = keysToEmit = "doc." + values[0];
            }
            else
            {
				var keys = string.Join(" && ", values.Select(s => "doc." + s));
				keysToCheck = string.Format(keysToCheck, keys);

				keys = string.Join(", ", values.Select(s => "doc." + s));
				keysToEmit = string.Format(keysToEmit, keys);				
            }

			var condition = "doc.type == \"" + type + "\" && " + keysToCheck;
            return string.Format(template, condition, keysToEmit);
        }
    }
}
