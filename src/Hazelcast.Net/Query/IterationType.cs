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
namespace Hazelcast.Query
{
    /// <summary>
    /// To differentiate users selection on result collection on map-wide operations like values , keySet , query etc.
    /// </summary>
    public enum IterationType
    {
        /// <summary>
        /// Iterate over keys
        /// </summary>
        Key = 0,

        /// <summary>
        /// Iterate over values
        /// </summary>
        Value = 1,

        /// <summary>
        /// Iterate over whole entry (so key and value)
        /// </summary>
        Entry = 2
    }
}
