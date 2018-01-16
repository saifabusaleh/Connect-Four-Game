using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFourDBCore
{
    public class ConnectFourContext : DbContext
    {

        public DbSet<User> Users { get; set; }
        public DbSet<Game> Games { get; set; }
        public ConnectFourContext()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ConnectFourContext>());
        }
    }
}
