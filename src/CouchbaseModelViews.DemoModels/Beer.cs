using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CouchbaseModelViews.Framework.Attributes;

namespace CouchbaseModelViews.DemoModels
{
    [CouchbaseDesignDoc("beers")]
    public class Beer
    {
        public string Id { get; set; }

        [CouchbaseViewKey("by_name_and_abv", "name")]
        [CouchbaseViewKey("by_name", "name")]
        public string Name { get; set; }

        public string Description { get; set; }

        [CouchbaseViewKey("by_name_and_abv", "abv")]
        public float ABV { get; set; }
    }
}
