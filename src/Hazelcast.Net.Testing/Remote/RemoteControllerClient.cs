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
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hazelcast.Core;
using Thrift;
using Thrift.Protocol;

namespace Hazelcast.Testing.Remote
{
    /// <summary>
    /// Represents a remote controller client.
    /// </summary>
    public class RemoteControllerClient : RemoteController.Client, IRemoteControllerClient
    {
        private static FieldInfo _exceptionMessage;
        private readonly SemaphoreSlim _lock = new(1);

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteControllerClient"/> class.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        private RemoteControllerClient(TProtocol protocol)
            : base(protocol)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteControllerClient"/> class.
        /// </summary>
        /// <param name="inputProtocol">The input protocol.</param>
        /// <param name="outputProtocol">The output protocol.</param>
        private RemoteControllerClient(TProtocol inputProtocol, TProtocol outputProtocol)
            : base(inputProtocol, outputProtocol)
        { }

        /// <summary>
        /// Creates and connects a new remote controller client.
        /// </summary>
        /// <param name="rcHostAddress">The remote controller address.</param>
        /// <param name="port">The remote controller port.</param>
        /// <returns>A new remote controller client.</returns>
        public static Task<IRemoteControllerClient> CreateAsync(string rcHostAddress = "127.0.0.1", int port = 9701)
            => CreateAsync(IPAddress.Parse(rcHostAddress), port);
        
        /// <summary>
        /// Creates and connects a new remote controller client.
        /// </summary>
        /// <param name="rcHostAddress">The remote controller address.</param>
        /// <param name="port">The remote controller port.</param>
        /// <returns>A new remote controller client.</returns>
        public static async Task<IRemoteControllerClient> CreateAsync(IPAddress rcHostAddress, int port = 9701)
        {
            var configuration = new Thrift.TConfiguration();
            var tSocketTransport = new Thrift.Transport.Client.TSocketTransport(rcHostAddress, port, configuration);
            var transport = new Thrift.Transport.TFramedTransport(tSocketTransport);
            if (!transport.IsOpen)
            {
                await transport.OpenAsync(CancellationToken.None).CfAwait();
            }
            var protocol = new TBinaryProtocol(transport);
            return Create(protocol);
        }
        
        /// <summary>
        /// Creates a new remote controller client.
        /// </summary>
        /// <param name="protocol">The protocol.</param>
        /// <returns>A new remote controller client.</returns>
        public static IRemoteControllerClient Create(TProtocol protocol)
            => new RemoteControllerClient(protocol);

        /// <summary>
        /// Creates a new remote controller client.
        /// </summary>
        /// <param name="inputProtocol">The input protocol.</param>
        /// <param name="outputProtocol">The output protocol.</param>
        /// <returns>A new remote controller client.</returns>
        public static IRemoteControllerClient Create(TProtocol inputProtocol, TProtocol outputProtocol)
            => new RemoteControllerClient(inputProtocol, outputProtocol);

        private async Task WithLock(Func<CancellationToken, Task> action, CancellationToken cancellationToken)
        {
            await _lock.WaitAsync(cancellationToken).CfAwait();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                await action(cancellationToken).CfAwait();
            }
            catch (TException e)
            {
                // Thrift exceptions are weird and need to be "fixed"
                FixMessage(e);
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }
        
        private async Task<T> WithLock<T>(Func<CancellationToken, Task<T>> action, CancellationToken cancellationToken)
        {
            await _lock.WaitAsync(cancellationToken).CfAwait();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await action(cancellationToken).CfAwait();
            }
            catch (TException e)
            {
                // Thrift exceptions are weird and need to be "fixed"
                FixMessage(e);
                throw;
            }
            finally
            {
                _lock.Release();
            }
        }

        /// <summary>
        /// Fix the <see cref="TException"/> message so it appears correctly.
        /// </summary>
        /// <param name="thriftException">A <see cref="TException"/> instance.</param>
        private static void FixMessage(TException thriftException)
        {
            // the TException classes are generated by Thrift so we should not modify them,
            // yet their code is ugly C# which hides the actual Message property, which thus
            // remains empty... so we provide a equally ugly method to fix it.

            var thriftMessage = thriftException.GetType().GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic);
            var message = (string)thriftMessage?.GetValue(thriftException);

            if (string.IsNullOrWhiteSpace(message))
                message = "Exception of type 'Hazelcast.Testing.Remote.CloudException' was thrown (no message).";

