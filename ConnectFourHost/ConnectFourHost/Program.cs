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
            typeof(ConnectFourServer.ConnectFourService), new Uri(
                "http://localhost:8000/ConnectFourService"));
                host.Description.Behaviors.Add(
                    new ServiceMetadataBehavior { HttpGetEnabled = true });
                host.Open();
                Console.WriteLine("Connected...");
                Console.WriteLine("Type Enter to shutdown");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not connect");
            }
        }
    }
}
