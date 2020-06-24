using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zadatak_1
{
    /// <summary>
    /// Class for simulating truck company operations.
    /// Creates threads for performing tasks of generating and selecting routes and Loading/Unloading trucks
    /// </summary>

    public class Truck
    {
        public Random rnd = new Random();
        public SemaphoreSlim semaphore = new SemaphoreSlim(2, 2);
        private readonly string routesFile = @"../../Routes.txt";
        //array for best selected routes
        public int[] bestRoutes = new int[10];
        //array of threads representing each delivery truck
        public Thread[] trucks = new Thread[10];

        static readonly object locker = new object();
        public static List<int> listOfRouteNo = new List<int>();
        double unloadingTime { get; set; }
        //counter for threads 
        int count = 0;
      
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
       
        /// <summary>
        /// Method performing loading/unloading trucks
        /// </summary>
        /// <param name="routeNo"></param>
        public void LoadingUnloadingTrucks(object routeNo)
        {
            var name = Thread.CurrentThread.Name;
            int loadingTime = rnd.Next(500, 5001);            
            semaphore.Wait();
            Console.WriteLine(name + " is loading for {0} ms ...\n", loadingTime);
            Thread.Sleep(loadingTime);

            Console.WriteLine(name + " is loaded.\n");
            semaphore.Release();

            //thread counter..waits for all Trucks to load and then assign route for each of them
            count++;
            while (count != 10)
            {
                Thread.Sleep(0);
            }

            Console.WriteLine("{0} gets route {1}", name, routeNo);

            //wait until routes are assigned to start to destination
            lock (locker)
            {
                count--;

            }
            while (count != 0)
            {
                Thread.Sleep(0);
            }

            //delivery waiting time
            int waitTime = rnd.Next(500, 5001);
            Console.WriteLine("\n{0} started to destination. \nDelivery waiting time is {1} milliseconds.\n", name, waitTime);
            //Thread waits for 3000 ms
            Thread.Sleep(3000);

            if (waitTime > 3000)
            {
                Console.WriteLine("Order for {0} is cancelled.", name);
                Console.WriteLine("{0} returning to starting point.\n", name);
                Thread.Sleep(3000);
                Console.WriteLine("{0} returned to starting point after 3000 ms.", name);
            }
            else
            {
                unloadingTime = Convert.ToDouble(loadingTime) / 1.5;
                Console.WriteLine(name + " arrived.\n Unloading will last " + Convert.ToInt32(unloadingTime) + " milliseconds.\n");
                Thread.Sleep(Convert.ToInt32(unloadingTime));
                Console.WriteLine(name + " is unloaded.\n");
            }
        }

        /// <summary>
        /// Method creating and starting all threads
        /// </summary>
        public void PerformDelivery()
        {
            //Creating and naming thread for generating routes
            Thread getRoutes = new Thread(GenerateRouteNo)
            {
                Name = "routeGenerator"
            };

            //Creating and naming thread for manager's job-selecting best routes
            Thread manager = new Thread(SelectBestRoutes)
            {
                Name = "manager"
            };

            //starting threads
            getRoutes.Start();
            manager.Start();

            //Joining threads-wait till these threads are finished
            getRoutes.Join();
            manager.Join();

            //creating and starting truck threads from thread array
            for (int i = 0; i < 10; i++)
            {
                trucks[i] = new Thread(LoadingUnloadingTrucks)
                {
                    //naming each thread
                    Name = String.Format("Truck_{0}", i + 1)
                };
                trucks[i].Start(bestRoutes[i]);
            }
            for (int i = 0; i < trucks.Length; i++)
            {
                trucks[i].Join();
            }
        }
    }
}