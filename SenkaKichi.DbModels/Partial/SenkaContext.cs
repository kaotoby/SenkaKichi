using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;

namespace SenkaKichi.DbModels
{
    public partial class SenkaContext
    {
        public static SenkaContext Create() {
            var db = new SenkaContext();
#if DEBUG
            db.Database.Log = s => Debug.WriteLine(s);
#endif
            return db;
        }
    }
}
