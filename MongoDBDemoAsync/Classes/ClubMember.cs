namespace MongoDBDemoAsync
{
    using System;
    using System.Collections.Generic;

    using MongoDB.Bson;

    public class ClubMember
    {
        #region Public Properties

        public int Age { get; set; }

        public List<string> Cars { get; set; }

        public string Forename { get; set; }

        public ObjectId Id { get; set; }

        public string Lastname { get; set; }

        public DateTime MembershipDate { get; set; }

        #endregion

        #region Public Methods and Operators

        

        #endregion
    }

   
}