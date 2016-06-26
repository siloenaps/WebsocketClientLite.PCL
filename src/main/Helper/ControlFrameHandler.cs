﻿using System.Diagnostics;
using System.Threading.Tasks;
using ISocketLite.PCL.Interface;
using WebsocketClientLite.PCL.Model;

namespace WebsocketClientLite.PCL.Helper
{
    internal class ControlFrameHandler
    {
        private readonly ITcpSocketClient _client;
        internal bool IsPingReceived { get; private set; }

        internal bool IsCloseReceived { get; private set; }

        private byte[] _pong;

        private bool _isReceivingPingData = false;
        private int _payloadLength = 0;
        private int _payloadPosition = 0;
        private bool _isNextBytePayloadLength = false;

        internal ControlFrameHandler(ITcpSocketClient client)
        {
            _client = client;
        }

        internal ControlFrameType CheckForPingOrCloseControlFrame(byte data)
        {
            if (_isReceivingPingData)
            {
                AddPingPayload(data);
                return ControlFrameType.Ping;
            }

            switch (data)
            {
                case 136:
                    IsCloseReceived = true;
                    return ControlFrameType.Close;
                case 137:
                    InitPingStart();
                    return ControlFrameType.Ping;
            }
            return ControlFrameType.None;
        }

        internal void AddPingPayload(byte data)
        {
            if (_isNextBytePayloadLength)
            {
                var b = data;
                if (b == 0)
                {
                    InitNoPing();
                    _pong = new byte[2] {138, 0};
                    SendPong();
                }
                else
                {
                    _payloadLength = b >> 1;
                    Debug.WriteLine($"Ping payload lenght: {_payloadLength}");
                    _pong = new byte[_payloadLength];
                    _pong[0] = 138;
                    _payloadPosition = 1;
                    _isNextBytePayloadLength = false;
                }
            }
            else
            {
                if (_payloadPosition < _payloadLength)
                {
                    Debug.WriteLine("Ping payload received");
                    _pong[_payloadPosition] = data;
                    _payloadPosition++;
                }
                else
                {
                    InitNoPing();
                    SendPong();
                }
            }
        }

        private void SendPong()
        {
            //SendPongAsync();
            Task.Run(async () => await SendPongAsync()).ConfigureAwait(false);
        }

        private void InitPingStart()
        {
            Debug.WriteLine("Ping received");
            IsPingReceived = true;
            _isReceivingPingData = true;
            _isNextBytePayloadLength = true;
        }

        private void InitNoPing()
        {
            IsPingReceived = false;
            _isReceivingPingData = false;
            _isNextBytePayloadLength = false;
        }

        private async Task SendPongAsync()
        {
            await _client.WriteStream.WriteAsync(_pong, 0, _pong.Length);
            await _client.WriteStream.FlushAsync();
            Debug.WriteLine("Pong send");
        }
    }
}