using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MongoDBDemoAsync
{
    public  class ClubMembersBuilder
    {
        public async Task BuildClubMembersAsync(IMongoCollection<ClubMember> collection,long count)
        {
            string[] forenames =
            {
                //increase the number of Forenames=David  by listing David twice
                "Anwen", "David", "David", "Rhys", "Megan", "Sean", "Delyth", "Rhian", "Aled", "Alun", "Euan", "Gerient",
                "Manon"
            };
            string[] lastnames = { "Jones", "Davies", "Williams", "Rees", "Parry", "Edwards", "Thomas", "Richards" };
            string[] vintageCars = { "Alvis", "Austin", "Cooper", "Bristol", "Hillman", "Humber", "Riley" };
            var members = new List<ClubMember>();
            Random rand = new Random();
            for (int i = 0; i < count; i++)
            {
                int forenameIndex = rand.Next(0, forenames.Count());
                int lastnameIndex = rand.Next(0, lastnames.Count());

                int totalNumberOfCars = rand.Next(0, vintageCars.Count());
                DateTime today = DateTime.Now;
                var member = new ClubMember
                {
                    Forename = forenames[forenameIndex],
                    Lastname = lastnames[lastnameIndex],
                    Age = rand.Next(16, 65),
                    MembershipDate = today.AddDays(-1 * rand.Next(0, 7200))
                };
                if (totalNumberOfCars > 0)
                {
                    member.Cars = new List<string>();
                }
                for (int c = 0; c < totalNumberOfCars; c++)
                {
                    int carIndex = rand.Next(0, vintageCars.Count());
                    member.Cars.Add(vintageCars[carIndex]);
                }
                members.Add(member);
            }
            await collection.InsertManyAsync(members);
        }
    }
}
