﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServiceExercise {
    public class Program {

        private const int CONNECTION_COUNT = 4;
        private const int CLIENTS_COUNT = 3;

        static void Main(string[] args) {

            RequestsService service = new RequestsService(CONNECTION_COUNT, CLIENTS_COUNT);

            List<Task> clientTasks = new List<Task>();

            for (int i = 0; i < CLIENTS_COUNT; i++) {
                clientTasks.Add(Task.Run(() => {
                    Client client = new Client(service);
                    client.sendRequests();
                }));
            }

            Task.WaitAll(clientTasks.ToArray());

            int result = service.getSummary();

            Console.WriteLine($"Result is {result}");
        }
    }
}
