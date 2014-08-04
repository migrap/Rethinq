using Remotion.Linq.Parsing.Structure;
using Rethinq;
using Rethinq.Data.RqlClient;
using Rethinq.Linq;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Automatonymous;
using System.Reactive.Linq;

namespace Sandbox {
    class Program {
        static void Main(string[] args) {
            //Rethinq.Linq.Extensions.Sandbox();     

            var connection = new Connection();
            connection.ConnectAsync(x => x.Pugna).Wait();
            connection.ConnectAsync(x => x.Courier).Wait();

            Globals.Connetion = connection;

            var queryable = new RethinqTable<People>().AsQueryable();
            var count = queryable.Where(x => x.Id == 0).Count();


            Console.ReadLine();
        }
    }

    public static partial class Extensions {
        

        public static EndPoint Courier(this Connection connection) {
            return new DnsEndPoint("10.0.1.75", 28015);
        }

        public static EndPoint Pugna(this Connection connection) {
            return new DnsEndPoint("10.0.1.80", 28015);
        }
    }

    class Customers {
        public string Name { get; set; }
        public string City { get; set; }
    }

    class People {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}