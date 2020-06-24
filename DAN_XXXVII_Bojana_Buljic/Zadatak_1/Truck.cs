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
                Console.WriteLine("{0} returned to starting point after 3000 milliseconds.", name);

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
                int routeNo = bestRoutes[i];
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
