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
using Newtonsoft.Json;

namespace CouchbaseModelViews.DemoModels
{
	[CouchbaseDesignDoc("beers", "beer")]
	public class Beer
	{
		public string Id { get; set; }

		[JsonProperty("name")]
		[CouchbaseViewKey("by_abv_and_name", "name", 1)]
		[CouchbaseViewKey("by_name", "name")]
		public string Name { get; set; }

		[JsonProperty("description")]
		public string Description { get; set; }

		[JsonProperty("abv")]
		[CouchbaseViewKey("by_abv_and_name", "abv", 0)]
		public float ABV { get; set; }

		[JsonProperty("brewery_id")]
		[CouchbaseViewKey("by_brewery", "brewery_id")]
		public string Brewery { get; set; }
	}
}
