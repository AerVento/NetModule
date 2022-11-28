using System;
using System.Collections.Generic;
using System.Text;

namespace NetModule.Messages.SendingProtocol
{
    public class BasicProtocol : FormProtocol
    {
        public override ushort Identifier => 0x2a4f;

        private string version = "1.0.0.0";
        public override ushort Version {
            get
            {
                string[] strs = version.Split('.');
                return (ushort)(Convert.ToUInt16(strs[0]) << 3 + Convert.ToUInt16(strs[1]) << 2
                    + Convert.ToUInt16(strs[2]) << 1 + Convert.ToUInt16(strs[3]));
            }
        }

        public override int Length => sizeof(ushort) * 3;

        public override ushort GetCheckcode(byte[] data,int count)
        {
            ushort result = 0;
            for(int i = 0;i < count;i++)
            {
                byte tmp = data[i];
                while(tmp > 0)
                {
                    if (Convert.ToBoolean(tmp % 2))
                        result++;
                    tmp /= 2;
                }
            }
            return result;
        }
    }
}
