using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceExercise
{
    public sealed class RequestsService : IService
    {
        private static object syncRoot = new Object();
        private static RequestsService? instance;
        static int maxConnections = 0;
        private static Connection connection = new Connection();
        private static int currentConnectionsNumber = 0;
        private static int _sum = 0;
        private static ConcurrentBag<Request> requests = null;
        private object _lock = new object();


        public static RequestsService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            Console.WriteLine("service created");
                            instance = new RequestsService(maxConnections);
                        }
                    }
                }
                return instance;
            }
        }

        public void setNumberOfConnections(int numberOfConnections)
        {
            maxConnections = numberOfConnections;
        }
        private RequestsService(int _MaxConnections)
        {
            maxConnections = _MaxConnections;
        }

        private RequestsService(IAsyncEnumerable<Request> stream)
        {
            
        }



        public int getSummary()
        {
             return _sum;
        }

        // should not block
        public void sendRequest(Request request)
        {
            Console.WriteLine("sendRequest called");
            List<Connection> connections = new List<Connection>(maxConnections);

            //requests = new ConcurrentBag<Request>();


            // Task.Run(() => {
            //    requests.Add(request);

            //});
            //while (request != null)
            //{
            //    requests.Add(request);
            //}

            int i = 0;

            while (i < maxConnections) {
                Console.WriteLine($"new connection - { i }");
                connections.Add(new Connection());
                i++;
            }





            lock (_lock)
            {
                Console.WriteLine("Entered lock");
                Task.Run(() =>
                {
                    foreach (var connection in connections)
                    {
                        Console.WriteLine(connection.GetHashCode());
                        Connect(connection, request);
                    }
                });
            }

            //startRunAsyncTask(request);Task<int> result = 

            //int number = 0;
            //while (currentConnectionsNumber < maxConnections)
            //{
            //    //connection.runCommand(request.Command);
            //    number += runAsyncTask(request).Result;
            //}

            Console.WriteLine("sendRequest ended");
        }

        public static async void Connect(Connection connection, Request request)
        {
            Console.WriteLine("Connect");
            var tasks = new List<Task<int>>();
            //int sum = 0;

            await Task.Run(() =>
            {
                //Interlocked.Add(ref sum, connection.runCommand(request.Command));
                var response = ProcessCard(connection, request);
                tasks.Add(response);
            });

            //It will execute all the tasks concurrently
            await Task.WhenAll(tasks);
        }

         

        public static async Task<int> ProcessCard(Connection connection, Request request)
        {   

            Action action = () => { Interlocked.Add(ref _sum, connection.runCommand(request.Command)); };
            Task t1 = new Task(action);
            
            //await Task.Delay(1000);
            //await t1.Start();
            //string message = $"Credit Card Number: {creditCard.CardNumber} Name: {creditCard.Name} Processed";
            await Task.Run(action);
            string message = $"Credit Card Number: {_sum } Processed";
            return _sum;
        }

        //private int startRunAsyncTask(Request request)
        //{
        //    int number = 0;
        //    while (currentConnectionsNumber < maxConnections)
        //    {
        //        //connection.runCommand(request.Command);
        //        number += runAsyncTask(request).Result;
        //    }
        //    return number;

        //}

        //public static async Task Foo(int sum, Request request)
        //{
        //    Console.WriteLine("Thread {0} - Start {1}", Thread.CurrentThread.ManagedThreadId, sum);

        //    //await Task.Delay(1000);
        //    Action action = () => { Interlocked.Add(ref sum, connection.runCommand(request.Command)); };
        //    Task t1;
        //    t1 = new Task(action);

        //    await Task.Run(() => t1.Start());
        //    //t1.Start();

        //    Console.WriteLine("Thread {0} - End {1}", Thread.CurrentThread.ManagedThreadId, sum);
        //}

        //private async Task<int> runAsyncTask(Request request)
        //{

        //    try
        //    {
        //        int sum = 0;
        //        Task t1 = null;
        //        var tasks = new List<Task>();

        //        //Action action = () => { Interlocked.Add(ref sum,connection.runCommand(request.Command)); };

        //        //Action action = () => { Interlocked.Add(ref sum, connection.runCommand(request.Command)); };



        //        //t1 = new Task(action);

        //        while (currentConnectionsNumber < maxConnections)
        //        {
        //            tasks.Add(Foo(sum, request));              
        //            //await t1.Start();
        //            currentConnectionsNumber++;
        //        }

        //        Task.WaitAll(tasks.ToArray());
        //        return sum;

        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //}
    }
}
