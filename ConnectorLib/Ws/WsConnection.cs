﻿using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ConnectorLib.Ws.Interfaces;
using EltraCommon.Contracts.Users;
using EltraCommon.Contracts.Ws;
using EltraCommon.Logger;
using Newtonsoft.Json;

namespace ConnectorLib.Ws
{
    class WsConnection : IConnection
    {
        #region Private fields

        private CancellationTokenSource _cancellationTokenSource;
        private CancellationTokenSource _disconnectTokenSource;

        #endregion

        #region Constructors

        public WsConnection(string uniqueId, string channelName)
        {
            BufferSize = 4096;

            Initialize();

            UniqueId = uniqueId;
            ChannelName = channelName;
        }

        #endregion

        #region Properties

        public int BufferSize { get; set; }

        public ClientWebSocket Socket { get; private set; }
        
        public string UniqueId { get; private set; }
        
        public string ChannelName { get; private set; }
        
        public string Url { get; private set; }
        
        public bool IsConnected
        {
            get
            {
                bool result = false;

                try
                {
                    result = (Socket.State == WebSocketState.Open);
                }
                catch(Exception e)
                {
                    MsgLogger.Exception($"{GetType().Name} - IsConnected", e);
                }

                return result;
            }
        }

        public bool IsDisconnecting
        {
            get
            {
                bool result = false;

                if (_disconnectTokenSource != null)
                {
                    if(_disconnectTokenSource.IsCancellationRequested)
                    {
                        if(Socket.State == WebSocketState.Open ||
                            Socket.State == WebSocketState.CloseSent ||
                            Socket.State == WebSocketState.CloseReceived)
                        {
                            result = true;
                        }
                    }
                }

                return result;
            }
        }

        public WebSocketCloseStatus? LastCloseStatus { get; set; }

        #endregion

        #region Methods

        private void Initialize()
        {
            Socket = new ClientWebSocket();
            
            LastCloseStatus = null;

            _cancellationTokenSource = new CancellationTokenSource();
            _disconnectTokenSource = new CancellationTokenSource();
        }

        public async Task<bool> Connect(string url)
        {
            bool result = false;

            try
            {
                Url = url;

                Initialize();

                await Socket.ConnectAsync(new Uri(Url), _cancellationTokenSource.Token);

                result = (Socket.State == WebSocketState.Open);
            }
            catch(WebSocketException e)
            {
                MsgLogger.Exception($"{GetType().Name} - Connect", e);

                HandleBrokenConnection(e.WebSocketErrorCode);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Connect", e);
            }

            return result;
        }

        public async Task<bool> Abort()
        {
            bool result = false;

            _cancellationTokenSource.Cancel();

            if (IsConnected)
            {
                result = await Disconnect();
            }

            return result;
        }

        public async Task<bool> Disconnect()
        {
            bool result = false;

            try
            {
                if (IsConnected)
                {
                    if (!_disconnectTokenSource.IsCancellationRequested)
                    {
                        _disconnectTokenSource.Cancel();
                    }

                    if (Socket.State == WebSocketState.Open)
                    {
                        await Socket.CloseAsync(WebSocketCloseStatus.Empty, "", _cancellationTokenSource.Token);
                    }
                    else if (Socket.State == WebSocketState.Closed)
                    {
                        MsgLogger.WriteDebug($"{GetType().Name} - Disconnect", $"Disconnect, state = {Socket.State}");
                    }

                    if (Socket.State == WebSocketState.Closed)
                    {
                        result = true;
                    }
                }
                else
                {
                    MsgLogger.WriteDebug($"{GetType().Name} - Disconnect", $"Disconnect, state = {Socket.State}");
                }
            }
            catch (WebSocketException e)
            {
                MsgLogger.Exception($"{GetType().Name} - Disconnect", e);

                HandleBrokenConnection(e.WebSocketErrorCode);
            }
            catch (Exception e)
            {
                unchecked
                {
                    if (e.HResult == (int)0x80131620)
                    {
                        MsgLogger.WriteError($"{GetType().Name} - Disconnect", "connection failed, reconnect");

                        Initialize();
                    }
                    else
                    {
                        MsgLogger.Exception($"{GetType().Name} - Disconnect", e);
                    }
                }
            }

            return result;
        }

        public async Task<bool> Send(UserIdentity identity, string typeName, string data)
        {
            bool result = false;

            try
            {
                var wsMessage = new WsMessage()
                {
                    Identity = identity,
                    ChannelName = ChannelName,
                    TypeName = typeName,
                    Data = data
                };

                var json = JsonConvert.SerializeObject(wsMessage);
                var buffer = Encoding.UTF8.GetBytes(json);

                if (!Socket.CloseStatus.HasValue && Socket.State == WebSocketState.Open)
                {
                    await Socket.SendAsync(new ArraySegment<byte>(buffer, 0, buffer.Length), WebSocketMessageType.Text, true, _cancellationTokenSource.Token);

                    result = true;
                }
            }
            catch (WebSocketException e)
            {
                MsgLogger.Exception($"{GetType().Name} - Send", e);

                HandleBrokenConnection(e.WebSocketErrorCode);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Send", e);
            }

            return result;
        }

        private async void HandleBrokenConnection(WebSocketError errorCode)
        {
            MsgLogger.WriteError($"{GetType().Name} - HandleBrokenConnection", $"Connection to '{Url}' failed, error code = {errorCode}");

            if (errorCode == WebSocketError.ConnectionClosedPrematurely && Socket.State == WebSocketState.Open)
            {
                MsgLogger.WriteLine("try recover connection ...");

                Initialize();
            }

            await Task.Delay(100);
        }

