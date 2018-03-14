
namespace MongoDBDemoAsync
{
    using MongoDB.Bson;

    public class CarStat
    {
        #region Public Properties

        public string MakeOfCar { get; set; }

        public BsonDocument[] Owners { get; set; }

        #endregion
    }
}
