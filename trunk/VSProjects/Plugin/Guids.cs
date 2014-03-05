// Guids.cs
// MUST match guids.h
using System;

namespace MEFEditor.Plugin
{
    static class GuidList
    {
        public const string guidPluginPkgString = "3c0d5fdb-865f-40a5-97f4-54b51a6b5c1d";
        public const string guidPluginCmdSetString = "24916ca4-7787-4681-9db4-aa9fe6403d0c";
        public const string guidToolWindowPersistanceString = "3493cb8a-1c51-4fdf-8765-8d8f3116438a";

        public static readonly Guid guidPluginCmdSet = new Guid(guidPluginCmdSetString);
    };
}