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
using System.Collections.Generic;

namespace Hazelcast.Clustering.LoadBalancing
{
    /// <summary>
    /// Defines a service that can load-balance requests to different members of a cluster.
    /// </summary>
    public interface ILoadBalancer
    {
        /// <summary>
        /// Gets the number of members.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Selects a member.
        /// </summary>
        /// <returns>The unique identifier of the selected member, if any; otherwise <c>Guid.Empty</c>.</returns>
        Guid GetMember();

        /// <summary>
        /// Sets the members.
        /// </summary>
        /// <param name="memberIds">The identifiers of the members.</param>
        /// <remarks>
        /// <para>The set of members that fully replaces the existing members.</para>
        /// </remarks>
        void SetMembers(IEnumerable<Guid> memberIds);
    }
}
