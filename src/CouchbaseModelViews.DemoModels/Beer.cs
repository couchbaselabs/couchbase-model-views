using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CouchbaseModelViews.Framework.Attributes;
using Newtonsoft.Json;

namespace CouchbaseModelViews.DemoModels
{
	[CouchbaseDesignDoc("beers")]
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

		[JsonProperty("breweryId")]
		[CouchbaseViewKey("by_brewery", "breweryId")]
		public string Brewery { get; set; }
	}
}
