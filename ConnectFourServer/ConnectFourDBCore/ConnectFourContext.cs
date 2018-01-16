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


        public ConnectFourContext()
        {
            Database.SetInitializer(new DropCreateDatabaseIfModelChanges<ConnectFourContext>());
        }
    }
}
