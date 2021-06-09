
using System;

namespace XIHServer
{
    public static class IdGenerater
    {
        public static ulong CreateUniqueId() {
            return BitConverter.ToUInt64(Guid.NewGuid().ToByteArray(), 0);
        }
    }
}
