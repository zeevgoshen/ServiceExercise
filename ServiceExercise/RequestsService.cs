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
        static Task t;

        public RequestsService(int _MaxConnections)
        {
            connectionArray = new Connection[_MaxConnections];
            watch = Stopwatch.StartNew();
        }
 

        public int getSummary()

        {
            t.Wait();

            if (t.Status == TaskStatus.RanToCompletion)
            {
                watch.Stop();
                var elapsedMs = watch.ElapsedMilliseconds;
                Console.WriteLine($"Total execution time: { elapsedMs }");
                return _sum;
            }
            else
            {
                return -1;
            }
        }


        // should not block
        public async void sendRequest(Request request)
        {
            try
            {
                Console.WriteLine("sendRequest started");
             
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
            try
            {
                Random random = new Random();
                int i = random.Next(0, connectionArray.Length);

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
                //Console.WriteLine($"Using connection number - #{ i }.");
                return connectionArray[i];
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task ConnectAndSendRequestParallelAsync(Connection connection, Request request)
        {
            try
            {
                Console.WriteLine("ConnectAndSendRequestParallelAsync started");
                List<Task<int>> tasks = new List<Task<int>>();

                tasks.Add(Task.Run(() => Interlocked.Add(ref _sum, SendRequestInternal(connection, request))));

                t = Task.WhenAll(tasks);

                 
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
                int result = 0;
                
                if (request != null && connection != null)
                {
                    result = connection.runCommand(request.Command);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

    }
}
