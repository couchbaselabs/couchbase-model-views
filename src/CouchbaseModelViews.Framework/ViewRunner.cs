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
using Couchbase.Configuration;
using Couchbase;
using System.Configuration;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CouchbaseModelViews.Framework
{
	public class ViewRunner
	{
		private static CouchbaseClientSection _config;
		private static CouchbaseClient _client;
		
		public ViewRunner(string sectionName = "couchbase")
		{
			if (_client == null)
			{
				_config = ConfigurationManager.GetSection(sectionName) as CouchbaseClientSection;
				_client = new CouchbaseClient(_config);
			}
		}

		public ViewRunner(CouchbaseClientConfiguration config)
		{
			if (_client == null)
			{
				_client = new CouchbaseClient(config);
			}
		}

		public void Run(IDictionary<string, string> designDocs, Action<string, string, IDictionary<string, object>> rowHandler, int limit = 10)
		{
			foreach (var key in designDocs.Keys)
			{
				var jObj = JsonConvert.DeserializeObject(designDocs[key]) as JObject;
				var views = jObj["views"] as JObject;

				foreach (var view in views)
				{
					var v = _client.GetView(key, view.Key);
					foreach (var item in v.Limit(limit))
					{
						rowHandler(key, view.Key, item.Info);
					}
				}
			}
		}
	}
}
