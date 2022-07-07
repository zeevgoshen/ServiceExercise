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
        private int                         sum = 0;
        private Stopwatch                   watch = null;
        private Connection[]                connectionArray;
        private object                      syncLock = new object();
        private Task                        mainTask;
        private ConcurrentBag<Task<int>>    concurrentTasks = new ConcurrentBag<Task<int>>();
        private int                         numberOfClients = 0;

        public RequestsService(int _maxConnections, int _numberOfClients)
        {
            connectionArray     = new Connection[_maxConnections];
            watch               = Stopwatch.StartNew();
            numberOfClients     = _numberOfClients;
        }
 

        public int getSummary()
        {
            try
            {
                mainTask.Wait();

                if (mainTask.Status == TaskStatus.RanToCompletion)
                {
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Console.WriteLine($"Total execution time: { elapsedMs }");
                    Console.WriteLine("The sum result should be divided by the number of clients.");
                    Console.WriteLine($"{sum/numberOfClients} requests per client.");
                    return sum;
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

        private Connection CreateOrUseExistingConnection()
        {
            try
            {
                Random random = new Random();
                int i = random.Next(0, connectionArray.Length);

                if (connectionArray[i] == null)
                {
                    lock (syncLock)
                    {
                        if (connectionArray[i] == null)
                        {
                            connectionArray[i] = new Connection();
                            Console.WriteLine($"----------- Creating a new connection:  #{ i } - {connectionArray[i].GetHashCode()}.");
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

        private async Task ConnectAndSendRequestParallelAsync(Connection connection, Request request)
        {
            try
            {
                Console.WriteLine($"ConnectAndSendRequestParallelAsync started using connection { connection.GetHashCode() }");

                concurrentTasks.Add(Task.Run(() => Interlocked.Add(ref sum, SendRequestInternal(connection, request))));
                mainTask = Task.WhenAll(concurrentTasks);
                await mainTask;

                Console.WriteLine("ConnectAndSendRequestParallelAsync ended");
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        private static int SendRequestInternal(Connection connection, Request request)
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
