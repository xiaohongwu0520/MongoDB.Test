using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using MongoDB.Driver.GridFS;
using System.Linq;

namespace Example1
{
    public class GridFSHelper
    {
        private readonly IMongoClient client;
        private readonly IMongoDatabase database;
        private readonly IMongoCollection<BsonDocument> collection;
        private readonly GridFSBucket bucket;
        private GridFSFileInfo fileInfo;
        private ObjectId oid;

        public GridFSHelper()
            : this(
                ConfigurationManager.AppSettings["mongoQueueUrl"], ConfigurationManager.AppSettings["mongoQueueDb"],
                ConfigurationManager.AppSettings["mongoQueueCollection"])
        {
        }

        public GridFSHelper(string url, string db, string collectionName)
        {
            if (url == null)
            {
                throw new ArgumentNullException("url");
            }
            else
            {
                client = new MongoClient(url);
            }

            if (db == null)
            {
                throw new ArgumentNullException("db");
            }
            else
            {
                database = client.GetDatabase(db);
            }

            if (collectionName == null)
            {
                throw new ArgumentNullException("collectionName");
            }
            else
            {
                collection = database.GetCollection<BsonDocument>(collectionName);
            }

            //this.collection = new MongoClient(url).GetDatabase(db).GetCollection<BsonDocument>(collectionName);  

            GridFSBucketOptions gfbOptions = new GridFSBucketOptions()
            {
                BucketName = "bird",
                ChunkSizeBytes = 1 * 1024 * 1024,
                ReadConcern = null,
                ReadPreference = null,
                WriteConcern = null
            };
            var bucket = new GridFSBucket(database, new GridFSBucketOptions
            {
                BucketName = "videos",
                ChunkSizeBytes = 1048576, // 1MB  
                WriteConcern = WriteConcern.WMajority,
                ReadPreference = ReadPreference.Secondary
            });
            this.bucket = new GridFSBucket(database, null);
        }

        public GridFSHelper(IMongoCollection<BsonDocument> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            this.collection = collection;
            this.bucket = new GridFSBucket(collection.Database);
        }


        public ObjectId UploadGridFSFromBytes(string filename, Byte[] source)
        {
            oid = bucket.UploadFromBytes(filename, source);
            return oid;
        }

        public ObjectId UploadGridFSFromStream(string filename, Stream source)
        {
            using (source)
            {
                oid = bucket.UploadFromStream(filename, source);
                return oid;
            }
        }

        public Byte[] DownloadAsByteArray(ObjectId id)
        {
            Byte[] bytes = bucket.DownloadAsBytes(id);
            return bytes;
        }

        public Stream DownloadToStream(ObjectId id)
        {
            Stream destination = new MemoryStream();
            bucket.DownloadToStream(id, destination);
            return destination;
        }

        public Byte[] DownloadAsBytesByName(string filename)
        {
            Byte[] bytes = bucket.DownloadAsBytesByName(filename);
            return bytes;
        }

        public Stream DownloadToStreamByName(string filename)
        {
            Stream destination = new MemoryStream();
            bucket.DownloadToStreamByName(filename, destination);
            return destination;
        }

        public GridFSFileInfo FindFiles(string filename)
        {
            var filter = Builders<GridFSFileInfo>.Filter.And(
            Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, "man"),
            Builders<GridFSFileInfo>.Filter.Gte(x => x.UploadDateTime, new DateTime(2015, 1, 1, 0, 0, 0, DateTimeKind.Utc)),
            Builders<GridFSFileInfo>.Filter.Lt(x => x.UploadDateTime, new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, 0, 0, 0, DateTimeKind.Utc)));
            var sort = Builders<GridFSFileInfo>.Sort.Descending(x => x.UploadDateTime);
            var options = new GridFSFindOptions
            {
                Limit = 1,
                Sort = sort
            };
            using (var cursor = bucket.Find(filter, options))
            {
                fileInfo = cursor.ToList().FirstOrDefault();
            }
            return fileInfo;
        }


        public void DeleteAndRename(ObjectId id)
        {
            bucket.Delete(id);
        }

        //The “fs.files” collection will be dropped first, followed by the “fs.chunks” collection. This is the fastest way to delete all files stored in a GridFS bucket at once.   
        public void DroppGridFSBucket()
        {
            bucket.Drop();
        }

        public void RenameAsingleFile(ObjectId id, string newFilename)
        {
            bucket.Rename(id, newFilename);
        }

        public void RenameAllRevisionsOfAfile(string oldFilename, string newFilename)
        {
            var filter = Builders<GridFSFileInfo>.Filter.Eq(x => x.Filename, oldFilename);
            var filesCursor = bucket.Find(filter);
            var files = filesCursor.ToList();
            foreach (var file in files)
            {
                bucket.Rename(file.Id, newFilename);
            }
        }

    }
}
