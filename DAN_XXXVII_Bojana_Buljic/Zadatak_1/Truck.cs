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
        public Random rnd = new Random();
        public SemaphoreSlim semaphore = new SemaphoreSlim(2, 2);
        private readonly string routesFile = @"../../Routes.txt";
        public int[] bestRoutes = new int[10];
        public Thread[] trucks = new Thread[10];

        static readonly object locker = new object();
        public static List<int> listOfRouteNo = new List<int>();

        int loadingTime { get; set; }
        double unloadingTime { get; set; }

        int waitTime;


        /// <summary>
        /// Method for generating random route numbers and writing it into file Routes.txt
        /// </summary>
        public void GenerateRouteNo()
        {
            int[] routes = new int[1000];

            //locks the code until writing to file is finished
            lock (routesFile)
            {
                using (StreamWriter sw = new StreamWriter(routesFile))
                {

                    for (int i = 0; i < routes.Length; i++)
                    {
                        routes[i] = rnd.Next(1, 5001);
                        sw.WriteLine(routes[i]);
                    }
                }
                //signal that writing in file is finished
                Monitor.Pulse(routesFile);
            }

        }

        /// <summary>
        /// Method reads file Routes and creates array with selected 10 best routes
        /// </summary>
        public void SelectBestRoutes()
        {
            int number;
            lock (routesFile)
            {
                //wait 3000 ms until file is created
                while (!File.Exists(routesFile))
                {
                    Monitor.Wait(routesFile, 3000);
                }

                //reading lines from file
                using (StreamReader reader = File.OpenText(routesFile))
                {
                    string line = " ";
                    while ((line = reader.ReadLine()) != null)
                    {
                        //converting each string into number
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

                Console.WriteLine("Best routes are selected. \nManager chooses next routes for trucks:\n");
                //writing selected best routes on Console
                for (int i = 0; i < bestRoutes.Length; i++)
                {
                    Console.WriteLine(bestRoutes[i]);
                }
                Console.WriteLine("\nYou can start loading trucks.\n");
            }

        }
    }
}
