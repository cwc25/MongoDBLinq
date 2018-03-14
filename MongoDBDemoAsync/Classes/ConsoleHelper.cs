using System;

namespace MongoDBDemoAsync
{
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public  class ConsoleHelper
    {
        public const int MINIMUMMEMBERSHIPSIZE = 60;
        public const int MAXIMUMMEMBERSHIPSIZE = 100;
        public static void PrintClubMemberToConsole(ClubMember member)
        {
           
                Console.WriteLine(
                    "{0,-12}{1,-10}{2,4}{3,14}",
                    member.Lastname,
                    member.Forename,
                    member.Age,
                    member.MembershipDate.ToShortDateString());
           
        }
        public static int GetNumberFromUser(int min, int max)
        {

            int value;
            while (true)
            {
                Console.Write("Please enter an integer between {0} and {1} or 'exit':", min, max);
                string line = Console.ReadLine();
                if (line == "exit")
                {
                    Environment.Exit(0);
                }
                if (int.TryParse(line, out value))
                {
                    if (value <= max && value >= min)
                    {
                        break;
                    }


                }


            }
            return value;
        }

        public static async Task PrintClubMembersToConsoleAsync(IEnumerable<ClubMember> members)
      {
          var sb = new StringBuilder();
          foreach (var member in members)
          {
             sb.Append(string.Format(
                  "{0,-12}{1,-10}{2,4}{3,14}\r\n",
                  member.Lastname,
                  member.Forename,
                  member.Age,
                  member.MembershipDate.ToShortDateString()));
          }
          await Console.Out.WriteAsync(sb.ToString());
      }

      public static async Task PrintClubMemberToConsoleAsync(ClubMember member)
      {
          var s = string.Format(
              "{0,-12}{1,-10}{2,4}{3,14}\r\n",
              member.Lastname,
              member.Forename,
              member.Age,
              member.MembershipDate.ToShortDateString());
       
          await Console.Out.WriteAsync(s);
      }
     
      public static void PromptToContinue()
      {
          Console.WriteLine("\r\nHit return to continue");
          Console.ReadLine();
      }
    }
}
