namespace MongoDBDemoAsync
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    using MongoDB.Driver;

    public class DemoManager
    {
        #region Constants and Fields

        private readonly IMongoCollection<ClubMember> collection;

          #endregion

        #region Constructors and Destructors

        public DemoManager(IMongoCollection<ClubMember> collection)
        {
            this.collection = collection;
        }

        #endregion

        #region Public Methods and Operators

      

        public async Task RunDemosAsync()
        {
            var tasks = new List<Task>
            {
                new AggregationDemo().RunDemoAsync(collection),
                new LinqDemo().RunDemoAsync(collection),
                new MapreduceDemo().RunDemoAsync(collection),
                new GridFSDemo().RunDemoAsync(collection)
            };
            await Task.WhenAll(tasks);
        }

        #endregion
    }
}