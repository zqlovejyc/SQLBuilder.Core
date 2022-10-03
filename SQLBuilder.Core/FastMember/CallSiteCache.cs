using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;

namespace SQLBuilder.Core.FastMember
{
    internal static class CallSiteCache
    {
        private static readonly ConcurrentDictionary<string, Lazy<CallSite<Func<CallSite, object, object>>>> getters = new();
        private static readonly ConcurrentDictionary<string, Lazy<CallSite<Func<CallSite, object, object, object>>>> setters = new();

        internal static object GetValue(string name, object target)
        {
            var callSite = getters.GetOrAdd(name, key =>
                new Lazy<CallSite<Func<CallSite, object, object>>>(() =>
                    CallSite<Func<CallSite, object, object>>.Create(
                         Binder.GetMember(CSharpBinderFlags.None, name, typeof(CallSiteCache),
                         new CSharpArgumentInfo[]
                         {
                             CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                         })))).Value;

            return callSite.Target(callSite, target);
        }

        internal static void SetValue(string name, object target, object value)
        {
            var callSite = setters.GetOrAdd(name, key =>
                new Lazy<CallSite<Func<CallSite, object, object, object>>>(() =>
                    CallSite<Func<CallSite, object, object, object>>.Create(
                        Binder.SetMember(CSharpBinderFlags.None, name, typeof(CallSiteCache),
                        new CSharpArgumentInfo[]
                        {
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                            CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.UseCompileTimeType,
                            null)
                        })))).Value;

            callSite.Target(callSite, target, value);
        }
    }
}