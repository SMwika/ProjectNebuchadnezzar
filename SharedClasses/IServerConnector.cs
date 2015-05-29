using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SharedClasses
{
    [ServiceContract]
    public interface IServerConnector
    {
        [OperationContract]
        List<PacketDB> GetUniqueFileNames();

        [OperationContract]
        List<PacketDB> GetUniqueFileNamesByDate(string dt);

        [OperationContract]
        String GetFileContents(int id);
    }
}
