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
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Clustering;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization;
using Hazelcast.Serialization.Collections;
using Microsoft.Extensions.Logging;

namespace Hazelcast.DistributedObjects.Impl
{
    /// <summary>
    /// Implements <see cref="IHRingBuffer{TItem}"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    internal class HRingBuffer<TItem> : DistributedObjectBase, IHRingBuffer<TItem>
    {
        private long _capacity = -1;

        /// <summary>
        /// Initializes a new instance of the <see cref="HRingBuffer{TItem}"/> class.
        /// </summary>
        /// <param name="name">The unique name of the ring buffer.</param>
        /// <param name="factory">The factory owning this object.</param>
        /// <param name="cluster">The cluster.</param>
        /// <param name="serializationService">The serialization service.</param>
        /// <param name="loggerFactory">A logger factory.</param>
        public HRingBuffer(string name, DistributedObjectFactory factory, Cluster cluster, SerializationService serializationService, ILoggerFactory loggerFactory)
            : base(ServiceNames.RingBuffer, name, factory, cluster, serializationService, loggerFactory)
        {
            const int maxBatchSize = 1000;
            MaxBatchSize = maxBatchSize;
        }

        /// <inheritdoc />
        public int MaxBatchSize { get; }

        /// <inheritdoc />
        public async Task<long> AddAsync(TItem item)
            => await AddAsync(item, OverflowPolicy.Overwrite).CfAwait();

        /// <inheritdoc />
        public async Task<long> AddAsync(TItem item, OverflowPolicy overflowPolicy)
        {
            var itemData = ToSafeData(item);
            var requestMessage = RingbufferAddCodec.EncodeRequest(Name, (int) overflowPolicy, itemData);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return RingbufferAddCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<long> AddAllAsync(ICollection<TItem> items, OverflowPolicy overflowPolicy)
        {
            if (items.Count == 0) throw new ArgumentException("Cannot add zero items.", nameof(items));
            var itemsData = ToSafeData(items);

            var requestMessage = RingbufferAddAllCodec.EncodeRequest(Name, itemsData, (int) overflowPolicy);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return RingbufferAddAllCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<long> GetCapacityAsync()
        {
            if (_capacity != -1) return _capacity;

            var requestMessage = RingbufferCapacityCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return _capacity = RingbufferCapacityCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<long> GetHeadSequenceAsync()
        {
            var requestMessage = RingbufferHeadSequenceCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return RingbufferHeadSequenceCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<IRingBufferResultSet<TItem>> ReadManyWithResultSetAsync(long startSequence, int minCount, int maxCount, CancellationToken cancellationToken=default)
        {
            if (startSequence < 0) throw new ArgumentOutOfRangeException(nameof(startSequence));
            if (minCount < 0) throw new ArgumentOutOfRangeException(nameof(minCount), "The value of minCount must be equal to, or greater than, zero.");
            if (maxCount < minCount) throw new ArgumentOutOfRangeException(nameof(maxCount), "The value of maxCount must be greater than, or equal to, the value of minCount.");

            var capacity = await GetCapacityAsync().CfAwait();
            if (minCount > capacity) throw new ArgumentOutOfRangeException(nameof(minCount), "The value of minCount must be smaller than, or equal to, the capacity.");
            if (maxCount > MaxBatchSize) throw new ArgumentOutOfRangeException(nameof(maxCount), "The value of maxCount must be lower than, or equal to, the max batch size.");

            var requestMessage = RingbufferReadManyCodec.EncodeRequest(Name, startSequence, minCount, maxCount, null);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId, cancellationToken).CfAwait();
            var response = RingbufferReadManyCodec.DecodeResponse(responseMessage);
            var result = new RingBufferResultSet<TItem>(SerializationService, response.ItemSeqs, response.ReadCount, response.NextSeq);
            await result.AddAsync(response.Items).CfAwait();
            return result;
        }

        /// <inheritdoc />
        public async Task<IReadOnlyList<TItem>> ReadManyAsync(long startSequence, int minCount, int maxCount)
        {
            return await ReadManyWithResultSetAsync(startSequence, minCount, maxCount).CfAwait();
        }

        /// <inheritdoc />
        public async ValueTask<TItem> ReadOneAsync(long sequence)
        {
            if (sequence < 0) throw new ArgumentOutOfRangeException(nameof(sequence));

            var requestMessage = RingbufferReadOneCodec.EncodeRequest(Name, sequence);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            var response = RingbufferReadOneCodec.DecodeResponse(responseMessage).Response;
            return await ToObjectAsync<TItem>(response).CfAwait();
        }

        /// <inheritdoc />
        public async Task<long> GetRemainingCapacityAsync()
        {
            var requestMessage = RingbufferRemainingCapacityCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return RingbufferRemainingCapacityCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<long> GetSizeAsync()
        {
            var requestMessage = RingbufferSizeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return RingbufferSizeCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<long> GetTailSequenceAsync()
        {
            var requestMessage = RingbufferTailSequenceCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return RingbufferTailSequenceCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
