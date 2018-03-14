namespace MongoDBDemoAsync
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using MongoDB.Driver;
    using MongoDB.Driver.Linq;

    public class LinqDemo : IDemo
    {
        #region Public Methods and Operators

        public async Task AggregateFamilyStatsLinqAsync(IMongoCollection<ClubMember> collection)
        {
            Console.WriteLine("Starting GetFamilyStatsLinqAsync");

            List<FamilyStat> familyStats =
                await
                    collection.Aggregate()
                        .Group(
                            x => x.Lastname,
                            g =>
                                new FamilyStat
                                {
                                    FamilyName = g.Key,
                                    TotalAge = g.Sum(x => x.Age),
                                    MinAge = (g.Min(x => x.Age)),
                                    MaxAge = (g.Max(x => x.Age)),
                                    Count = g.Count()
                                })
                        .SortBy(x => x.FamilyName)
                        .ToListAsync();
            Console.WriteLine("Finished GetFamilyStatsLinqAsync");
            Console.WriteLine("\r\nFamily Stats grouped by Lastname");

            Console.WriteLine("{0,-12}{1,10}{2,12}{3,13}{4,12}", "Family", "Members", "Oldest", "Youngest", "TotalAge");
            foreach (FamilyStat stats in familyStats)
            {
                Console.WriteLine(
                    "{0,-11}{1,8}{2,12}{3,14}{4,14}",
                    stats.FamilyName,
                    stats.Count,
                    stats.MaxAge,
                    stats.MinAge,
                    stats.TotalAge);
            }
        }

        public async Task EnumerateClubMembersAsync(IMongoCollection<ClubMember> collection)
        {
            Console.WriteLine("Starting EnumerateClubMembersAsync");
            List<ClubMember> membershipList =
                await collection.AsQueryable().OrderBy(p => p.Lastname).ThenBy(p => p.Forename).ToListAsync();
            Console.WriteLine("Finished EnumerateClubMembersAsync");
            Console.WriteLine("List of ClubMembers in collection ...");
            foreach (ClubMember clubMember in membershipList)
            {
                ConsoleHelper.PrintClubMemberToConsole(clubMember);
            }
        }

        public async Task FindUsingFilterDefinitionBuilder1Async(IMongoCollection<ClubMember> collection)
        {
            Console.WriteLine("Starting FindUsingFilterDefinitionBuilder1Async");
            DateTime cutOffDate = DateTime.Now.AddYears(-5);
            FilterDefinitionBuilder<ClubMember> builder = Builders<ClubMember>.Filter;
            //A greater than filter. Selects where the MembershipDate is greater than the cutOffDate
            FilterDefinition<ClubMember> filterDefinition = builder.Gt("MembershipDate", cutOffDate.ToUniversalTime());
            //DateTime is stored in BsonElement as a UTC value so need to convert
            List<ClubMember> membersList =
                await collection.Find(filterDefinition).SortBy(c => c.Lastname).ThenBy(c => c.Forename).ToListAsync();
            Console.WriteLine("Finished FindUsingFilterDefinitionBuilder1Async");
            Console.WriteLine("\r\nMembers who have joined in the last 5 years ...");
            foreach (ClubMember clubMember in membersList)
            {
                ConsoleHelper.PrintClubMemberToConsole(clubMember);
            }
        }

        public async Task FindUsingFilterDefinitionBuilder2Async(IMongoCollection<ClubMember> collection)
        {
            Console.WriteLine("Starting FindUsingFilterDefinitionBuilder2Async");
            FilterDefinitionBuilder<ClubMember> builder = Builders<ClubMember>.Filter;
            //An 'Or' filter. Selects where Lastname ==Rees Or Lastname==Jones
            FilterDefinition<ClubMember> filter = builder.Or(
                Builders<ClubMember>.Filter.Eq("Lastname", "Rees"),
                Builders<ClubMember>.Filter.Eq("Lastname", "Jones"));
            IEnumerable<ClubMember> jonesReesList =
                await
                    collection.Find(filter)
                        .SortBy(c => c.Lastname)
                        .ThenBy(c => c.Forename)
                        .ThenByDescending(c => c.Age)
                        .ToListAsync();
            Console.WriteLine("Finished FindUsingFilterDefinitionBuilder2Async");
            Console.WriteLine("Members named Jones or Rees ...");
            foreach (ClubMember clubMember in jonesReesList)
            {
                ConsoleHelper.PrintClubMemberToConsole(clubMember);
            }
            Console.WriteLine("...........");
        }

        public async Task FindUsingForEachAsync(IMongoCollection<ClubMember> collection)
        {
            Console.WriteLine("Starting FindUsingForEachAsync");
            FilterDefinitionBuilder<ClubMember> builder = Builders<ClubMember>.Filter;
            FilterDefinition<ClubMember> filter = builder.Or(
                Builders<ClubMember>.Filter.Eq("Lastname", "Rees"),
                Builders<ClubMember>.Filter.Eq("Lastname", "Jones"));
            await collection.Find(filter)
                //the async read of each item is awaited in sequence
                //the 'action' delegate runs on a threadpool thread
                .ForEachAsync(c => DoSomeAction(c));
            Console.WriteLine(" Finished FindUsingForEachAsync");
        }

        public async Task OrderedFindSelectingAnonymousTypeAsync(IMongoCollection<ClubMember> collection)
        {
            Console.WriteLine("Starting  OrderedFindSelectingAnonymousTypeAsync");
            var names =
                await
                    collection.AsQueryable()
                        .Where(p => p.Lastname.StartsWith("R") && p.Forename.EndsWith("an"))
                        .OrderBy(p => p.Lastname)
                        .ThenBy(p => p.Forename)
                        .Select(p => new { p.Forename, p.Lastname })
                        .ToListAsync();
            Console.WriteLine("Finished  OrderedFindSelectingAnonymousTypeAsync");
            Console.WriteLine("Members with Lastname starting with 'R' and Forename ending with 'an'");
            foreach (var name in names)
            {
                Console.WriteLine(name.Lastname + " " + name.Forename);
            }
        }

        //this method is very slow
        public async Task RegexFindAsync(IMongoCollection<ClubMember> collection)
        {
            Console.WriteLine("Starting  RegexFindAsync");
            var regex = new Regex("ar");
            List<string> regexResults =
                await
                    collection.AsQueryable()
                        .Where(py => regex.IsMatch(py.Lastname))
                        .Select(p => p.Lastname)
                        .Distinct()
                        .ToListAsync();

            Console.WriteLine("Finished  RegexFindAsync");
            Console.WriteLine("List of Lastnames containing the substring 'ar'");
            foreach (string name in regexResults)
            {
                Console.WriteLine(name);
            }
            //   ConsoleHelper.PromptToContinue();
        }

        public async Task RunDemoAsync(IMongoCollection<ClubMember> collection)
        {
            Console.WriteLine("starting Linq Demo");
            var tasks = new List<Task>
            {
                //This starts each task without 'awaiting' them
                AggregateFamilyStatsLinqAsync(collection),
                EnumerateClubMembersAsync(collection),
                OrderedFindSelectingAnonymousTypeAsync(collection),
                FindUsingForEachAsync(collection),
                RegexFindAsync(collection),
                FindUsingFilterDefinitionBuilder1Async(collection),
                FindUsingFilterDefinitionBuilder2Async(collection),
                SelectiveCountAsync(collection)
            };
            //await them all to complete
            await Task.WhenAll(tasks);
            Console.WriteLine("Linq Demo Completed");
            //run a demo that changes the collection.
            await UpdateManyAsync(collection);
        }

        public async Task SelectiveCountAsync(IMongoCollection<ClubMember> collection)
        {
            Console.WriteLine("Starting SelectiveCountAsync");
            int result =
                await
                    collection.AsQueryable()
                        .Select(c => c)
                        .CountAsync(c => c.Lastname == "Jones" && c.Forename == "David");
            Console.WriteLine("Finished SelectiveCountAsync");
            Console.WriteLine("\r\nThe number of members named David Jones is {0}", result);
            //  ConsoleHelper.PromptToContinue();
        }

        public async Task UpdateManyAsync(IMongoCollection<ClubMember> collection)
        {
            //Change the name of every lastname Jones to Jones-Rees
            Console.WriteLine("Starting  UpdateManyAsync");
            FilterDefinitionBuilder<ClubMember> DavidJonesBuilder = Builders<ClubMember>.Filter;
            FilterDefinition<ClubMember> DavidJonesFilter = DavidJonesBuilder.Eq("Lastname", "Jones")
                                                            & DavidJonesBuilder.Eq("Forename", "David");
            UpdateDefinition<ClubMember> update = Builders<ClubMember>.Update.Set("Forename", "Dai");
            await collection.UpdateManyAsync(DavidJonesFilter, update);

            List<ClubMember> resultList =
                await collection.AsQueryable().Where(c => (c.Lastname == "Jones" && c.Forename == "Dai")).ToListAsync();
            Console.WriteLine("Finished  UpdateManyAsync");
            Console.WriteLine("\r\nChanged all members named David Jones to Dai Jones");

            foreach (ClubMember member in resultList)
            {
                ConsoleHelper.PrintClubMemberToConsole(member);
            }
            //   ConsoleHelper.PromptToContinue();
        }

        #endregion

        #region Methods

        private void DoSomeAction(ClubMember c)
        {
            //It's best only to use thread-safe methods here
            //as this is method is not running on the main thread
            Console.WriteLine("Foreach Async result: {0} {1}", c.Lastname, c.Forename);
        }

        #endregion
    }
}