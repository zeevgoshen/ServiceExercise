using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServiceExercise
{
    public sealed class RequestsService : IService
    {
        private static object syncRoot = new Object();
        private static RequestsService? instance;
        static int maxConnections = 0;

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
        public int getSummary()
        {
            throw new NotImplementedException();
        }

        // should not block
        public void sendRequest(Request request)
        {
            startRunAsyncTask(request);
            
        }

        private Task startRunAsyncTask(Request request)
        {
            return runAsyncTask(request);
        }

        private async Task runAsyncTask(Request request)
        {
            await Task.Run(() =>
            {
                Console.WriteLine(request.Command);
            });
        }
    }
}
