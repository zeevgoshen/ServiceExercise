using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceExercise
{
    public sealed class RequestsService : IService
    {
        private static int _sum = 0;
        Stopwatch watch = null;

        private static Connection[] connectionArray;

     
        public RequestsService(int _MaxConnections)
        {
            connectionArray = new Connection[_MaxConnections];
            watch = System.Diagnostics.Stopwatch.StartNew();
        }
 

        public int getSummary()
        {
            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Console.WriteLine($"Total execution time: { elapsedMs }");
            return _sum;
        }

        // should not block
        public async void sendRequest(Request request)
        {
            Console.WriteLine("sendRequest called");
            Random random = new Random();

            int i = random.Next(0, connectionArray.Length);

            connectionArray[i] = new Connection();

            Console.WriteLine($"using connection number - #{ i }.");

            await ConnectAndSendRequestParallelAsync(connectionArray[i], request);

            Console.WriteLine("sendRequest ended");
        }
 

        public static async Task ConnectAndSendRequestParallelAsync(Connection connection, Request request)
        {
            Console.WriteLine("ConnectAndSendRequestParallelAsync");
            List<Task<int>> tasks = new List<Task<int>>();
            //int sum = 0;

            tasks.Add(Task.Run(() => ProcessCard(connection, request)));
            

            var results = await Task.WhenAll(tasks);

            foreach (var item in results)
            {
                //_sum
                //Console.WriteLine(item);
                Interlocked.Add(ref _sum, connection.runCommand(request.Command));
            }
            //It will execute all the tasks concurrently
        }

        public static int ProcessCard(Connection connection, Request request)
        {
            return connection.runCommand(request.Command);
        }

    }
}
