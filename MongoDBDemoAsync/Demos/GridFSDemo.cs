namespace MongoDBDemoAsync
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.GridFS;

    public class GridFSDemo : IDemo
    {
        #region Public Methods and Operators

        public async Task DemoCreateIndexAsync(IMongoDatabase database, string indexName)
        {
            IMongoCollection<GridFSFileInfo> filesCollection = database.GetCollection<GridFSFileInfo>("fs.files");
            IndexKeysDefinition<GridFSFileInfo> keys =
                Builders<GridFSFileInfo>.IndexKeys.Ascending("Metadata.Category").Ascending("Metadata.SubGroup");
            //Add an optional name- useful for admin
            var options = new CreateIndexOptions { Name = indexName };
            await filesCollection.Indexes.CreateOneAsync(keys, options);
        }

        public async Task DemoDownloadFileAsync(IMongoDatabase database, string filePath, string fileName)
        {
            var gridFsBucket = new GridFSBucket(database);
            using (
                GridFSDownloadStream<ObjectId> sourceStream = await gridFsBucket.OpenDownloadStreamByNameAsync(fileName)
                )
            {
                using (FileStream destinationStream = File.Open(filePath, FileMode.Create))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                }
            }
        }

        public async Task<List<GridFSFileInfo>> DemoFindFilesAsync(IMongoCollection<GridFSFileInfo> filesCollection)
        {
            //Search using the PhotoIndex
            FilterDefinitionBuilder<GridFSFileInfo> builder = Builders<GridFSFileInfo>.Filter;
            FilterDefinition<GridFSFileInfo> filter = builder.Eq("metadata.Category", "Astronomy")
                                                      & builder.Eq("metadata.SubGroup", "Planet");
            return await filesCollection.Find(filter).ToListAsync();
        }

         public async Task DemoUploadAsync(IMongoDatabase database, string filePath, string fileName)
        {
            var photoMetadata = new BsonDocument
            {
                { "Category", "Astronomy" },
                { "SubGroup", "Planet" },
                { "ImageWidth", 640 },
                { "ImageHeight", 480 }
            };
            var uploadOptions = new GridFSUploadOptions { Metadata = photoMetadata };
            await UploadFileAsync(database, filePath, fileName, uploadOptions);
        }

        public async Task RunDemoAsync(IMongoCollection<ClubMember> collection)
        {
            Console.WriteLine("Starting GridFSDemo");
            IMongoDatabase database = collection.Database;
            var gridFsBucket = new GridFSBucket(database);
            const string filePath = @"..\..\Images\mars996.png";
            //the name of the uploaded GridFS file
            const string fileName = @"mars996";
            try
            {
                await DemoUploadAsync(database, filePath, fileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("***GridFS Error " + e.Message);
            }

            await DemoCreateIndexAsync(database, "PhotoIndex");
           IMongoCollection<GridFSFileInfo> filesCollection = database.GetCollection<GridFSFileInfo>("fs.files");
            List<GridFSFileInfo> fileInfos = await DemoFindFilesAsync(filesCollection);
            foreach (GridFSFileInfo gridFsFileInfo in fileInfos)
            {
                Console.WriteLine("Found file {0} Length {1} ID= {2}", gridFsFileInfo.Filename, gridFsFileInfo.Length,gridFsFileInfo.Id);
                
            }
            try
            {
                await DemoDownloadFileAsync(database, filePath, fileName);
            }
            catch (Exception e)
            {
                Console.WriteLine("***GridFS Error " + e.Message);
            }
            var fileInfo = fileInfos.FirstOrDefault(fInfo => fInfo.Filename == fileName);
            if (fileInfo != null)
            {
                await gridFsBucket.DeleteAsync(fileInfo.Id);
                Console.WriteLine("\r\n{0} has been deleted",fileName);
                //to delete everything in the gridBucket use   await gridFsBucket.DropAsync();
            }
        }

        public async Task UploadFileAsync(
            IMongoDatabase database,
            string filePath,
            string fileName,
            GridFSUploadOptions uploadOptions=null)
        {
            var gridFsBucket = new GridFSBucket(database);
            using (FileStream sourceStream = File.Open(filePath, FileMode.Open))
            {
                using (
                    GridFSUploadStream destinationStream =
                        await gridFsBucket.OpenUploadStreamAsync(fileName, uploadOptions))
                {
                    await sourceStream.CopyToAsync(destinationStream);
                    await destinationStream.CloseAsync();
                }
            }
        }

        #endregion
    }
}