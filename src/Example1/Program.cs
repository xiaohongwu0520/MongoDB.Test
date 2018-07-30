using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GeoJsonObjectModel;
using MongoDB.Driver.GridFS;
using System;
using System.Drawing;
using System.IO;
using System.Linq;

namespace Example1
{
    class Program
    {


        static void Main(string[] args)
        {
            // or use a connection string
            var client = new MongoClient("mongodb://localhost:27017");
            //Get a Database
            var database = client.GetDatabase("foo");
            //Get a Collection
            var collection = database.GetCollection<BsonDocument>("bar");


            //        var document = new BsonDocument
            //                           {
            //{ "name", "MongoDB" },
            //{ "type", "Database" },
            //{ "count", 1 },
            //{ "info", new BsonDocument
            //    {
            //        { "x", 203 },
            //        { "y", 102 }
            //    }}
            //};
            //            collection.InsertOne(document);

            //            var documents = Enumerable.Range(0, 100).Select(i => new BsonDocument("counter", i));
            //            collection.InsertMany(documents);

            //Counting Documents
            var count = collection.CountDocuments(new BsonDocument());
            Console.WriteLine("文档个数：" + count.ToString());

            //Find the First Document in a Collection
            var documentNew = collection.Find(new BsonDocument()).FirstOrDefault();
            Console.WriteLine(documentNew.ToString());

            //Find All Documents in a Collection
            var documents = collection.Find(new BsonDocument()).ToList();
            foreach (var d in documents)
            {
                Console.WriteLine(d);
            }
            //var cursor = collection.Find(new BsonDocument()).ToCursor();
            //foreach (var document in cursor.ToEnumerable())
            //{
            //    Console.WriteLine(document);
            //}
            
            //Get a Single Document with a Filter
            var filter = Builders<BsonDocument>.Filter.Eq("counter", 71);
            var document = collection.Find(filter).First();
            Console.WriteLine(document);
            var filter2 = Builders<BsonDocument>.Filter.Gt("counter", 50);
            var cursor = collection.Find(filter2).ToCursor();
            foreach (var d in cursor.ToEnumerable())
            {
                Console.WriteLine(d);
            }

            //Updating Documents
            var filter6 = Builders<BsonDocument>.Filter.Eq("counter", 10);
            var update6 = Builders<BsonDocument>.Update.Set("counter", 110);
            var result6 = collection.UpdateOne(filter6, update6);

            var filter7 = Builders<BsonDocument>.Filter.Lt("counter", 100);
            var update7 = Builders<BsonDocument>.Update.Inc("counter", 100);
            var result = collection.UpdateMany(filter7, update7);

            if (result.IsModifiedCountAvailable)
            {
                Console.WriteLine(result.ModifiedCount);
            }

            //Deleting Documents
            var filter8 = Builders<BsonDocument>.Filter.Eq("counter", 110);
            collection.DeleteOne(filter8);

            var filter9 = Builders<BsonDocument>.Filter.Gte("counter", 100);
            var result9 = collection.DeleteMany(filter9);
            Console.WriteLine(result9.DeletedCount);

            //Bulk Writes
            var models = new WriteModel<BsonDocument>[]
{
    new InsertOneModel<BsonDocument>(new BsonDocument("_id", 4)),
    new InsertOneModel<BsonDocument>(new BsonDocument("_id", 5)),
    new InsertOneModel<BsonDocument>(new BsonDocument("_id", 6)),
    new UpdateOneModel<BsonDocument>(
        new BsonDocument("_id", 1),
        new BsonDocument("$set", new BsonDocument("x", 2))),
    new DeleteOneModel<BsonDocument>(new BsonDocument("_id", 3)),
    new ReplaceOneModel<BsonDocument>(
        new BsonDocument("_id", 3),
        new BsonDocument("_id", 3).Add("x", 4))
};
            // 1. Ordered bulk operation - order of operation is guaranteed
            collection.BulkWrite(models);

            // 2. Unordered bulk operation - no guarantee of order of operation
            collection.BulkWrite(models, new BulkWriteOptions { IsOrdered = false });

            Console.Read();

            GridFSHelper helper = new GridFSHelper("mongodb://localhost:27017", "GridFSDemo", "Pictures");

            #region 上传图片  

            Image image2 = Image.FromFile("D:\\man.png");
            var stream = image2.ImageToStream();
            ObjectId oid2 = helper.UploadGridFSFromStream("man", stream);
            Console.WriteLine(oid2.ToString());

            Image image = Image.FromFile("D:\\dog.jpg");
            byte[] imgdata = image.ImageToBytes();
            ObjectId oid = helper.UploadGridFSFromBytes("dog", imgdata);
            Console.WriteLine(oid.ToString());

            #endregion

            #region 下载图片  

            //第一种  
            var ms = helper.DownloadToStream(oid2);
            Image.FromStream(ms).Save("d:\\aaa.jpg");

            //第二种  
            byte[] Downdata = helper.DownloadAsByteArray(oid);
            var img = Downdata.BytesToImage();
            img.Save("D:\\qqqq.jpg");

            #endregion

            #region 查找图片  
            GridFSFileInfo gridFsFileInfo = helper.FindFiles("man");
            Console.WriteLine(gridFsFileInfo.Id);
            #endregion

            #region 删除图片  
            //helper.DroppGridFSBucket();  
            #endregion

            Console.ReadKey();
        }
    }
}
