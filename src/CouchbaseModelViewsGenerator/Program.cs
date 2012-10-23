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
