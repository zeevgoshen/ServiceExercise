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
        private static int                      _sum = 0;
        private Stopwatch                       watch = null;
        private static Connection[]             connectionArray;
        private object                          _lock = new object();
        private static Task                     allTasks;
        private static ConcurrentBag<Task<int>> concurrentTasks = new ConcurrentBag<Task<int>>();
        private static int                      NumberOfClients = 0;

        public RequestsService(int _MaxConnections, int _NumberOfClients)
        {
            connectionArray = new Connection[_MaxConnections];
            watch = Stopwatch.StartNew();
            NumberOfClients = _NumberOfClients;
        }
 

        public int getSummary()
        {
            try
            {
                allTasks.Wait();

                if (allTasks.Status == TaskStatus.RanToCompletion)
                {
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Console.WriteLine($"Total execution time: { elapsedMs }");
                    Console.WriteLine("The sum result should be divided by the number of clients.");
                    Console.WriteLine($"{_sum/NumberOfClients} requests per client.");
                    return _sum;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
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

                concurrentTasks.Add(Task.Run(() => Interlocked.Add(ref _sum, SendRequestInternal(connection, request))));
                allTasks = Task.WhenAll(concurrentTasks);
                await allTasks;

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