        public async Task<bool> Send<T>(UserIdentity identity, T obj)
        {
            bool result = false;

            try
            {
                if(await Send(identity, typeof(T).FullName, JsonConvert.SerializeObject(obj)))
                {
                    if(obj is WsMessageAck)
                    {
                        result = true;
                    }
                    else
                    { 
                        var ack = await ReadWsMessage();

                        if(ack!=null && ack.Data=="ACK")
                        {
                            result = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Send", e);
            }

            return result;
        }

        private async Task<WsMessage> ReadWsMessage()
        {
            WsMessage result = null;

            var json = await ReadMessage();

            if (!string.IsNullOrEmpty(json))
            {
                var msg = JsonConvert.DeserializeObject<WsMessage>(json);

                if (msg is WsMessage)
                {
                    if(msg.TypeName == typeof(WsMessage).FullName)
                    {
                        result = JsonConvert.DeserializeObject<WsMessage>(msg.Data);
                    }
                    else
                    {
                        result = msg;
                    }
                }
            }

            return result;
        }

        private async Task<string> ReadMessage()
        {
            string result = string.Empty;
            int bufferSize = BufferSize;
            var buffer = new byte[bufferSize];
            var segment = new ArraySegment<byte>(buffer);
            var fullBuffer = new byte[bufferSize];
            int offset = 0;
            int fullBufferSize = 0;

            try
            {
                if (Socket.State == WebSocketState.Open)
                {
                    var receiveResult = await Socket.ReceiveAsync(segment, _cancellationTokenSource.Token);

                    while (!receiveResult.CloseStatus.HasValue)
                    {
                        if (receiveResult.EndOfMessage)
                        {
                            if (fullBufferSize > 0)
                            {
                                if (fullBuffer.Length < receiveResult.Count + offset)
                                {
                                    Array.Resize(ref fullBuffer, receiveResult.Count + offset);
                                }

                                Array.Copy(buffer, 0, fullBuffer, offset, receiveResult.Count);

                                offset += receiveResult.Count;
                                fullBufferSize += receiveResult.Count;
                            }
                            else
                            {
                                if (fullBuffer.Length < receiveResult.Count)
                                {
                                    Array.Resize(ref fullBuffer, receiveResult.Count);
                                }

                                Array.Copy(buffer, 0, fullBuffer, 0, receiveResult.Count);
                                fullBufferSize = receiveResult.Count;
                            }

                            result = Encoding.UTF8.GetString(fullBuffer, 0, fullBufferSize);

                            if (!string.IsNullOrEmpty(result))
                            {
                                break;
                            }

                            buffer = new byte[bufferSize];
                            offset = 0;
                            fullBufferSize = 0;
                        }
                        else
                        {
                            if (fullBuffer.Length < receiveResult.Count + offset)
                            {
                                Array.Resize(ref fullBuffer, receiveResult.Count + offset);
                            }

                            Array.Copy(buffer, 0, fullBuffer, offset, receiveResult.Count);

                            offset += receiveResult.Count;
                            fullBufferSize += receiveResult.Count;
                        }

                        if (Socket.State == WebSocketState.Open)
                        {
                            receiveResult = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationTokenSource.Token);
                        }
                        else
                        {
                            MsgLogger.WriteError($"{GetType().Name} - ReadMessage", $"cannot read - socket state = {Socket.State}");
                            break;
                        }
                    }

                    if(receiveResult.MessageType == WebSocketMessageType.Close &&
                        (Socket.State == WebSocketState.CloseReceived || Socket.State == WebSocketState.Open))
                    {
                        await Socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Server closed connection", _cancellationTokenSource.Token);
                    }
                }
                else
                {
                    MsgLogger.WriteWarning($"{GetType().Name} - ReadMessage", $"cannot read - socket state = {Socket.State}");
                }
            }
            catch (WebSocketException e)
            {
                MsgLogger.Exception($"{GetType().Name} - ReadMessage", e.InnerException != null ? e.InnerException : e);
            }
            catch (InvalidOperationException e)
            {               
                MsgLogger.Exception($"{GetType().Name} - ReadMessage", e);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - ReadMessage", e);
            }

            return result;
        }

        public async Task<string> Receive()
        {
            string result = string.Empty;
            
            try
            {
                var wsMsg = await ReadWsMessage();

                if(wsMsg is WsMessageAck)
                {
                    result = wsMsg.Data;
                }
                else if(wsMsg!=null)
                {
                    if (await Send(new UserIdentity(), new WsMessageAck()))
                    {
                        result = wsMsg.Data;
                        LastCloseStatus = null;
                    }
                }
                else if (IsConnected)
                {
                    MsgLogger.WriteError($"{GetType().Name} - Receive", $"receive ws message failed!");
                }
                else
                {
                    MsgLogger.WriteLine($"receive ws message skipped, socket state = {Socket.State}!");
                }
            }
            catch (WebSocketException e)
            {
                MsgLogger.Exception($"{GetType().Name} - Receive", e);

                HandleBrokenConnection(e.WebSocketErrorCode);
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Receive", e);
            }

            return result;
        }

        public static bool IsJson(string json)
        {
            json = json.Trim();

            return json.StartsWith("{") && json.EndsWith("}")
                   || json.StartsWith("[") && json.EndsWith("]");
        }

        public async Task<T> Receive<T>()
        {
            T result = default;

            try
            {
                var json = await Receive();

                if (IsJson(json))
                {                 
                    result = JsonConvert.DeserializeObject<T>(json);                    
                }
                else if(string.IsNullOrEmpty(json) && IsConnected)
                {                    
                    MsgLogger.WriteLine($"empty string received, close status='{Socket.CloseStatus}'");
                }                
            }
            catch (Exception e)
            {
                MsgLogger.Exception($"{GetType().Name} - Receive", e);
            }

            return result;
        }

        #endregion
    }
}
