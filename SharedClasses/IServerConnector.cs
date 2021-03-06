﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SharedClasses
{
    /// <summary>
    /// interface for WCF connections between Server and GUI
    /// </summary>
    [ServiceContract]
    public interface IServerConnector
    {
        [OperationContract]
        List<PacketDB> GetUniqueFileNames();

        [OperationContract]
        List<PacketDB> GetUniqueFileNamesByDate(string dt);

        [OperationContract]
        String GetFileContents(int id);

        [OperationContract]
        List<PacketDB> GetFileRevisions(String name);

        [OperationContract]
        int GetLastRevisionID(String name);

        [OperationContract]
        List<String> GetActiveConnections();

        [OperationContract]
        List<String> GetValidConnections();

        [OperationContract]
        List<String> GetLogs();

        [OperationContract]
        void injectConfig();
    }
}
