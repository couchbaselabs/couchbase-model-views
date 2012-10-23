using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections;
using System.Reflection;

namespace CouchbaseModelViews.Framework
{
    public static class ConfigParser
    {
        public static IEnumerable<Assembly> GetAssemblies()
        {
            var assemblies = ConfigurationManager.GetSection("modelViews/assemblies") as IDictionary;

            foreach (DictionaryEntry ass in assemblies)
            {
                yield return Assembly.Load(ass.Value as string);
            }
        }
    }
}
