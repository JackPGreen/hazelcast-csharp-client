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

// This code file is heavily inspired from the Kerberos.NET library,
// which is Copyright (c) 2017 Steve Syfuhs and released under the MIT
// license at
//
// https://github.com/SteveSyfuhs/Kerberos.NET
//

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using static Hazelcast.Security.Win32.NativeMethods;

namespace Hazelcast.Security.Win32
{
    internal partial class SspiSecurityContext : IDisposable
    {
        private const int SECPKG_CRED_BOTH = 0x00000003;
        private const int SECURITY_NETWORK_DREP = 0x00;

        private const int MaxTokenSize = 16 * 1024;

        private const ContextFlag DefaultRequiredFlags =
                                    ContextFlag.Connection |
                                    ContextFlag.ReplayDetect |
                                    ContextFlag.SequenceDetect |
                                    ContextFlag.Confidentiality |
                                    ContextFlag.AllocateMemory |
                                    ContextFlag.Delegate |
                                    ContextFlag.InitExtendedError;

        private const ContextFlag DefaultServerRequiredFlags =
                                    DefaultRequiredFlags |
                                    ContextFlag.AcceptStream |
                                    ContextFlag.AcceptExtendedError;

        private SECURITY_HANDLE credentialsHandle = new SECURITY_HANDLE();
        private SECURITY_HANDLE securityContext = new SECURITY_HANDLE();

        private readonly HashSet<object> disposable = new HashSet<object>();

        private readonly Credential credential;
        private readonly ContextFlag clientFlags;
        private readonly ContextFlag serverFlags;

        public SspiSecurityContext(
            Credential credential,
            string package,
            ContextFlag clientFlags = DefaultRequiredFlags,
            ContextFlag serverFlags = DefaultServerRequiredFlags
        )
        {
            this.credential = credential;
            this.clientFlags = clientFlags;
            this.serverFlags = serverFlags;

            Package = package;
        }

        public bool Impersonating { get; private set; }

        public string Package { get; }

        public string UserName { get { return QueryContextAttributeAsString(SecurityContextAttribute.SECPKG_ATTR_NAMES); } }

        public unsafe string QueryContextAttributeAsString(SecurityContextAttribute attr)
        {
            SecPkgContext_SecString pBuffer = default(SecPkgContext_SecString);
            SecStatus status;
            string strValue = null;

            // see https://github.com/dotnet/docs/blob/main/docs/fundamentals/syslib-diagnostics/syslib0004.md
            // this only exists in .NET Framework and has been obsoleted
#if NETFRAMEWORK
            RuntimeHelpers.PrepareConstrainedRegions();
#endif
            try { }
            finally
            {
                status = QueryContextAttributesString(ref this.securityContext, attr, ref pBuffer);

                if (status == SecStatus.SEC_E_OK)
                {
                    try
                    {
                        strValue = Marshal.PtrToStringUni((IntPtr)pBuffer.sValue);
                    }
                    finally
                    {
                        FreeContextBuffer(pBuffer.sValue);
                    }
                }

                if (status != SecStatus.SEC_E_UNSUPPORTED_FUNCTION && status > SecStatus.SEC_E_ERROR)
                {
                    throw new Win32Exception((int)status);
                }
            }

            return strValue;
        }

        public ContextStatus InitializeSecurityContext(string targetName, byte[] serverResponse, out byte[] clientRequest)
        {
            var targetNameNormalized = targetName.ToLowerInvariant();

            clientRequest = null;

            // 1. acquire
            // 2. initialize
            // 3. ??

            SecStatus result = 0;

            int tokenSize = 0;

            SecBufferDesc clientToken = default(SecBufferDesc);

            try
            {
                do
                {
                    clientToken = new SecBufferDesc(tokenSize);

                    if (!credentialsHandle.IsSet || result == SecStatus.SEC_I_CONTINUE_NEEDED)
                    {
                        AcquireCredentials();
                    }

                    if (serverResponse == null)
                    {
                        result = InitializeSecurityContext_0(
                            ref credentialsHandle,
                            IntPtr.Zero,
                            targetNameNormalized,
                            clientFlags,
                            0,
                            SECURITY_NETWORK_DREP,
                            IntPtr.Zero,
                            0,
                            ref securityContext,
                            ref clientToken,
                            out ContextFlag ContextFlags,
                            IntPtr.Zero
                        );
                    }
                    else
                    {
                        var pInputBuffer = new SecBufferDesc(serverResponse);
                        {
                            IntPtr pExpiry = IntPtr.Zero;

                            result = InitializeSecurityContext_1(
                                ref credentialsHandle,
                                ref securityContext,
                                targetNameNormalized,
                                clientFlags,
                                0,
                                SECURITY_NETWORK_DREP,
                                ref pInputBuffer,
                                0,
                                ref securityContext,
                                ref clientToken,
                                out ContextFlag ContextFlags,
                                ref pExpiry
                            );
                        }
                    }

                    if (result == SecStatus.SEC_E_INSUFFICENT_MEMORY)
                    {
                        if (tokenSize > MaxTokenSize)
                        {
                            break;
                        }

                        tokenSize += 1000;
                    }
                }
                while (result == SecStatus.SEC_I_INCOMPLETE_CREDENTIALS || result == SecStatus.SEC_E_INSUFFICENT_MEMORY);

                if (result > SecStatus.SEC_E_ERROR)
                {
                    throw new Win32Exception((int)result);
                }

                clientRequest = clientToken.ReadBytes();

                if (result == SecStatus.SEC_I_CONTINUE_NEEDED)
                {
                    return ContextStatus.RequiresContinuation;
                }

                return ContextStatus.Accepted;
            }
            finally
            {
                clientToken.Dispose();
            }
        }

