using System;
using System.Collections.Generic;
using System.Linq;

namespace MongoDBDemoAsync
{
    using System.Threading.Tasks;

    using MongoDB.Bson;
    using MongoDB.Bson.Serialization;
    using MongoDB.Driver;

    public class AggregationDemo : IDemo
    {
        public async Task AggregateFamilyStatsAsync(IMongoCollection<ClubMember> collection)
        {
            Console.WriteLine("Starting GetFamilyStatsAsync");
            var sortFamiliesOperation = new BsonDocument { { "$sort", new BsonDocument { { "FamilyName", 1 } } } };
            var groupFamiliesOperation = new BsonDocument
            {
                {
                    //Sort the documents into groups
                    "$group", new BsonDocument
                    {
                        //Make the unique identifier for the group a BSON element consisting
                        // of a field named FamilyId.
                        // Set its value to that of the Lastname field 
                                
                        { "_id", new BsonDocument { { "FamilyId", "$Lastname" } } },
                        {
                            //Declare another output field and name it Count. 
                            //Increase Count's value by one for each document in the group 
                            "Count", new BsonDocument { { "$sum", 1 } }
                        },
                        {
                            //Declare an other output field and name it TotalAge. 
                            //Increase TotalAge's value by the Age value for each document in the group 
                            "TotalAge", new BsonDocument { { "$sum", "$Age" } }
                        },
                        { "MinAge", new BsonDocument { { "$min", "$Age" } } },
                        { "MaxAge", new BsonDocument { { "$max", "$Age" } } }
                    }
                }
            };

            var projectFamilyOperation = new BsonDocument
            {
                {
                    "$project", new BsonDocument
                    {
                        //Drop the _id field, the '0' means drop
                        { "_id", 0 },
                        //Declare a new field. Set its value to the FamilyId value of the _id
                        { "FamilyName", "$_id.FamilyId" },
                        //Keep the fields detailed below. The '1' means keep
                        { "Count", 1 },
                        { "TotalAge", 1 },
                        { "MinAge", 1 },
                        { "MaxAge", 1 }
                    }
                }
            };
            var pipeline = new[] { groupFamiliesOperation, projectFamilyOperation, sortFamiliesOperation };
            var familyStats = await collection.Aggregate<FamilyStat>(pipeline).ToListAsync();
           Console.WriteLine("\r\nFamily Stats grouped by Lastname");
           Console.WriteLine("Finished GetFamilyStatsAsync");
            Console.WriteLine("{0,-12}{1,10}{2,12}{3,13}{4,12}", "Family", "Members", "Oldest", "Youngest", "TotalAge");
            foreach (var stats in familyStats)
            {


                Console.WriteLine(
                    "{0,-11}{1,8}{2,12}{3,14}{4,14}",
                    stats.FamilyName,
                    stats.Count,
                    stats.MaxAge,
                    stats.MinAge,
                    stats.TotalAge);
            }
          //  ConsoleHelper.PromptToContinue();

        }

        public async Task AggregateOwnersByCarManufacturerAsync(IMongoCollection<ClubMember> collection)
        {

            Console.WriteLine("Starting AggregateOwnersByCarManufacturerAsync");
            DateTime utcTime5yearsago = DateTime.Now.AddYears(-5).ToUniversalTime();

            var matchMembershipDateOperation = new BsonDocument
            {
                {
                    "$match", new BsonDocument { { "MembershipDate", new BsonDocument { { "$gte", utcTime5yearsago } } } }
                }
            };

            var unwindCarsOperation = new BsonDocument { { "$unwind", "$Cars" } };

            var groupByCarTypeOperation = new BsonDocument
            {
                {
                    //Sort the documents into groups
                    "$group", new BsonDocument
                    {
                        //Make the unique identifier for the group a BSON element consisting
                        // of a field named Car.
                        // Set its value to that of the Cars field 
                        // The Cars field is no longer an array because it has been unwound
                        { "_id", new BsonDocument { { "Car", "$Cars" } } },
                        {
                            "Owners", new BsonDocument
                            {
                                {
                                    //add a value to the Owners Array if it does not
                                    //already contain an  identical value
                                    "$addToSet",
                                    //The value to add is a BsonDocument with an identical structure to
                                    // a serialized ClubMember class.
                                    new BsonDocument
                                    {
                                        { "_id", "$_id" },
                                        { "Lastname", "$Lastname" },
                                        { "Forename", "$Forename" },
                                        { "Age", "$Age" },
                                        { "MembershipDate", "$MembershipDate" }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            var projectMakeOfCarOperation = new BsonDocument
            {
                { "$project", new BsonDocument { { "_id", 0 }, { "MakeOfCar", "$_id.Car" }, { "Owners", 1 } } }
            };

            var sortCarsOperation = new BsonDocument { { "$sort", new BsonDocument { { "MakeOfCar", 1 } } } };

            var pipeline = new[]
            {
                matchMembershipDateOperation, unwindCarsOperation, groupByCarTypeOperation, projectMakeOfCarOperation,
                sortCarsOperation
            };
           
            var carStatsList = await collection.Aggregate<CarStat>(pipeline).ToListAsync();
            Console.WriteLine("Finished AggregateOwnersByCarManufacturerAsync");
            Console.WriteLine("\r\nMembers grouped by Car Marque");
            foreach (CarStat stat in carStatsList)
            {
                Console.WriteLine("\n\rCar Marque : {0}\n\r", stat.MakeOfCar);
                IEnumerable<ClubMember> clubMembers =
                    stat.Owners.ToArray()
                    //deserialize the BsonDocument[] to an IEnumerable<ClubMember>
                        .Select(d => BsonSerializer.Deserialize<ClubMember>(d))
                        .OrderBy(c => c.Lastname)
                        .ThenBy(c => c.Forename)
                        .ThenBy(c => c.Age)
                        .Select(c => c);
                foreach (ClubMember clubMember in clubMembers)
                {

                    ConsoleHelper.PrintClubMemberToConsole(clubMember);
                }
              
            }
           // ConsoleHelper.PromptToContinue();
        }

        public async Task RunDemoAsync(IMongoCollection<ClubMember> collection)
        {
            Console.WriteLine("Starting Aggregation Demo");
            var tasks = new List<Task>
            {
                //This starts each task without 'awaiting' them
                AggregateFamilyStatsAsync(collection),
                AggregateOwnersByCarManufacturerAsync(collection)
            };
            await Task.WhenAll(tasks);
            Console.WriteLine("Finished Aggregation Demo");
        }
    }
}
