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

// <auto-generated>
//   This code was generated by a tool.
//   Hazelcast Client Protocol Code Generator @c89bc95
//   https://github.com/hazelcast/hazelcast-client-protocol
//   Change to this file will be lost if the code is regenerated.
// </auto-generated>

#pragma warning disable IDE0051 // Remove unused private members
// ReSharper disable UnusedMember.Local
// ReSharper disable RedundantUsingDirective
// ReSharper disable CheckNamespace

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Hazelcast.Protocol.BuiltInCodecs;
using Hazelcast.Protocol.CustomCodecs;
using Hazelcast.Core;
using Hazelcast.Messaging;
using Hazelcast.Clustering;
using Hazelcast.Serialization;
using Microsoft.Extensions.Logging;

namespace Hazelcast.Protocol.Codecs
{
    /// <summary>
    /// Creates a session for the caller on the given CP group.
    ///</summary>
#if SERVER_CODEC
    internal static class CPSessionCreateSessionServerCodec
#else
    internal static class CPSessionCreateSessionCodec
#endif
    {
        public const int RequestMessageType = 2031872; // 0x1F0100
        public const int ResponseMessageType = 2031873; // 0x1F0101
        private const int RequestInitialFrameSize = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int ResponseSessionIdFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseTtlMillisFieldOffset = ResponseSessionIdFieldOffset + BytesExtensions.SizeOfLong;
        private const int ResponseHeartbeatMillisFieldOffset = ResponseTtlMillisFieldOffset + BytesExtensions.SizeOfLong;
        private const int ResponseInitialFrameSize = ResponseHeartbeatMillisFieldOffset + BytesExtensions.SizeOfLong;

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// ID of the CP group
            ///</summary>
            public Hazelcast.CP.CPGroupId GroupId { get; set; }

            /// <summary>
            /// Name of the caller HazelcastInstance
            ///</summary>
            public string EndpointName { get; set; }
        }
#endif

        public static ClientMessage EncodeRequest(Hazelcast.CP.CPGroupId groupId, string endpointName)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = true,
                OperationName = "CPSession.CreateSession"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            clientMessage.Append(initialFrame);
            RaftGroupIdCodec.Encode(clientMessage, groupId);
            StringCodec.Encode(clientMessage, endpointName);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            iterator.Take(); // empty initial frame
            request.GroupId = RaftGroupIdCodec.Decode(iterator);
            request.EndpointName = StringCodec.Decode(iterator);
            return request;
        }
#endif

        public sealed class ResponseParameters
        {

            /// <summary>
            /// Id of the session.
            ///</summary>
            public long SessionId { get; set; }

            /// <summary>
            /// Time to live value in milliseconds that must be respected by the caller.
            ///</summary>
            public long TtlMillis { get; set; }

            /// <summary>
            /// Time between heartbeats in milliseconds that must be respected by the caller.
            ///</summary>
            public long HeartbeatMillis { get; set; }
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse(long sessionId, long ttlMillis, long heartbeatMillis)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            initialFrame.Bytes.WriteLongL(ResponseSessionIdFieldOffset, sessionId);
            initialFrame.Bytes.WriteLongL(ResponseTtlMillisFieldOffset, ttlMillis);
            initialFrame.Bytes.WriteLongL(ResponseHeartbeatMillisFieldOffset, heartbeatMillis);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }
#endif

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.SessionId = initialFrame.Bytes.ReadLongL(ResponseSessionIdFieldOffset);
            response.TtlMillis = initialFrame.Bytes.ReadLongL(ResponseTtlMillisFieldOffset);
            response.HeartbeatMillis = initialFrame.Bytes.ReadLongL(ResponseHeartbeatMillisFieldOffset);
            return response;
        }

    }
}
