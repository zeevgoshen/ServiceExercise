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
        private static Connection connection = new Connection();
        private static int currentConnectionsNumber = 0;


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

            //startRunAsyncTask(request);Task<int> result = 

            int number = 0;
            while (currentConnectionsNumber < maxConnections)
            {
                //connection.runCommand(request.Command);
                number += runAsyncTask(request).Result;
            }


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

        private async Task<int> runAsyncTask(Request request)
        {

            try
            {


                await Task.Run(() =>
                {
                    return connection.runCommand(request.Command);
                });

                return Task.Result;

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
