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
using Couchbase.Management;
using Couchbase.Configuration;
using System.Configuration;
using System.Net;

namespace CouchbaseModelViews.Framework
{
	public class DesignDocManager
	{
		private static CouchbaseClientSection _config;
		private static CouchbaseCluster _cluster;
		
		public DesignDocManager(string sectionName = "couchbase")
		{
			if (_cluster == null)
			{
				_config = ConfigurationManager.GetSection(sectionName) as CouchbaseClientSection;
				_cluster = new CouchbaseCluster(_config);
			}
		}

		public void Create(string designDocName, string designDoc, Action<string> callback = null)
		{
			var doc = "";

			try
			{
				_cluster.RetrieveDesignDocument(_config.Servers.Bucket, designDocName);
			}
			catch (WebException ex)
			{
				if (!ex.Message.Contains("404")) throw ex;
				//Do nothing on 404
			}
			
			if (!string.IsNullOrEmpty(doc))
			{
				_cluster.DeleteDesignDocument(_config.Servers.Bucket, designDocName);
			}

			_cluster.CreateDesignDocument(_config.Servers.Bucket, designDocName, designDoc);

			if (callback != null) callback(designDocName);
		}

		public void Create(IDictionary<string, string> designDocs, Action<string> callback = null)
		{
			foreach (var key in designDocs.Keys)
			{
				Create(key, designDocs[key], callback);
			}
		}		
	}
}
