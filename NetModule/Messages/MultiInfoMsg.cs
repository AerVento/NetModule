using NetModule.Messages.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace NetModule.Messages
{
    /// <summary>
    /// This class contained a list of serializable items to present extra information.
    /// </summary>
    public class MultiInfoMsg : BaseMsg,IEnumerable<ISerializable>
    {
        /// <summary>
        /// The list of serializable items.
        /// </summary>
        protected List<ISerializable> items = new List<ISerializable>();
        /// <summary>
        /// The list of serializable items.
        /// </summary>
        public List<ISerializable> Items => items;
        /// <summary>
        /// To create MultiInfoMsg with a empty list.
        /// </summary>
        public MultiInfoMsg()
        {

        }
        /// <summary>
        /// To initialize the list with a list of serializable items.
        /// </summary>
        /// <param name="items">A list of serializable items to be added.</param>
        public MultiInfoMsg(List<ISerializable> items)
        {
            foreach (ISerializable item in items)
            {
                this.items.Add(item);
            }
        }
        /// <summary>
        /// To initialize the list with a list of serializable items.
        /// </summary>
        /// <param name="items">A list of serializable items to be added.</param>
        public MultiInfoMsg(params ISerializable[] items)
        {
            foreach(ISerializable item in items)
            {
                this.items.Add(item);
            }
        }
        
        /// <summary>
        /// The count of the items.
        /// </summary>
        public int Count => items.Count;
        /// <summary>
        /// Add a serializable item to list.
        /// </summary>
        /// <param name="item">The T item.</param>
        public void Add(ISerializable item) => items.Add(item);
        /// <summary>
        /// Remove a serializable item from list.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        public void Remove(ISerializable item) => items.Remove(item);
        /// <summary>
        /// Remove a serializable item by index.
        /// </summary>
        /// <param name="index">The index of item to be removed.</param>
        public void RemoveAt(int index) => items.RemoveAt(index);
        /// <summary>
        /// Whether the list contained the item.
        /// </summary>
        /// <param name="item">The item to search.</param>
        public void Contains(ISerializable item) => items.Contains(item);
        /// <summary>
        /// To clear the list.
        /// </summary>
        public void Clear() => items.Clear();
        /// <summary>
        /// To iterate the list with action.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ForEach(Action<ISerializable> action) => items.ForEach(action);
        /// <summary>
        /// To find a item in list which make the delegate "match" returns true.
        /// </summary>
        /// <param name="match">The delagete.</param>
        public void Find(Predicate<ISerializable> match) => items.Find(match);
        
        /// <summary>
        /// Get a item and transform it to given type.
        /// </summary>
        /// <typeparam name="T">The given type.</typeparam>
        /// <param name="index">The index of the item.</param>
        /// <returns>The item with specific type, -or- null if the transform failed.</returns>
        /// <exception cref="ArgumentOutOfRangeException"> index is less than 0. -or- index is equal to or greater than count.</exception>
        public T Get<T>(int index) where T:ISerializable
        {
            return (T)items[index];
        }
        
        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The item.</returns>
        public ISerializable this[int index]
        {
            get => items[index];
            set => items[index] = value;
        }
        /// <summary>
        /// The length of serialized byte array.
        /// </summary>
        public override ushort InfoLength
        {
            get
            {
                ushort total = 0;
                foreach (ISerializable item in items)
                {
                    total += item.InfoLength;
                    total += (ushort)IdManager.GetGenericId(item.GetType()).Length;
                }
                //Every item in list need a int number to recognize the length of item when in a byte array.
                total += (ushort)(sizeof(int) * items.Count);
                return total;
            }
        }
        /// <summary>
        /// Read the data between the start index and end index, and use the data to initialize the value of current instance.
        /// </summary>
        /// <param name="data">The data array.</param>
        /// <param name="startIndex">The start index of useful data.</param>
        /// <param name="endIndex">The end index of useful data.</param>
        /// <returns>Whether the deserialization succeed.</returns>
        public override bool Deserialize(byte[] data,int startIndex,int endIndex)
        {
            try
            {
                int index = startIndex;
                while (index < endIndex)
                {
                    int offset = index;
                    int length = BitConverter.ToInt32(data, offset);
                    offset += sizeof(int);

                    Type type = IdManager.GetGenericType(data, offset, out offset);
                    ISerializable item = IdManager.GetConstructor(type).Invoke(new object[0]) as ISerializable;
                    
                    if (!item.Deserialize(data, offset, length + index))
                        return false;
                    items.Add(item);
                    index += length;
                }
                return true;
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
        /// <summary>
        /// Serialize this instance and write the byte data to buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write in.</param>
        /// <param name="startIndex">The start index of the buffer to write in. </param>
        /// <returns>The count of bytes have written. </returns>
        public override int Serialize(ref byte[] buffer, int startIndex)
        {
            int index = startIndex;
            foreach(ISerializable item in items)
            {
                byte[] id = IdManager.GetGenericId(item.GetType());
                int length = sizeof(int)+ id.Length + item.InfoLength;
                BitConverter.GetBytes(length).CopyTo(buffer, index);
                index += sizeof(int);
                id.CopyTo(buffer, index);
                index += id.Length;
                int data = item.Serialize(ref buffer, index);
                index += data;
            }
            return InfoLength;
        }

        public IEnumerator<ISerializable> GetEnumerator()
        {
            return ((IEnumerable<ISerializable>)items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)items).GetEnumerator();
        }
        
        public static implicit operator List<ISerializable>(MultiInfoMsg msg) => msg.items;
        public static implicit operator MultiInfoMsg(List<ISerializable> list) => new MultiInfoMsg(list);
    }
    /// <summary>
    /// This class contained a list of specific type which can be serialized to present extra information.
    /// </summary>
    /// <typeparam name="T">The contained type of the list.</typeparam>
    public class MultiInfoMsg<T> : BaseMsg, IEnumerable<T> where T:ISerializable
    {
        /// <summary>
        /// The list of the T items.
        /// </summary>
        protected List<T> items = new List<T>();
        /// <summary>
        /// The T type of the list.
        /// </summary>
        public List<T> Items => items;
        /// <summary>
        /// To create a MultiInfoMsg with a empty list.
        /// </summary>
        public MultiInfoMsg()
        {

        }
        /// <summary>
        /// To initialize the list with a list of T items.
        /// </summary>
        /// <param name="items">The list of T items.</param>
        public MultiInfoMsg(List<T> items)
        {
            foreach (T item in items)
            {
                this.items.Add(item);
            }
        }
        /// <summary>
        /// To initialize the list with a list of T items.
        /// </summary>
        /// <param name="items">The list of T items.</param>
        public MultiInfoMsg(params T[] items)
        {
            foreach (T item in items)
            {
                this.items.Add(item);
            }
        }
        /// <summary>
        /// The count of the list.
        /// </summary>
        public int Count => items.Count;
        /// <summary>
        /// Add a T item to list.
        /// </summary>
        /// <param name="item">The T item.</param>
        public void Add(T item) => items.Add(item);
        /// <summary>
        /// Remove a T item from list.
        /// </summary>
        /// <param name="item">The item to be removed.</param>
        public void Remove(T item) => items.Remove(item);
        /// <summary>
        /// Remove a T item by index.
        /// </summary>
        /// <param name="index">The index of item to be removed.</param>
        public void RemoveAt(int index) => items.RemoveAt(index);
        /// <summary>
        /// Whether the list contained the item.
        /// </summary>
        /// <param name="item">The item to search.</param>
        public void Contains(T item) => items.Contains(item);
        /// <summary>
        /// To clear the list.
        /// </summary>
        public void Clear() => items.Clear();
        /// <summary>
        /// To iterate the list with action.
        /// </summary>
        /// <param name="action">The action.</param>
        public void ForEach(Action<T> action) => items.ForEach(action);
        /// <summary>
        /// To find a item in list which make the delegate "match" returns true.
        /// </summary>
        /// <param name="match">The delagete.</param>
        public void Find(Predicate<T> match) => items.Find(match);

        /// <summary>
        /// Gets or sets the item at the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The T item.</returns>
        public T this[int index]
        {
            get => items[index];
            set => items[index] = value;
        }
        /// <summary>
        /// The length of serialized byte array.
        /// </summary>
        public override ushort InfoLength
        {
            get
            {
                ushort total = 0;
                foreach (ISerializable item in items)
                {
                    total += item.InfoLength;
                    total += (ushort)IdManager.GetGenericId(item.GetType()).Length;
                }
                //Every item in list need a int number to recognize the length of item when in a byte array.
                total += (ushort)(sizeof(int) * items.Count);
                return total;
            }
        }
        /// <summary>
        /// Read the data between the start index and end index, and use the data to initialize the value of current instance.
        /// </summary>
        /// <param name="data">The data array.</param>
        /// <param name="startIndex">The start index of useful data.</param>
        /// <param name="endIndex">The end index of useful data.</param>
        /// <returns>Whether the deserialization succeed.</returns>
        public override bool Deserialize(byte[] data, int startIndex, int endIndex)
        {
            try
            {
                int index = startIndex;
                while (index < endIndex)
                {
                    int offset = index;
                    int length = BitConverter.ToInt32(data, offset);
                    offset += sizeof(int);

                    Type type = IdManager.GetGenericType(data, offset, out offset);
                    T item = (T)GetInstanceOf(type);

                    if (!item.Deserialize(data, offset, length + index))
                        return false;
                    items.Add(item);
                    index += length;
                }
                return true;
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
        /// <summary>
        /// Serialize this instance and write the byte data to buffer.
        /// </summary>
        /// <param name="buffer">The buffer to write in.</param>
        /// <param name="startIndex">The start index of the buffer to write in. </param>
        /// <returns>The count of bytes have written. </returns>
        public override int Serialize(ref byte[] buffer, int startIndex)
        {
            int index = startIndex;
            foreach (T item in items)
            {
                byte[] id = IdManager.GetGenericId(item.GetType());
                int length = sizeof(int) + id.Length + item.InfoLength;
                BitConverter.GetBytes(length).CopyTo(buffer, index);
                index += sizeof(int);
                id.CopyTo(buffer, index);
                index += id.Length;
                int data = item.Serialize(ref buffer, index);
                index += data;
            }
            return InfoLength;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)items).GetEnumerator();
        }

        public static implicit operator List<T>(MultiInfoMsg<T> msg) => msg.items;
        public static implicit operator MultiInfoMsg<T>(List<T> list) => new MultiInfoMsg<T>(list);
    }
}
