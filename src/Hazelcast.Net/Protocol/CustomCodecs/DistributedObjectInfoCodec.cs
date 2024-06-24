﻿// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
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
//   Hazelcast Client Protocol Code Generator @de0885d
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
    internal static class DistributedObjectInfoCodec
    {

        public static void Encode(ClientMessage clientMessage, Hazelcast.Models.DistributedObjectInfo distributedObjectInfo)
        {
            clientMessage.Append(Frame.CreateBeginStruct());

            StringCodec.Encode(clientMessage, distributedObjectInfo.ServiceName);
            StringCodec.Encode(clientMessage, distributedObjectInfo.Name);

            clientMessage.Append(Frame.CreateEndStruct());
        }

        public static Hazelcast.Models.DistributedObjectInfo Decode(IEnumerator<Frame> iterator)
        {
            // begin frame
            iterator.Take();
            var serviceName = StringCodec.Decode(iterator);
            var name = StringCodec.Decode(iterator);

            iterator.SkipToStructEnd();
            return new Hazelcast.Models.DistributedObjectInfo(serviceName, name);
        }
    }
}

#pragma warning restore IDE0051 // Remove unused private members
