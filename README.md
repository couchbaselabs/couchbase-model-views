couchbase-model-views
=====================

Sample project demonstrating how to generate Couchbase Views with the CouchbaseClient and custom attributes on data objects

Usage (Models)
=====================

Reference Couchbase.ModelViews.Framework in your models project.  Decorate the class with the name of the design doc.  
Decorate properties with the name of the views to which they should be keys.  Optionally, include the order in which 
the properties should be emitted.  

    [CouchbaseDesignDoc("beers")]
    public class Beer
	  {
  		public string Id { get; set; }
  
  		[CouchbaseViewKey("by_abv_and_name", "name", 1)]
  		[CouchbaseViewKey("by_name", "name")]
  		public string Name { get; set; }
  
  		public string Description { get; set; }
  
  		[CouchbaseViewKey("by_abv_and_name", "abv", 0)]
  		public float ABV { get; set; }
  
  		[CouchbaseViewKey("by_brewery", "breweryId")]
  		public string Brewery { get; set; }
	  }
    
Usage (Console App)
=====================

In app.config, list the assemblies from which to build views (along with the CouchbaseCluster configuration).

    <?xml version="1.0" encoding="utf-8" ?>
    <configuration>
      <configSections>    
        <sectionGroup name="modelViews">
          <section name="assemblies" type="System.Configuration.DictionarySectionHandler"/>      
        </sectionGroup>
        <section name="couchbase" type="Couchbase.Configuration.CouchbaseClientSection, Couchbase"/>
      </configSections>
  
      <modelViews>
        <assemblies>
          <add key="DemoModels" value="CouchbaseModelViews.DemoModels" />
        </assemblies>
      </modelViews>
    
      <couchbase>
        <servers bucket="beer-sample" bucketPassword="">
          <add uri="http://localhost:8091/pools"/>      
        </servers>
      </couchbase>
    </configuration>
    
Run the console application.  The sample Beer class above will produce the following views:

    {
      "views": {
        "by_name_and_abv": {
          "map": "function(doc, meta) { \r\n\t if (doc.name && doc.abv) { \r\n\t\t emit([doc.name, doc.abv], null); \r\n\t } \r\n }"
        },
        "by_name": {
          "map": "function(doc, meta) { \r\n\t if (doc.name) { \r\n\t\t emit(doc.name, null); \r\n\t } \r\n }"
        },
        "by_brewery": {
          "map": "function(doc, meta) { \r\n\t if (doc.breweryId) { \r\n\t\t emit(doc.breweryId, null); \r\n\t } \r\n }"
        }
      }
    }