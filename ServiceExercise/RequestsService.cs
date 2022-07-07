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
        private object              _lock = new object();

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
             
            try
            {
                await ConnectAndSendRequestParallelAsync(CreateOrUseExistingConnection(), request);

                Console.WriteLine("sendRequest ended");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        
        public Connection CreateOrUseExistingConnection()
        {
            Random random = new Random();
            int i = 0;

            try
            {
                i = random.Next(0, connectionArray.Length);

                if (connectionArray[i] == null)
                {
                    lock (_lock)
                    {
                        if (connectionArray[i] == null)
                        {
                            connectionArray[i] = new Connection();
                            Console.WriteLine($" ----------- Creating a new connection:  #{ i }.");
                        }
                    }
                }
                Console.WriteLine($"Using connection number - #{ i }.");
                return connectionArray[i];

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task ConnectAndSendRequestParallelAsync(Connection connection, Request request)
        {
            Console.WriteLine("ConnectAndSendRequestParallelAsync started");
            List<Task<int>> tasks = new List<Task<int>>();

            try
            {

                tasks.Add(Task.Run(() => SendRequestInternal(connection, request)));

                var results = await Task.WhenAll(tasks);

                foreach (var item in results)
                {
                    Interlocked.Add(ref _sum, item);
                }
                Console.WriteLine("ConnectAndSendRequestParallelAsync ended");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static int SendRequestInternal(Connection connection, Request request)
        {
            try
            {
                return connection.runCommand(request.Command);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
