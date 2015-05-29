using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ServerService
{
    class ServerConnector : SharedClasses.IServerConnector
    {
        public override List<SharedClasses.PacketDB> GetUniqueFileNames()
        {
            return new DBConnect().GetUniqueFileNames();
        }

        public override List<SharedClasses.PacketDB> GetUniqueFileNames(string dt)
        {
            return new DBConnect().GetUniqueFileNames(dt);
        }
    }
}
