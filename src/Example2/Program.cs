using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using System;
using System.Threading.Tasks;

namespace Example2
{
    public class GeocodeModel
    {
        public ObjectId Id { get; set; }
        public string StreetAddress { get; set; }
        public GeoJsonPoint<GeoJson2DGeographicCoordinates> Location { get; set; }
    }


    class Program
    {
        private static readonly MongoClient Client = new MongoClient("mongodb://localhost:27017");
        private static readonly IMongoDatabase Database = Client.GetDatabase("GeoTest");
        private static readonly IMongoCollection<GeocodeModel> Collection = Database.GetCollection<GeocodeModel>("Geo");
        static void Main(string[] args)
        {
            //Collection.Indexes.CreateOne("{'Location': '2dsphere'}");

            //Build an index if it is not already built
            IndexKeysDefinition<GeocodeModel> keys = Builders<GeocodeModel>.IndexKeys.Geo2DSphere(x => x.Location);
            //Add an optional name- useful for admin
            var options = new CreateIndexOptions { Name = "Location_Index" };

            var indexModel = new CreateIndexModel<GeocodeModel>(keys, options);
            Collection.Indexes.CreateOne(indexModel);

            var model = new GeocodeModel();
            model.StreetAddress = "天安门";
            model.Location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(116.403119, 39.915156));
            Collection.InsertOne(model);
            var model2 = new GeocodeModel();
            model2.StreetAddress = "地址1";
            model2.Location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(116.36345, 39.913385));
            Collection.InsertOne(model2);

            var model3 = new GeocodeModel();
            model3.StreetAddress = "地址2";
            model3.Location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(116.523276, 39.915156));
            Collection.InsertOne(model3);

            var model4 = new GeocodeModel();
            model4.StreetAddress = "地址3";
            model4.Location = new GeoJsonPoint<GeoJson2DGeographicCoordinates>(new GeoJson2DGeographicCoordinates(116.497405, 40.016907));
            Collection.InsertOne(model4);
       
            var point = GeoJson.Point(GeoJson.Geographic(116.420366, 39.915156));
            var locationQuery = new FilterDefinitionBuilder<GeocodeModel>().Near(tag => tag.Location, point, 5000); //fetch results that are within a 50 metre radius of the point we're searching.
            var query = Collection.Find(locationQuery).Limit(10); //Limit the query to return only the top 10 results.
            var cursor = query.ToCursor();
            foreach (var document in cursor.ToEnumerable())
            {
                Console.WriteLine(document.StreetAddress);
            }
            Console.Read();
        }

    }
}
