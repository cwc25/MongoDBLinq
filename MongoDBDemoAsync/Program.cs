namespace MongoDBDemoAsync
{
    using System;
    using System.Threading.Tasks;

    using MongoDB.Bson;
    using MongoDB.Driver;
    using MongoDB.Driver.GridFS;

    using Nito.AsyncEx;

    internal class Program
    {
        #region Public Methods and Operators

        public static void Main()
        {

           
            Console.WriteLine("****Running on UI type SynchronizationContext****\n");
            AsyncContext.Run(() => MainAsync());
            //Does not return here until MainAsync's continuation has completed
        }

        #endregion

        #region Methods

        private static async Task MainAsync()
        {
           const string connectionString = "mongodb://localhost";
        //const string connectionString =
        //    "mongodb://localhost/?replicaSet=myReplSet&readPreference=primary";
            var client = new MongoClient(connectionString);
          
            // Use the client to access the 'entities' database
            IMongoDatabase database = client.GetDatabase("TestMongoDB");
            IMongoCollection<ClubMember> collection = database.GetCollection<ClubMember>("CarClub");
           
            var demoManager = new DemoManager(collection);
            long count = await collection.CountAsync(new BsonDocument());
            if (count == 0)
            {
                Console.WriteLine("About to add Club members to the database");
                count = ConsoleHelper.GetNumberFromUser(ConsoleHelper.MINIMUMMEMBERSHIPSIZE,ConsoleHelper.MAXIMUMMEMBERSHIPSIZE);
                var clubMembersBuilder = new ClubMembersBuilder();
               Console.WriteLine("Adding {0} new ClubMembers", count);
                await clubMembersBuilder.BuildClubMembersAsync(collection,count);
            }
            else
            {
                Console.WriteLine("Number of ClubMembers in Collection {0}", count);
            }
            //Build an index if it is not already built
            IndexKeysDefinition<ClubMember> keys =
                Builders<ClubMember>.IndexKeys.Ascending("Lastname").Ascending("Forename").Descending("Age");
            //Add an optional name- useful for admin
            var options = new CreateIndexOptions { Name = "MyIndex" };
            await collection.Indexes.CreateOneAsync(keys, options);
            await demoManager.RunDemosAsync();

            Console.WriteLine("Press return key to empty the collection and exit ");
            Console.ReadLine();
            var gridFsBucket = new GridFSBucket(database);
            await gridFsBucket.DropAsync();
            var emptyFilter = new BsonDocument();
            await collection.DeleteManyAsync(emptyFilter);
        }

        #endregion
    }
}