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
using CouchbaseModelViews.Framework.Attributes;

namespace CouchbaseModelViews.DemoModels
{
	[CouchbaseDesignDoc("breweries")]
	[CouchbaseAllView]
	public class Brewery 
	{
		public string Id { get; set; }
		
		[CouchbaseViewKey("by_name", "name")]
		public string Name { get; set; }

		[CouchbaseViewKeySum("sum_by_state", "state")]
		[CouchbaseViewKeyCount("count_by_state", "state", Order=0)]
		public string State { get; set; }

		[CouchbaseViewKeyCount("count_by_state", "city", Order = 1)]
		public string City { get; set; }

		public string Description { get; set; }

		[CouchbaseSpatialViewKey("by_location", "geo.lng", 0)]
		public float Longitude { get; set; }

		[CouchbaseSpatialViewKey("by_location", "geo.lat", 1)]
		public float Latitude { get; set; }
	}
}
