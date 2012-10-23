using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using System.Collections;
using CouchbaseModelViews.Framework;

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
			ddManager.Create(designDocs, (s) => Console.WriteLine("Created {0} design doc", s));
			var runner = new ViewRunner();
			runner.Run(designDocs, (k, v, s) => Console.WriteLine("[{0}::{1}] Key {2}", k, v, s["key"]), 5);
        }
    }
}
