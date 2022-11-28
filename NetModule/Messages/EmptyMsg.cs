namespace NetModule.Messages
{
    /// <summary>
    /// A empty message without extra information.
    /// </summary>
    public class EmptyMsg:BaseMsg
    {
        public EmptyMsg()
        {
        
        }

        public override ushort InfoLength => 0;

        public override bool Deserialize(byte[] data, int startIndex, int endIndex)
        {
            return true;
        }

        public override int Serialize(ref byte[] buffer, int startIndex)
        {
            return 0;
        }
    }
}
