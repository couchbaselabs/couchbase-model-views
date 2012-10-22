using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections;

namespace CouchbaseModelViewsGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            var assemblies = ConfigParser.GetAssemblies();
            var builder = new ViewBuilder();
            builder.AddAssemblies(assemblies.ToList());
            var designDocs = builder.Build();
			var ddManager = new DesignDocManager();
			ddManager.Create(designDocs, (s) => Console.WriteLine(s));			
        }
    }
}
