// Guids.cs
// MUST match guids.h
using System;

namespace MEFEditor.Plugin
{
    static class GuidList
    {
        public const string guidPluginPkgString = "341ff64d-d713-4c3d-a92d-85ac434393fd";
        public const string guidPluginCmdSetString = "7fb0aafd-a86f-4bfe-b8ef-047fe0fa1c4e";
        public const string guidToolWindowPersistanceString = "9358ccce-9353-4de9-9f65-28b3755709dc";

        public static readonly Guid guidPluginCmdSet = new Guid(guidPluginCmdSetString);
    };
}