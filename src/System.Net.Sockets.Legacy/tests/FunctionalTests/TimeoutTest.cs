﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Xunit;

namespace System.Net.Sockets.Tests
{
    public class TimeoutTest
    {
        private const int TestPortBase = TestPortBases.Timeout;

        [Fact]
        public void GetAndSet_Success()
        {
            Socket socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            Assert.Equal(0, socket.ReceiveTimeout);
            socket.ReceiveTimeout = 100;
            Assert.Equal(100, socket.ReceiveTimeout);
        }

        [Fact]
        public void SocketSendTimeout_GetAndSet_Success()
        {
            Socket socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            Assert.Equal(0, socket.SendTimeout);
            socket.SendTimeout = 100;
            Assert.Equal(100, socket.SendTimeout);
        }

        [Fact]
        public void ReceiveTimesOut_Throws()
        {
            using (Socket localSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp))
            {
                using (Socket remoteSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp))
                {
                    localSocket.Bind(new IPEndPoint(IPAddress.IPv6Loopback, TestPortBase));
                    localSocket.Listen(1);
                    IAsyncResult localAsync = localSocket.BeginAccept(null, null);

                    remoteSocket.Connect(IPAddress.IPv6Loopback, TestPortBase);

                    Socket acceptedSocket = localSocket.EndAccept(localAsync);
                    acceptedSocket.ReceiveTimeout = 100;

                    SocketException sockEx = Assert.Throws<SocketException>( () =>
                    {
                        acceptedSocket.Receive(new byte[1]);
                    });
                    
                    Assert.Equal(SocketError.TimedOut, sockEx.SocketErrorCode);
                    Assert.True(acceptedSocket.Connected);
                }
            }
        }

        [Fact]
        public void SocketSendTimeout_Send_Success()
        {
            using (Socket localSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp))
            {
                using (Socket remoteSocket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp))
                {
                    localSocket.Bind(new IPEndPoint(IPAddress.IPv6Loopback, TestPortBase + 1));
                    localSocket.Listen(1);
                    IAsyncResult localAsync = localSocket.BeginAccept(null, null);

                    remoteSocket.Connect(IPAddress.IPv6Loopback, TestPortBase + 1);

                    Socket acceptedSocket = localSocket.EndAccept(localAsync);
                    acceptedSocket.SendTimeout = 100;

                    // Note that Send almost never times out because it only has to copy the data to the native buffer.
                    int bytes = acceptedSocket.Send(new byte[100]);
                    Assert.Equal(100, bytes);
                    Assert.True(acceptedSocket.Connected);
                }
            }
        }
    }
}
