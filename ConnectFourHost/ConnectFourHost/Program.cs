using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConnectFourServer;
using System.ServiceModel;
using System.ServiceModel.Description;
namespace ConnectFourHost
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                ServiceHost host = new ServiceHost(
            typeof(ConnectFourService));
                host.Description.Behaviors.Add(
                    new ServiceMetadataBehavior { HttpGetEnabled = true });
                host.Open();
                Console.WriteLine("Connected...");
                Console.WriteLine("Type Enter to shutdown");
                Console.ReadLine();
                host.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not connect, " + ex.Message);
            }
        }
    }
}
