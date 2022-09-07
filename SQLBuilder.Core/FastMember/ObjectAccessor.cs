using System;
using System.Dynamic;

namespace SQLBuilder.Core.FastMember
{
    /// <summary>
    /// Represents an individual object, allowing access to members by-name
    /// </summary>
    public abstract class ObjectAccessor
    {
        /// <summary>
        /// Get or Set the value of a named member for the underlying object
        /// </summary>
        public abstract object this[string name] { get; set; }

        /// <summary>
        /// The object represented by this instance
        /// </summary>
        public abstract object Target { get; }

        /// <summary>
        /// Use the target types definition of equality
        /// </summary>
        public override bool Equals(object obj) => Target.Equals(obj);

        /// <summary>
        /// Obtain the hash of the target object
        /// </summary>
        public override int GetHashCode() => Target.GetHashCode();

        /// <summary>
        /// Use the target's definition of a string representation
        /// </summary>
        public override string ToString() => Target.ToString();

        /// <summary>
        /// Wraps an individual object, allowing by-name access to that instance
        /// </summary>
        public static ObjectAccessor Create(object target) => Create(target, false);

        /// <summary>
        /// Wraps an individual object, allowing by-name access to that instance
        /// </summary>
        public static ObjectAccessor Create(object target, bool allowNonPublicAccessors)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            if (target is IDynamicMetaObjectProvider dlr)
                return new DynamicWrapper(dlr);

            return new TypeAccessorWrapper(target, TypeAccessor.Create(target.GetType(), allowNonPublicAccessors));
        }

        sealed class TypeAccessorWrapper : ObjectAccessor
        {
            private readonly object target;
            private readonly TypeAccessor accessor;
            public TypeAccessorWrapper(object target, TypeAccessor accessor)
            {
                this.target = target;
                this.accessor = accessor;
            }
            public override object this[string name]
            {
                get { return accessor[target, name]; }
                set { accessor[target, name] = value; }
            }
            public override object Target => target;
        }

        sealed class DynamicWrapper : ObjectAccessor
        {
            private readonly IDynamicMetaObjectProvider target;
            public override object Target => target;
            public DynamicWrapper(IDynamicMetaObjectProvider target) => this.target = target;
            public override object this[string name]
            {
                get { return CallSiteCache.GetValue(name, target); }
                set { CallSiteCache.SetValue(name, target, value); }
            }
        }
    }
}
