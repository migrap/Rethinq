using Remotion.Linq.Parsing.Structure;
using Rethinq.Data.RqlClient;
using System.Linq;

namespace Rethinq.Linq {
    public static partial class Extensions {
        public static IQueryable<T> AsQueryable<T>(this RethinqTable<T> table) {
            return new RethinqQueryable<T>(QueryParser.CreateDefault(), new RethinqQueryExecutor());
        }        

        public static void Sandbox() {
            var query = (new RethinqTable<People>()).AsQueryable();
            query.Where(x => x.Id == 0).Count();
            //query.Count();
        }
    }

    internal class People {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
