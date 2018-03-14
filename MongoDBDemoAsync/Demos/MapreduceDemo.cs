using System;
using System.Linq;

namespace MongoDBDemoAsync
{
    using System.Threading.Tasks;

    using MongoDB.Bson;
    using MongoDB.Driver;

    public class MapreduceDemo : IDemo 
    {
        public async Task RunDemoAsync(IMongoCollection<ClubMember> collection)
        {
            try
            {
               await DemoMapReduceAsync(collection);
            }
            catch(MongoCommandException ex)
            {
                Console.WriteLine("The MapReduce Command is not supported with this deployment.\r\n {0}",ex.ErrorMessage);
            }
                
               
          
        }

        public async Task DemoMapReduceAsync(IMongoCollection<ClubMember> collection)
      {
          //The match function has to be written in JavaScript 
          //the map method outputs a key value pair for each document
          //in this case the key is the Lastname property value and the value is the Age property value
          var map = new BsonJavaScript(@"function() 
                                            {
                                             //Associate each LastName property with the Age value
                                               emit(this.Lastname,this.Age);
                                             }");
          //The MapReduce method  uses the output from the Map method
          //   to produce a list of ages for each unique LastName value.
          //  Then each key and its associated list of values is presented to the Reduce method in turn.
          var reduce = new BsonJavaScript(@"function(lastName,ages) 
                                             {
                                                 return Array.sum(ages);
                                              }");
          //The Reduce method returns the Lastname as the key and the sum of the ages as the value
          //The beauty of this technique is that data can be processed in batches
          //The output of one batch is combined with that of another and fed back through the Reducer
          //This is repeated until the output is reduced to the number of unique keys.
          // The results are output to a new collection named ResultsCollection on the server. 
          // This saves on the use of computer memory and enables the results to be queried effectively by using indexes. 

          var options = new MapReduceOptions<ClubMember, BsonDocument>
          {
              //Replace (the default) : the content of existing collection is dropped, and the new output go into it
              // Merge  The existing collection is kept.When a result with the same Key(the _id) exists, the value in the collection is replaced with it.
              //Reduce : The existing collection is kept.When a result with the same Key(the _id) exists, 
              //MongoDB runs the reduce function against the two of them, and stores the result.
              OutputOptions = MapReduceOutputOptions.Reduce("ResultsCollection")
          };
          var resultAsBsonDocumentList = await collection.MapReduce(map, reduce, options).ToListAsync();
           Console.WriteLine("The total age for every member of each family  is ....");
          var reduction =
              resultAsBsonDocumentList.Select(
                  doc => new { family = doc["_id"].AsString, age = (int)doc["value"].AsDouble });
          foreach (var anon in reduction)
          {
              Console.WriteLine("{0} Family Total Age {1}", anon.family, anon.age);
          }
       }
    }
}
