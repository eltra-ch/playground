﻿using EltraCommon.Contracts.Users;
using System.Threading.Tasks;

namespace ConnectorLib.Ws.Interfaces
{
    interface IConnection
    {
        bool IsConnected { get; }
        bool IsDisconnecting { get; }
        Task<bool> Connect(string url);

        Task<bool> Abort();

        Task<bool> Disconnect();

        Task<bool> Send(UserIdentity identity, string typeName, string data);

        Task<bool> Send<T>(UserIdentity identity, T obj);

        Task<string> Receive();

        Task<T> Receive<T>();
    }
}
