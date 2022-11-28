using System;
using System.Collections.Generic;
using System.Text;

namespace NetModule.Messages
{
    /// <summary>
    /// This message class contained two extra information in type T1 and type T2.
    /// </summary>
    /// <typeparam name="T1">The type of left information.</typeparam>
    /// <typeparam name="T2">The type of right information.</typeparam>
    public class DoubleInfoMsg<T1, T2> : BaseMsg where T1 : ISerializable where T2 : ISerializable
    {
        protected DoubleInfoMsg()
        {

        }
        /// <summary>
        /// Initial the left value and the right value.
        /// </summary>
        /// <param name="leftValue">The left value.</param>
        /// <param name="rightValue">The right value.</param>
        public DoubleInfoMsg(T1 leftValue, T2 rightValue)
        {
            LeftValue = leftValue;
            RightValue = rightValue;
        }
        /// <summary>
        /// The left value.
        /// </summary>
        public T1 LeftValue { get; set; }
        /// <summary>
        /// The right value.
        /// </summary>
        public T2 RightValue { get; set; }

        public override ushort InfoLength
        {
            get
            {
                return (ushort)(2 * sizeof(int) + IdManager.GetGenericId(typeof(T1)).Length + IdManager.GetGenericId(typeof(T2)).Length
                    + LeftValue.InfoLength + RightValue.InfoLength);
            }
        }

        public override bool Deserialize(byte[] data, int startIndex, int endIndex)
        {
            try
            {
                int offset = startIndex;

                int length1 = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
                Type type1 = IdManager.GetGenericType(data, offset, out offset);
                LeftValue = (T1)GetInstanceOf(type1);
                bool resultA = LeftValue.Deserialize(data, offset, offset + length1);
                offset += length1;

                int length2 = BitConverter.ToInt32(data, offset);
                offset += sizeof(int);
                Type type2 = IdManager.GetGenericType(data, offset, out offset);
                RightValue = (T2)GetInstanceOf(type2);
                bool resultB = RightValue.Deserialize(data, offset, offset + length2);
                offset += length2;

                return resultA && resultB;
            }
            catch (DeserializeException e)
            {
                e.deserializationChain.Push(GetType());
                throw;
            }
            catch (Exception e)
            {
                DeserializeException exception = new DeserializeException("Deserialization failed.", e);
                exception.failedArray = new byte[endIndex - startIndex];
                Array.Copy(data, startIndex, exception.failedArray, 0, endIndex - startIndex);
                exception.deserializationChain.Push(GetType());
                throw exception;
            }
        }

        public override int Serialize(ref byte[] buffer, int startIndex)
        {
            int index = startIndex;

            int length1 = LeftValue.InfoLength;
            BitConverter.GetBytes(length1).CopyTo(buffer,index);
            index += sizeof(int);
            byte[] id1 = IdManager.GetGenericId(typeof(T1));
            id1.CopyTo(buffer, index);
            index += id1.Length;
            int count1 = LeftValue.Serialize(ref buffer, index);
            index += count1;

            int length2 = RightValue.InfoLength;
            BitConverter.GetBytes(length2).CopyTo(buffer, index);
            index += sizeof(int);
            byte[] id2 = IdManager.GetGenericId(typeof(T2));
            id2.CopyTo(buffer, index);
            index += id2.Length;
            int count2 = RightValue.Serialize(ref buffer, index);
            index += count2;

            return index - startIndex;
        }
    }
}
