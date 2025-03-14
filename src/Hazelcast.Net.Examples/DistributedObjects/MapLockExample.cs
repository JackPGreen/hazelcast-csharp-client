﻿// Copyright (c) 2008-2025, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Threading.Tasks;
using Hazelcast.Core;

namespace Hazelcast.Examples.DistributedObjects
{
    // ReSharper disable once UnusedMember.Global
    public class MapLockExample
    {
        public static async Task Main(string[] args)
        {
            // uncomment and enable HzConsole to see the context changes
            //HzConsole.Configure<AsyncContext>(config => { config.SetMaxLevel(0); });

            var options = new HazelcastOptionsBuilder()
                .With(args)
                .WithConsoleLogger()
                .Build();

            // in a dependency-injection-based application, the logger factory would be
            // provided by the container, and disposed when the container is disposed. we
            // *want* to dispose the logger factory, in order to flush its buffers - otherwise,
            // some entries might get lost.
            //
            // in a non-DI scenario, we have to dispose it by ourselves. the *client* cannot
            // dispose it, because the client factory might create several clients... in fact,
            // anything that is "created" cannot really be "owned" by the client, nor the
            // factory, only by the creator - which may be the options
            //
            // disposing the singleton factory disposes the singleton, as long as the factory
            // owns it (true by default)
            //
            // TODO: consider disposing the options?
            using var used = options.LoggerFactory;

            // create an Hazelcast client and connect to a server running on localhost
            await using var client = await HazelcastClientFactory.StartNewClientAsync(options);

            // get the distributed map from the cluster
            await using var map = await client.GetMapAsync<string, string>("map-lock-example");

            // add value
            await map.SetAsync("key", "value");

            // locking in the current context
            await map.LockAsync("key");

            // start a task that immediately update the value, in a new context
            // because it is a new context it will wait until the lock is released!
            //
            // simply running the code here would break the example, as it would run
            // with the same context, i.e. the context that has the lock, so we need
            // to start it with a new context

            Task task;
            using (AsyncContext.New())
            {
                task = Task.Run(async () =>
                {
                    await map.SetAsync("key", "value1");
                    Console.WriteLine("Put new value");
                });
            }

            try
            {
                var value = await map.GetAsync("key");
                // pretend to do something with the value..
                await Task.Delay(5000);
                await map.SetAsync("key", "value2");
            }
            finally
            {
                await map.UnlockAsync("key");
            }

            // now wait for the background task
            await task;

            // report
            Console.WriteLine("New value (should be 'value1'): " + await map.GetAsync("key")); // should be value1

            // destroy the map
            await client.DestroyAsync(map);
        }
    }
}
