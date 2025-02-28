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

namespace Hazelcast.Security
{
    /// <summary>
    /// Implements <see cref="ITokenCredentials"/> for simple token-based protocols.
    /// </summary>
    #if !NET8_0_OR_GREATER
    [Serializable]
#endif
    public class TokenCredentials : ITokenCredentials
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenCredentials"/> class with a token.
        /// </summary>
        /// <param name="token">The credentials token.</param>
        public TokenCredentials(byte[] token)
        {
            this.token = token ?? throw new ArgumentNullException(nameof(token));
        }

        /// <inheritdoc />
        public string Name => GetToken() == null ? "<empty>" : "<token>";

        private readonly byte[] token;

        /// <summary>
        /// Gets the token representing the credentials.
        /// </summary>
        public byte[] GetToken() => token;

        /// <inheritdoc />
        public override string ToString()
            => $"{nameof(TokenCredentials)} ({GetToken().Length} bytes)";
    }
}
