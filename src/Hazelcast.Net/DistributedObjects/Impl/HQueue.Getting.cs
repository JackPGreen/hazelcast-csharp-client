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
using System.Collections.Generic;
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Protocol.Codecs;
using Hazelcast.Serialization.Collections;

namespace Hazelcast.DistributedObjects.Impl
{
    internal partial class HQueue<T> // Getting
    {
        /// <inheritdoc />
        public override async Task<int> GetSizeAsync()
        {
            var requestMessage = QueueSizeCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return QueueSizeCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task<bool> ContainsAsync(T item)
        {
            var itemData = ToSafeData(item);
            var requestMessage = QueueContainsCodec.EncodeRequest(Name, itemData);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return QueueContainsCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task<IReadOnlyList<T>> GetAllAsync()
        {
            var requestMessage = QueueIteratorCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            var response = QueueIteratorCodec.DecodeResponse(responseMessage).Response;
            var result = new ReadOnlyLazyList<T>(SerializationService);
            await result.AddAsync(response).CfAwait();
            return result;
        }

        /// <inheritdoc />
        public override async Task<bool> ContainsAllAsync<TItem>(ICollection<TItem> items)
        {
            var itemsData = ToSafeData(items);
            var requestMessage = QueueContainsAllCodec.EncodeRequest(Name, itemsData);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return QueueContainsAllCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public override async Task<bool> IsEmptyAsync()
        {
            var requestMessage = QueueIsEmptyCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return QueueIsEmptyCodec.DecodeResponse(responseMessage).Response;
        }

        /// <inheritdoc />
        public async Task<int> GetRemainingCapacityAsync()
        {
            var requestMessage = QueueRemainingCapacityCodec.EncodeRequest(Name);
            var responseMessage = await Cluster.Messaging.SendToPartitionOwnerAsync(requestMessage, PartitionId).CfAwait();
            return QueueRemainingCapacityCodec.DecodeResponse(responseMessage).Response;
        }
    }
}
