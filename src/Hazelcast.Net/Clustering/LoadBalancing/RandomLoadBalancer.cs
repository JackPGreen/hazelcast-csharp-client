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
using Hazelcast.Core;

namespace Hazelcast.Clustering.LoadBalancing
{
    /// <summary>
    /// Represents a random load balancer.
    /// </summary>
    /// <remarks>
    /// <para>A random load balancer returns random members.</para>
    /// </remarks>
    public class RandomLoadBalancer : LoadBalancerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RandomLoadBalancer"/> class.
        /// </summary>
        public RandomLoadBalancer()
        { }

        /// <inheritdoc />
        public override Guid GetMember()
        {
            var members = Members;
            if (members == null || members.Count == 0) return default;
            return members[RandomProvider.Next(members.Count)];
        }
    }
}
