using SQLBuilder.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SQLBuilder.Core.FastMember
{
    /// <summary>
    /// Represents an abstracted view of the members defined for a type
    /// </summary>
    public sealed class MemberSet : IEnumerable<Member>, IList<Member>
    {
        readonly Member[] members;
        internal MemberSet(Type type)
        {
            const BindingFlags PublicInstance = BindingFlags.Public | BindingFlags.Instance;

            members = type.GetTypeAndInterfaceProperties(PublicInstance)
                .Cast<MemberInfo>()
                .Concat(type.GetFields(PublicInstance).Cast<MemberInfo>())
                .Select(member => new Member(member)).ToArray();
        }

        /// <summary>
        /// Return a sequence of all defined members
        /// </summary>
        public IEnumerator<Member> GetEnumerator()
        {
            foreach (var member in members)
                yield return member;
        }

        /// <summary>
        /// Get a member by index
        /// </summary>
        public Member this[int index] => members[index];

        /// <summary>
        /// The number of members defined for this type
        /// </summary>
        public int Count => members.Length;

        Member IList<Member>.this[int index]
        {
            get { return members[index]; }
            set { throw new NotSupportedException(); }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
        bool ICollection<Member>.Remove(Member item) => throw new NotSupportedException();
        void ICollection<Member>.Add(Member item) => throw new NotSupportedException();
        void ICollection<Member>.Clear() => throw new NotSupportedException();
        void IList<Member>.RemoveAt(int index) => throw new NotSupportedException();
        void IList<Member>.Insert(int index, Member item) => throw new NotSupportedException();

        bool ICollection<Member>.Contains(Member item) => members.Contains(item);
        void ICollection<Member>.CopyTo(Member[] array, int arrayIndex) => members.CopyTo(array, arrayIndex);
        bool ICollection<Member>.IsReadOnly => true;
        int IList<Member>.IndexOf(Member member) => Array.IndexOf(members, member);
    }

    /// <summary>
    /// Represents an abstracted view of an individual member defined for a type
    /// </summary>
    public sealed class Member
    {
        private readonly MemberInfo member;

        internal Member(MemberInfo member) => this.member = member;

        /// <summary>
        /// The ordinal of this member among other members.
        /// Returns -1 in case the ordinal is not set.
        /// </summary>
        public int Ordinal
        {
            get
            {
                var ordinalAttr = member.CustomAttributes.FirstOrDefault(p => p.AttributeType == typeof(OrdinalAttribute));

                if (ordinalAttr == null)
                {
                    return -1;
                }

                // OrdinalAttribute class must have only one constructor with a single argument.
                return Convert.ToInt32(ordinalAttr.ConstructorArguments.Single().Value);
            }
        }

        /// <summary>
        /// The name of this member
        /// </summary>
        public string Name => member.Name;

        /// <summary>
        /// The type of value stored in this member
        /// </summary>
        public Type Type => member switch
        {
            FieldInfo fi => fi.FieldType,
            PropertyInfo pi => pi.PropertyType,
            _ => throw new NotSupportedException(member.GetType().Name)
        };

        /// <summary>
        /// The Type object through which this MemberInfo object was obtained.
        /// </summary>
        public Type ReflectedType => member.ReflectedType;

        /// <summary>
        /// The Type object for the class that declares this member.
        /// </summary>
        public Type DeclaringType => member.DeclaringType;

        /// <summary>
        /// The MemberInfo of this member
        /// </summary>
        public MemberInfo MemberInfo => member;

        /// <summary>
        /// Is the attribute specified defined on this type
        /// </summary>
        public bool IsDefined(Type attributeType) =>
            Attribute.IsDefined(member, attributeType);

        /// <summary>
        /// Is the attribute specified defined on this type
        /// </summary>
        public bool IsDefined<T>() where T : Attribute
            => Attribute.IsDefined(member, typeof(T));

        /// <summary>
        /// Getting Attribute Type
        /// </summary>
        public Attribute GetAttribute(Type attributeType, bool inherit)
            => Attribute.GetCustomAttribute(member, attributeType, inherit);

        /// <summary>
        /// Getting Attribute Type
        /// </summary>
        public Attribute GetAttribute<T>(bool inherit) where T : Attribute
            => Attribute.GetCustomAttribute(member, typeof(T), inherit);

        /// <summary>
        /// Getting Attribute Type
        /// </summary>
        public T GetTypeAttribute<T>(bool inherit) where T : Attribute
        {
            if (GetAttribute<T>(inherit) is T attribute)
                return attribute;

            return null;
        }

        /// <summary>
        /// Property Can Write
        /// </summary>
        public bool CanWrite =>
            member.MemberType switch
            {
                MemberTypes.Property => ((PropertyInfo)member).CanWrite,
                _ => throw new NotSupportedException(member.MemberType.ToString())
            };

        /// <summary>
        /// Property Can Read
        /// </summary>
        public bool CanRead =>
             member.MemberType switch
             {
                 MemberTypes.Property => ((PropertyInfo)member).CanRead,
                 _ => throw new NotSupportedException(member.MemberType.ToString())
             };
    }
}
