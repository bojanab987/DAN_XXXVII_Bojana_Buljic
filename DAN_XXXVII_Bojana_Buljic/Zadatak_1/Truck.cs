using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zadatak_1
{
    public class Truck
    {
        static SemaphoreSlim semaphore = new SemaphoreSlim(2, 2);
        static string routesFile = @"../../Routes.txt";
        static readonly object locker = new object();
        static List<int> listOfRouteNo = new List<int>();

        public Thread thread;
        public int routeNo { get; set; }
        int[] bestRoutes = new int[10];        
        string name { get; set; }
        

        /// <summary>
        /// Method for generating random route numbers and writing it into file Routes.txt
        /// </summary>
        public void GenerateRouteNo()
        {
            int[] routes = new int[1000];
            Random rnd = new Random();

            for (int i = 0; i < routes.Length; i++)
            {
                routes[i] = rnd.Next(1, 5001);
            }

            //locks the code until writing to file is finished
            //lock (locker)
            //{
            using (StreamWriter sw = new StreamWriter(routesFile))
            {

                for (int i = 0; i < routes.Length; i++)
                {
                    sw.WriteLine(routes[i]);
                }
            }
            //    Monitor.Pulse(locker);
            //}

        }

        /// <summary>
        /// Method reads file Routes and creates array with selected 10 best routes
        /// </summary>
        public void SelectBestRoutes()
        {
            int number;
            //lock (locker)
            //{
            Random rnd = new Random();
            Thread.Sleep(rnd.Next(1, 3001));
            //Monitor.Wait(locker);
            using (StreamReader reader = File.OpenText(routesFile))
            {
                string line = " ";
                while ((line = reader.ReadLine()) != null)
                {
                    bool convert = Int32.TryParse(line, out number);
                    if (convert && number % 3 == 0)
                    {
                        //Add into list only numbers divisible by 3
                        listOfRouteNo.Add(number);

                    }
                }
            }
            //sorting list from lowest to largest number
            listOfRouteNo.Sort();
            //Filling array with 10 minimum and distinct values from list
            bestRoutes = listOfRouteNo.Distinct().Take(10).ToArray();

            Console.WriteLine("Best routes are selected. \nManager chooses routes for trucks.\n");
            for (int i = 0; i < bestRoutes.Length; i++)
            {
                Console.WriteLine("Truck {0} has route No. {1}", i + 1, bestRoutes[i]);
                routeNo = bestRoutes[i];
                name = string.Format((i + 1).ToString());
            }
            Console.WriteLine("\nYou can start loading trucks.");
            //}

        }
    }
}
