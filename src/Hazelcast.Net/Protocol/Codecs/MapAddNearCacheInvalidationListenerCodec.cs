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
    /// Adds listener to map. This listener will be used to listen near cache invalidation events.
    ///</summary>
#if SERVER_CODEC
    internal static class MapAddNearCacheInvalidationListenerServerCodec
#else
    internal static class MapAddNearCacheInvalidationListenerCodec
#endif
    {
        public const int RequestMessageType = 81664; // 0x013F00
        public const int ResponseMessageType = 81665; // 0x013F01
        private const int RequestListenerFlagsFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestLocalOnlyFieldOffset = RequestListenerFlagsFieldOffset + BytesExtensions.SizeOfInt;
        private const int RequestInitialFrameSize = RequestLocalOnlyFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseResponseFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseInitialFrameSize = ResponseResponseFieldOffset + BytesExtensions.SizeOfCodecGuid;
        private const int EventIMapInvalidationSourceUuidFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int EventIMapInvalidationPartitionUuidFieldOffset = EventIMapInvalidationSourceUuidFieldOffset + BytesExtensions.SizeOfCodecGuid;
        private const int EventIMapInvalidationSequenceFieldOffset = EventIMapInvalidationPartitionUuidFieldOffset + BytesExtensions.SizeOfCodecGuid;
        private const int EventIMapInvalidationInitialFrameSize = EventIMapInvalidationSequenceFieldOffset + BytesExtensions.SizeOfLong;
        private const int EventIMapInvalidationMessageType = 81666; // 0x013F02
        private const int EventIMapBatchInvalidationInitialFrameSize = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int EventIMapBatchInvalidationMessageType = 81667; // 0x013F03

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// name of the map
            ///</summary>
            public string Name { get; set; }

            /// <summary>
            /// flags of enabled listeners.
            ///</summary>
            public int ListenerFlags { get; set; }

            /// <summary>
            /// if true fires events that originated from this node only, otherwise fires all events
            ///</summary>
            public bool LocalOnly { get; set; }
        }
#endif

        public static ClientMessage EncodeRequest(string name, int listenerFlags, bool localOnly)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "Map.AddNearCacheInvalidationListener"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteIntL(RequestListenerFlagsFieldOffset, listenerFlags);
            initialFrame.Bytes.WriteBoolL(RequestLocalOnlyFieldOffset, localOnly);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, name);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.ListenerFlags = initialFrame.Bytes.ReadIntL(RequestListenerFlagsFieldOffset);
            request.LocalOnly = initialFrame.Bytes.ReadBoolL(RequestLocalOnlyFieldOffset);
            request.Name = StringCodec.Decode(iterator);
            return request;
        }
#endif

        public sealed class ResponseParameters
        {

            /// <summary>
            /// A unique string which is used as a key to remove the listener.
            ///</summary>
            public Guid Response { get; set; }
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse(Guid response)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            initialFrame.Bytes.WriteGuidL(ResponseResponseFieldOffset, response);
            clientMessage.Append(initialFrame);
            return clientMessage;
        }
#endif

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.Response = initialFrame.Bytes.ReadGuidL(ResponseResponseFieldOffset);
            return response;
        }

#if SERVER_CODEC
        public static ClientMessage EncodeIMapInvalidationEvent(IData key, Guid sourceUuid, Guid partitionUuid, long sequence)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[EventIMapInvalidationInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, EventIMapInvalidationMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteGuidL(EventIMapInvalidationSourceUuidFieldOffset, sourceUuid);
            initialFrame.Bytes.WriteGuidL(EventIMapInvalidationPartitionUuidFieldOffset, partitionUuid);
            initialFrame.Bytes.WriteLongL(EventIMapInvalidationSequenceFieldOffset, sequence);
            clientMessage.Append(initialFrame);
            clientMessage.Flags |= ClientMessageFlags.Event;
            CodecUtil.EncodeNullable(clientMessage, key, DataCodec.Encode);
            return clientMessage;
        }
        public static ClientMessage EncodeIMapBatchInvalidationEvent(ICollection<IData> keys, ICollection<Guid> sourceUuids, ICollection<Guid> partitionUuids, ICollection<long> sequences)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[EventIMapBatchInvalidationInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, EventIMapBatchInvalidationMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            clientMessage.Append(initialFrame);
            clientMessage.Flags |= ClientMessageFlags.Event;
            ListMultiFrameCodec.Encode(clientMessage, keys, DataCodec.Encode);
            ListUUIDCodec.Encode(clientMessage, sourceUuids);
            ListUUIDCodec.Encode(clientMessage, partitionUuids);
            ListLongCodec.Encode(clientMessage, sequences);
            return clientMessage;
        }
#endif
        public static ValueTask HandleEventAsync(ClientMessage clientMessage, Func<IData, Guid, Guid, long, object, ValueTask> handleIMapInvalidationEventAsync, Func<IList<IData>, IList<Guid>, IList<Guid>, IList<long>, object, ValueTask> handleIMapBatchInvalidationEventAsync, object state, ILoggerFactory loggerFactory)
        {
            using var iterator = clientMessage.GetEnumerator();
            var messageType = clientMessage.MessageType;
            if (messageType == EventIMapInvalidationMessageType)
            {
                var initialFrame = iterator.Take();
                var sourceUuid =  initialFrame.Bytes.ReadGuidL(EventIMapInvalidationSourceUuidFieldOffset);
                var partitionUuid =  initialFrame.Bytes.ReadGuidL(EventIMapInvalidationPartitionUuidFieldOffset);
                var sequence =  initialFrame.Bytes.ReadLongL(EventIMapInvalidationSequenceFieldOffset);
                var key = CodecUtil.DecodeNullable(iterator, DataCodec.Decode);
                return handleIMapInvalidationEventAsync(key, sourceUuid, partitionUuid, sequence, state);
            }
            if (messageType == EventIMapBatchInvalidationMessageType)
            {
                iterator.Take(); // empty initial frame
                var keys = ListMultiFrameCodec.Decode(iterator, DataCodec.Decode);
                var sourceUuids = ListUUIDCodec.Decode(iterator);
                var partitionUuids = ListUUIDCodec.Decode(iterator);
                var sequences = ListLongCodec.Decode(iterator);
                return handleIMapBatchInvalidationEventAsync(keys, sourceUuids, partitionUuids, sequences, state);
            }
            loggerFactory.CreateLogger(typeof(EventHandler)).LogDebug("Unknown message type received on event handler :" + messageType);
            return default;
        }
    }
}
