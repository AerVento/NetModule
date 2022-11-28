using System;
using System.Reflection;
namespace NetModule.Messages.Internal
{
    /// <summary>
    /// A strategy for giving serializable item a id.
    /// Every non-abstract class implements the interface ISerializable can be given a unique id through this interface.
    /// <see cref="ISerializable"/>
    /// </summary>
    public interface ISerializableItemIdManager
    {
        /// <summary>
        /// To get a type of serializable item by message id.
        /// </summary>
        /// <param name="id">The id of serializable item.</param>
        /// <returns></returns>The type of serializable item with specfic item id.
        /// <exception cref="NotSupportedException">Thrown when id is undefined. </exception>
        public Type GetType(int id);
        /// <summary>
        /// Get a specific generic type from current type data.
        /// </summary>
        /// <param name="msgData">The message data.</param>
        /// <param name="startIndex">The start index of generic type id data.</param>
        /// /// <param name="endIndex">The output end index of generic type id data.</param>
        /// <returns>The generic type.</returns>
        public Type GetGenericType(byte[] msgData, int startIndex, out int endIndex);
        /// <summary>
        /// To get id of serializable item by a type of class.
        /// </summary>
        /// <param name="type">The type of the class which implements the interface ISerializable.</param>
        /// <returns>The id of the serializable item.</returns>
        /// <exception cref="NotSupportedException">Thrown when the class in this type doesn't implements the interface ISerializable. </exception>
        public int GetId(Type type);
        /// <summary>
        /// To get a byte array which presentes the id of current generic type.
        /// </summary>
        /// <param name="type">The generic type.</param>
        /// <returns>The byte array presents the generic type.</returns>
        /// <exception cref="NotSupportedException">Thrown when the class in this type contained a generic type that doesn't implements the interface ISerializable. </exception>
        public byte[] GetGenericId(Type type);
        /// <summary>
        /// Get a parameterless constructor of serializable item by id.
        /// When the type is generic type, the constructor will be a generic defination.
        /// </summary>
        /// <param name="id">The id of the serializable item.</param>
        /// <returns>A generic type of constructor if the type is generic type, or just empty constructor.</returns>
        /// <exception cref="MissingMethodException">Thrown when no paramterless constructor is defined.</exception>
        /// <exception cref="NotSupportedException">Thrown when id is undefined. </exception>
        public ConstructorInfo GetConstructor(int id);
        /// <summary>
        /// Get a parameterless constructor of serializable item by type.
        /// When the type is generic type, the constructor will be a generic defination.
        /// </summary>
        /// <param name="type">The type of the serializable item.</param>
        /// <returns>A generic type of constructor if the type is generic type defination, or just empty constructor.</returns>
        /// <exception cref="MissingMethodException">Thrown when no paramterless constructor is defined.</exception>
        /// <exception cref="NotSupportedException">Thrown when the class in this type doesn't implements the interface ISerializable. </exception>
        public ConstructorInfo GetConstructor(Type type);
    }
}
