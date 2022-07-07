using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceExercise
{
    public sealed class RequestsService : IService
    {
        private static int          _sum = 0;
        Stopwatch                   watch = null;
        private static Connection[] connectionArray;

     
        public RequestsService(int _MaxConnections)
        {
            connectionArray = new Connection[_MaxConnections];
            watch = Stopwatch.StartNew();
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
            Console.WriteLine("sendRequest started");
            Random  random = new Random();
            int     i = 0;
            
            try
            {

                i = random.Next(0, connectionArray.Length);

                connectionArray[i] = new Connection();

                Console.WriteLine($"using connection number - #{ i }.");

                await ConnectAndSendRequestParallelAsync(connectionArray[i], request);

                Console.WriteLine("sendRequest ended");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
 

        public static async Task ConnectAndSendRequestParallelAsync(Connection connection, Request request)
        {
            Console.WriteLine("ConnectAndSendRequestParallelAsync");
            List<Task<int>> tasks = new List<Task<int>>();

            tasks.Add(Task.Run(() => ConnectionRunCommand(connection, request)));
            

            var results = await Task.WhenAll(tasks);

            foreach (var item in results)
            {
                Interlocked.Add(ref _sum, item);
            }
        }

        public static int ConnectionRunCommand(Connection connection, Request request)
        {
            return connection.runCommand(request.Command);
        }

    }
}
