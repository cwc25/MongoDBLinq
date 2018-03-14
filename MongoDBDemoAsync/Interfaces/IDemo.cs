namespace MongoDBDemoAsync
{
    using System.Threading.Tasks;

    using MongoDB.Driver;

    public interface IDemo
    {
        Task RunDemoAsync(IMongoCollection<ClubMember> collection);
    }
}