            _exceptionMessage ??= typeof(Exception).GetField("_message", BindingFlags.Instance | BindingFlags.NonPublic);
            _exceptionMessage?.SetValue(thriftException, message);
        }

        /// <inheritdoc />
        public Task<bool> PingAsync(CancellationToken cancellationToken = default)
            => WithLock(ping, cancellationToken);

        /// <inheritdoc />
        public Task<bool> CleanAsync(CancellationToken cancellationToken = default)
            => WithLock(clean, cancellationToken);

        /// <inheritdoc />
        public async Task<bool> ExitAsync(CancellationToken cancellationToken = default)
        {
            var result = await WithLock(exit, cancellationToken).CfAwait();
            InputProtocol?.Transport?.Close();
            return result;
        }

        /// <inheritdoc />
        public Task<Cluster> CreateClusterAsync(string hzVersion, string xmlconfig, CancellationToken cancellationToken = default)
            => WithLock(token => createCluster(hzVersion, xmlconfig, token), cancellationToken);

        /// <inheritdoc />
        public Task<Cluster> CreateClusterKeepClusterNameAsync(string serverVersion, string serverConfiguration, CancellationToken cancellationToken = default)
            => WithLock(token => createClusterKeepClusterName(serverVersion, serverConfiguration, token), cancellationToken);

        /// <inheritdoc />
        public Task<Member> StartMemberAsync(string clusterId, CancellationToken cancellationToken = default)
            => WithLock(token => startMember(clusterId, token), cancellationToken);

        /// <inheritdoc />
        public Task<bool> ShutdownMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => shutdownMember(clusterId, memberId, token), cancellationToken);

        /// <inheritdoc />
        public Task<bool> TerminateMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => terminateMember(clusterId, memberId, token), cancellationToken);

        /// <inheritdoc />
        public Task<bool> SuspendMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => suspendMember(clusterId, memberId, token), cancellationToken);

        /// <inheritdoc />
        public Task<bool> ResumeMemberAsync(string clusterId, string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => resumeMember(clusterId, memberId, token), cancellationToken);

        /// <inheritdoc />
        public Task<bool> ShutdownClusterAsync(string clusterId, CancellationToken cancellationToken = default)
            => WithLock(token => shutdownCluster(clusterId, token), cancellationToken);

        /// <inheritdoc />
        public Task<bool> TerminateClusterAsync(string clusterId, CancellationToken cancellationToken = default)
            => WithLock(token => terminateCluster(clusterId, token), cancellationToken);

        /// <inheritdoc />
        public Task<Cluster> SplitMemberFromClusterAsync(string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => splitMemberFromCluster(memberId, token), cancellationToken);

        /// <inheritdoc />
        public Task<Cluster> MergeMemberToClusterAsync(string clusterId, string memberId, CancellationToken cancellationToken = default)
            => WithLock(token => mergeMemberToCluster(clusterId, memberId, token), cancellationToken);

        /// <inheritdoc />
        public Task<Response> ExecuteOnControllerAsync(string clusterId, string script, Lang lang, CancellationToken cancellationToken = default)
            => WithLock(token => executeOnController(clusterId, script, lang, token), cancellationToken);

        /// <inheritdoc />
        public Task LoginToCloudAsync(string baseUrl, string apiKey, string apiSecret, CancellationToken cancellationToken = default)
            => WithLock(token => loginToCloud(baseUrl, apiKey, apiSecret, token), cancellationToken);

        /// <inheritdoc />
        public Task LoginToCloudAsync(CancellationToken cancellationToken = default)
            => WithLock(loginToCloudUsingEnvironment, cancellationToken);

        /// <inheritdoc />
        public Task<CloudCluster> CreateCloudClusterAsync(string hazelcastVersion, bool isTlsEnabled, CancellationToken cancellationToken = default)
            => WithLock(token => createCloudCluster(hazelcastVersion, isTlsEnabled, token), cancellationToken);

        /// <inheritdoc />
        public Task<CloudCluster> GetCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default)
            => WithLock(token => getCloudCluster(cloudClusterId, token), cancellationToken);

        /// <inheritdoc />
        public Task<CloudCluster> StopCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default)
            => WithLock(token => stopCloudCluster(cloudClusterId, token), cancellationToken);

        /// <inheritdoc />
        public Task<CloudCluster> ResumeCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default)
            => WithLock(token => resumeCloudCluster(cloudClusterId, token), cancellationToken);

        /// <inheritdoc />
        public Task DeleteCloudClusterAsync(string cloudClusterId, CancellationToken cancellationToken = default)
            => WithLock(token => deleteCloudCluster(cloudClusterId, token), cancellationToken);
    }
}
