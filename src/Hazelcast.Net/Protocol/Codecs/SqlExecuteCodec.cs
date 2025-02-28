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
    /// Starts execution of an SQL query (as of 4.2).
    ///</summary>
#if SERVER_CODEC
    internal static class SqlExecuteServerCodec
#else
    internal static class SqlExecuteCodec
#endif
    {
        public const int RequestMessageType = 2163712; // 0x210400
        public const int ResponseMessageType = 2163713; // 0x210401
        private const int RequestTimeoutMillisFieldOffset = Messaging.FrameFields.Offset.PartitionId + BytesExtensions.SizeOfInt;
        private const int RequestCursorBufferSizeFieldOffset = RequestTimeoutMillisFieldOffset + BytesExtensions.SizeOfLong;
        private const int RequestExpectedResultTypeFieldOffset = RequestCursorBufferSizeFieldOffset + BytesExtensions.SizeOfInt;
        private const int RequestSkipUpdateStatisticsFieldOffset = RequestExpectedResultTypeFieldOffset + BytesExtensions.SizeOfByte;
        private const int RequestInitialFrameSize = RequestSkipUpdateStatisticsFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseUpdateCountFieldOffset = Messaging.FrameFields.Offset.ResponseBackupAcks + BytesExtensions.SizeOfByte;
        private const int ResponseIsInfiniteRowsFieldOffset = ResponseUpdateCountFieldOffset + BytesExtensions.SizeOfLong;
        private const int ResponsePartitionArgumentIndexFieldOffset = ResponseIsInfiniteRowsFieldOffset + BytesExtensions.SizeOfBool;
        private const int ResponseInitialFrameSize = ResponsePartitionArgumentIndexFieldOffset + BytesExtensions.SizeOfInt;

#if SERVER_CODEC
        public sealed class RequestParameters
        {

            /// <summary>
            /// Query string.
            ///</summary>
            public string Sql { get; set; }

            /// <summary>
            /// Query parameters.
            ///</summary>
            public IList<IData> Parameters { get; set; }

            /// <summary>
            /// Timeout in milliseconds.
            ///</summary>
            public long TimeoutMillis { get; set; }

            /// <summary>
            /// Cursor buffer size.
            ///</summary>
            public int CursorBufferSize { get; set; }

            /// <summary>
            /// Schema name.
            ///</summary>
            public string Schema { get; set; }

            /// <summary>
            /// The expected result type. Possible values are:
            ///   ANY(0)
            ///   ROWS(1)
            ///   UPDATE_COUNT(2)
            ///</summary>
            public byte ExpectedResultType { get; set; }

            /// <summary>
            /// Query ID.
            ///</summary>
            public Hazelcast.Sql.SqlQueryId QueryId { get; set; }

            /// <summary>
            /// Flag to skip updating phone home statistics.
            ///</summary>
            public bool SkipUpdateStatistics { get; set; }

            /// <summary>
            /// <c>true</c> if the skipUpdateStatistics is received from the client, <c>false</c> otherwise.
            /// If this is false, skipUpdateStatistics has the default value for its type.
            /// </summary>
            public bool IsSkipUpdateStatisticsExists { get; set; }
        }
#endif

        public static ClientMessage EncodeRequest(string sql, ICollection<IData> parameters, long timeoutMillis, int cursorBufferSize, string schema, byte expectedResultType, Hazelcast.Sql.SqlQueryId queryId, bool skipUpdateStatistics)
        {
            var clientMessage = new ClientMessage
            {
                IsRetryable = false,
                OperationName = "Sql.Execute"
            };
            var initialFrame = new Frame(new byte[RequestInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, RequestMessageType);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.PartitionId, -1);
            initialFrame.Bytes.WriteLongL(RequestTimeoutMillisFieldOffset, timeoutMillis);
            initialFrame.Bytes.WriteIntL(RequestCursorBufferSizeFieldOffset, cursorBufferSize);
            initialFrame.Bytes.WriteByteL(RequestExpectedResultTypeFieldOffset, expectedResultType);
            initialFrame.Bytes.WriteBoolL(RequestSkipUpdateStatisticsFieldOffset, skipUpdateStatistics);
            clientMessage.Append(initialFrame);
            StringCodec.Encode(clientMessage, sql);
            ListMultiFrameCodec.EncodeContainsNullable(clientMessage, parameters, DataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, schema, StringCodec.Encode);
            SqlQueryIdCodec.Encode(clientMessage, queryId);
            return clientMessage;
        }

#if SERVER_CODEC
        public static RequestParameters DecodeRequest(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var request = new RequestParameters();
            var initialFrame = iterator.Take();
            request.TimeoutMillis = initialFrame.Bytes.ReadLongL(RequestTimeoutMillisFieldOffset);
            request.CursorBufferSize = initialFrame.Bytes.ReadIntL(RequestCursorBufferSizeFieldOffset);
            request.ExpectedResultType = initialFrame.Bytes.ReadByteL(RequestExpectedResultTypeFieldOffset);
            if (initialFrame.Bytes.Length >= RequestSkipUpdateStatisticsFieldOffset + BytesExtensions.SizeOfBool)
            {
                request.SkipUpdateStatistics = initialFrame.Bytes.ReadBoolL(RequestSkipUpdateStatisticsFieldOffset);
                request.IsSkipUpdateStatisticsExists = true;
            }
            else request.IsSkipUpdateStatisticsExists = false;
            request.Sql = StringCodec.Decode(iterator);
            request.Parameters = ListMultiFrameCodec.DecodeContainsNullable(iterator, DataCodec.Decode);
            request.Schema = CodecUtil.DecodeNullable(iterator, StringCodec.Decode);
            request.QueryId = SqlQueryIdCodec.Decode(iterator);
            return request;
        }
#endif

        public sealed class ResponseParameters
        {

            /// <summary>
            /// Row metadata.
            ///</summary>
            public IList<Hazelcast.Sql.SqlColumnMetadata> RowMetadata { get; set; }

            /// <summary>
            /// Row page.
            ///</summary>
            public Hazelcast.Sql.SqlPage RowPage { get; set; }

            /// <summary>
            /// The number of updated rows.
            ///</summary>
            public long UpdateCount { get; set; }

            /// <summary>
            /// Error object.
            ///</summary>
            public Hazelcast.Sql.SqlError Error { get; set; }

            /// <summary>
            /// Is the result set unbounded.
            ///</summary>
            public bool IsInfiniteRows { get; set; }

            /// <summary>
            /// Index of the partition-determining argument, -1 if not applicable.
            ///</summary>
            public int PartitionArgumentIndex { get; set; }

            /// <summary>
            /// <c>true</c> if the isInfiniteRows is received from the member, <c>false</c> otherwise.
            /// If this is false, isInfiniteRows has the default value for its type.
            /// </summary>
            public bool IsIsInfiniteRowsExists { get; set; }

            /// <summary>
            /// <c>true</c> if the partitionArgumentIndex is received from the member, <c>false</c> otherwise.
            /// If this is false, partitionArgumentIndex has the default value for its type.
            /// </summary>
            public bool IsPartitionArgumentIndexExists { get; set; }
        }

#if SERVER_CODEC
        public static ClientMessage EncodeResponse(IList<Hazelcast.Sql.SqlColumnMetadata> rowMetadata, Hazelcast.Sql.SqlPage rowPage, long updateCount, Hazelcast.Sql.SqlError error, bool isInfiniteRows, int partitionArgumentIndex)
        {
            var clientMessage = new ClientMessage();
            var initialFrame = new Frame(new byte[ResponseInitialFrameSize], (FrameFlags) ClientMessageFlags.Unfragmented);
            initialFrame.Bytes.WriteIntL(Messaging.FrameFields.Offset.MessageType, ResponseMessageType);
            initialFrame.Bytes.WriteLongL(ResponseUpdateCountFieldOffset, updateCount);
            initialFrame.Bytes.WriteBoolL(ResponseIsInfiniteRowsFieldOffset, isInfiniteRows);
            initialFrame.Bytes.WriteIntL(ResponsePartitionArgumentIndexFieldOffset, partitionArgumentIndex);
            clientMessage.Append(initialFrame);
            ListMultiFrameCodec.EncodeNullable(clientMessage, rowMetadata, SqlColumnMetadataCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, rowPage, SqlPageCodec.Encode);
            CodecUtil.EncodeNullable(clientMessage, error, SqlErrorCodec.Encode);
            return clientMessage;
        }
#endif

        public static ResponseParameters DecodeResponse(ClientMessage clientMessage)
        {
            using var iterator = clientMessage.GetEnumerator();
            var response = new ResponseParameters();
            var initialFrame = iterator.Take();
            response.UpdateCount = initialFrame.Bytes.ReadLongL(ResponseUpdateCountFieldOffset);
            if (initialFrame.Bytes.Length >= ResponseIsInfiniteRowsFieldOffset + BytesExtensions.SizeOfBool)
            {
                response.IsInfiniteRows = initialFrame.Bytes.ReadBoolL(ResponseIsInfiniteRowsFieldOffset);
                response.IsIsInfiniteRowsExists = true;
            }
            else response.IsIsInfiniteRowsExists = false;
            if (initialFrame.Bytes.Length >= ResponsePartitionArgumentIndexFieldOffset + BytesExtensions.SizeOfInt)
            {
                response.PartitionArgumentIndex = initialFrame.Bytes.ReadIntL(ResponsePartitionArgumentIndexFieldOffset);
                response.IsPartitionArgumentIndexExists = true;
            }
            else response.IsPartitionArgumentIndexExists = false;
            response.RowMetadata = ListMultiFrameCodec.DecodeNullable(iterator, SqlColumnMetadataCodec.Decode);
            response.RowPage = CodecUtil.DecodeNullable(iterator, SqlPageCodec.Decode);
            response.Error = CodecUtil.DecodeNullable(iterator, SqlErrorCodec.Decode);
            return response;
        }

    }
}
