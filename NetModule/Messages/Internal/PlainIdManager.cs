using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NetModule.Messages.Internal
{
    internal class PlainIdManager:ISerializableItemIdManager
    {
        private static Type baseType = typeof(ISerializable);
        private static List<Type> serializables = new List<Type>();
        private static Dictionary<Type, ConstructorInfo> constructors = new Dictionary<Type, ConstructorInfo>();
        static PlainIdManager()
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type t in assembly.GetTypes())
                {
                    Type[] interfaceTypes = t.GetInterfaces();
                    if (!t.IsAbstract)
                    {
                        foreach (Type interfaceType in interfaceTypes)
                        {
                            if (interfaceType == baseType)
                            {
                                serializables.Add(t);
                                break;
                            }
                        }
                    }
                }
            }
        }
        public ConstructorInfo GetConstructor(Type type)
        {
            if (constructors.ContainsKey(type))
            {
                return constructors[type];
            }
            else
            {
                ConstructorInfo info = type.GetConstructor(new Type[0]);
                if (info == null)
                {
                    ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic);
                    foreach (ConstructorInfo c in constructors)
                    {
                        if (c.GetParameters().Length == 0)
                        {
                            info = c;
                            break;
                        }
                    }
                }
                if (info != null)
                {
                    constructors.Add(type, info);
                    return info;
                }
                else
                    throw new MissingMethodException("The type " + type.FullName + " doesn't contain a constructor with no parameters.");
            }
        }
        public ConstructorInfo GetConstructor(int id)
        {
            return GetConstructor(GetType(id));
        }
        public Type GetType(int id)
        {
            if (id < 0 || id > serializables.Count)
                throw new NotSupportedException("This id " + id + "of serializable item is not supported. ");
            return serializables[id];
        }
        public int GetId(Type type)
        {
            if (type.IsGenericType)
                type = type.GetGenericTypeDefinition();
            int ans = serializables.IndexOf(type);
            if (ans == -1)
            {
                throw new NotSupportedException("The class of type "+ type.FullName + " is not a sub class of " + baseType.FullName);
            }
            return ans;
        }

        public Type GetGenericType(byte[] msgData,int startIndex, out int endIndex)
        {
            Type type = GetType(BitConverter.ToInt32(msgData,startIndex));
            int offset = startIndex;
            offset += sizeof(int);
            if (type.IsGenericType)
            {
                Stack<KeyValuePair<Type, List<Type>>> genericTypes = new Stack<KeyValuePair<Type, List<Type>>>();

                genericTypes.Push(new KeyValuePair<Type, List<Type>>(type, new List<Type>(type.GetGenericArguments().Length)));
                while (genericTypes.Count > 0)
                {
                    KeyValuePair<Type, List<Type>> pair = genericTypes.Peek();
                    int argsCount = pair.Value.Capacity;
                    int found = pair.Value.Count;
                    while (found < argsCount)
                    {
                        Type t = GetType(BitConverter.ToInt32(msgData, offset));
                        offset += sizeof(int);
                        if (t.IsGenericType)
                        {
                            genericTypes.Push(new KeyValuePair<Type, List<Type>>(t, new List<Type>(t.GetGenericArguments().Length)));
                            break;
                        }
                        else
                        {
                            pair.Value.Add(t);
                            found++;
                        }
                    }
                    if (found == argsCount)
                    {
                        Type realType = genericTypes.Pop().Key.MakeGenericType(pair.Value.ToArray());
                        if (genericTypes.Count > 0)//还有其他的未完成寻找
                        {
                            genericTypes.Peek().Value.Add(realType);
                        }
                        else//这是最后一个了，直接赋值给结果
                            type = realType;
                    }
                }
            }
            endIndex = offset;
            return type;
        }

        public byte[] GetGenericId(Type type)
        {
            byte[] idArray;
            if (type.IsGenericType)
            {
                int arrayLength = 0;
                List<byte[]> byteArr = new List<byte[]>();
                byteArr.Add(BitConverter.GetBytes(GetId(type)));
                arrayLength += byteArr[0].Length;
                foreach(Type t in type.GetGenericArguments())
                {
                    byte[] data = GetGenericId(t);
                    byteArr.Add(data);
                    arrayLength += data.Length;
                }
                idArray = new byte[arrayLength];
                int offset = 0;
                foreach(byte[] arr in byteArr)
                {
                    arr.CopyTo(idArray, offset);
                    offset += arr.Length;
                }
            }
            else
            {
                idArray = BitConverter.GetBytes(GetId(type));
            }
            return idArray;
        }
    }
}
