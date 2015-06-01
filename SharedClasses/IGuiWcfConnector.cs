using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SharedClasses
{
    [ServiceContract]
    public interface IGuiWcfConnector
    {
        [OperationContract]
        void SendLiverEvent(String evt);

        [OperationContract]
        void FlagLiverEvent();
    }
}
