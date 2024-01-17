﻿// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

// <auto-generated>
//   This code was generated by a tool.
//   Hazelcast Client Protocol Code Generator @2e80de297
//   https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantUsingDirective
// ReSharper disable CheckNamespace

using System;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Protocol.CustomCodecs
{
    internal static class HazelcastJsonValueCodec
    {

        public static void Encode(ClientMessage clientMessage, Hazelcast.Core.HazelcastJsonValue hazelcastJsonValue)
        {
            clientMessage.Append(Frame.CreateBeginStruct());

            StringCodec.Encode(clientMessage, hazelcastJsonValue.Value);

            clientMessage.Append(Frame.CreateEndStruct());
        }

        public static Hazelcast.Core.HazelcastJsonValue Decode(IEnumerator<Frame> iterator)
        {
            // begin frame
            iterator.Take();
            var @value = StringCodec.Decode(iterator);

            iterator.SkipToStructEnd();
            return CustomTypeFactory.CreateHazelcastJsonValue(@value);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members
