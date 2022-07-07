using System;
using System.Collections.Concurrent;
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
        static ConcurrentBag<Task<int>> tasks = new ConcurrentBag<Task<int>>();
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
                Console.WriteLine("The sum result should be divided by the number of clients.");
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
                            Console.WriteLine($" ----------- Creating a new connection:  #{ i } - {connectionArray[i].GetHashCode()}.");
                        }
                    }
                }
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
                Console.WriteLine($"ConnectAndSendRequestParallelAsync started using connection { connection.GetHashCode() }");

                tasks.Add(Task.Run(() => Interlocked.Add(ref _sum, SendRequestInternal(connection, request))));
                t = Task.WhenAll(tasks);
                await t;

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