        public ContextStatus AcceptSecurityContext(byte[] clientRequest, out byte[] serverResponse)
        {
            serverResponse = null;

            if (!credentialsHandle.IsSet)
            {
                AcquireCredentials();
            }

            var pInput = new SecBufferDesc(clientRequest);

            var tokenSize = 0;
            SecBufferDesc pOutput = default(SecBufferDesc);

            try
            {
                SecStatus result;

                do
                {
                    pOutput = new SecBufferDesc(tokenSize);

                    if (!securityContext.IsSet)
                    {
                        result = AcceptSecurityContext_0(
                            ref credentialsHandle,
                            IntPtr.Zero,
                            ref pInput,
                            serverFlags,
                            SECURITY_NETWORK_DREP,
                            ref securityContext,
                            out pOutput,
                            out ContextFlag pfContextAttr,
                            out SECURITY_INTEGER ptsTimeStamp
                        );
                    }
                    else
                    {
                        result = AcceptSecurityContext_1(
                            ref credentialsHandle,
                            ref securityContext,
                            ref pInput,
                            serverFlags,
                            SECURITY_NETWORK_DREP,
                            ref securityContext,
                            out pOutput,
                            out ContextFlag pfContextAttr,
                            out SECURITY_INTEGER ptsTimeStamp
                        );
                    }

                    if (result == SecStatus.SEC_E_INSUFFICENT_MEMORY)
                    {
                        if (tokenSize > MaxTokenSize)
                        {
                            break;
                        }

                        tokenSize += 1000;
                    }
                }
                while (result == SecStatus.SEC_I_INCOMPLETE_CREDENTIALS || result == SecStatus.SEC_E_INSUFFICENT_MEMORY);

                TrackUnmanaged(securityContext);

                if (result > SecStatus.SEC_E_ERROR)
                {
                    throw new Win32Exception((int)result);
                }

                serverResponse = pOutput.ReadBytes();

                if (result == SecStatus.SEC_I_CONTINUE_NEEDED)
                {
                    return ContextStatus.RequiresContinuation;
                }

                return ContextStatus.Accepted;
            }
            finally
            {
                pInput.Dispose();
                pOutput.Dispose();
            }
        }

        private void TrackUnmanaged(object thing)
        {
            disposable.Add(thing);
        }

        private unsafe void AcquireCredentials()
        {
            CredentialHandle creds = credential.Structify();

            TrackUnmanaged(creds);

            SecStatus result;

// see https://github.com/dotnet/docs/blob/main/docs/fundamentals/syslib-diagnostics/syslib0004.md
// this only exists in .NET Framework and has been obsoleted
#if NETFRAMEWORK
            RuntimeHelpers.PrepareConstrainedRegions();
#endif
            try { }
            finally
            {
                result = AcquireCredentialsHandle(
                    null,
                    Package,
                    SECPKG_CRED_BOTH,
                    IntPtr.Zero,
                    (void*)creds.DangerousGetHandle(),
                    IntPtr.Zero,
                    IntPtr.Zero,
                    ref credentialsHandle,
                    IntPtr.Zero
               );
            }

            if (result != SecStatus.SEC_E_OK)
            {
                throw new Win32Exception((int)result);
            }

            TrackUnmanaged(credentialsHandle);
        }

        public unsafe void Dispose()
        {
            foreach (var thing in disposable)
            {
                if (thing is IDisposable managedDispose)
                {
                    managedDispose.Dispose();
                }
                else if (thing is SECURITY_HANDLE handle)
                {
                    DeleteSecurityContext(&handle);
                    FreeCredentialsHandle(&handle);
                }
                else if (thing is IntPtr pThing)
                {
                    Marshal.FreeHGlobal(pThing);
                }
            }
        }
    }
}
