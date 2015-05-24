using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedClasses
{
    public enum WatcherInfoType
    {
        FILE_CREATED = 0x00,
        FILE_CHANGED = 0x01,
        FILE_DELETED = 0x02,
        FILE_RENAMED = 0x03
    }
}
