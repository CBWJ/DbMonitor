using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbMonitor.Domain;

namespace DbMonitor
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ctx = new DbMonitorEntities())
            {
                ctx.User.Add(new User {
                    ULoginName = "Admin",
                    UPassword = "123"
                });

                ctx.SaveChanges();
            }

            Console.ReadKey();
        }
    }
}
