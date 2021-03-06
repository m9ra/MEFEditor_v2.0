﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;

using MEFEditor.Drawing;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Singleton used for forwarding user's IO between <see cref="MEFEditor.Drawing"/> library nad 
    /// <see cref="MEFEditor.TypeSystem.Runtime.RuntimeTypeDefinition"/> and their edits.
    /// </summary>
    public static class UserInteraction
    {
        /// <summary>
        /// Name of edit used for accepting by drop edit.
        /// </summary>
        public static readonly string AcceptEditName = ".accept";

        /// <summary>
        /// Name of edit used for excluding by drag edit.
        /// </summary>
        public static readonly string ExcludeName = ".exclude";

        /// <summary>
        /// Instance that is currently dragged.
        /// <remarks>Setter is required to be public for testing purposes</remarks>.
        /// </summary>
        /// <value>The dragged instance.</value>
        public static Instance DraggedInstance { get; set; }

        /// <summary>
        /// Event used for handlers fired when resource disposing can be done.
        /// Is supposed to be used by type definitions for disposing resources registered
        /// during analysis.
        /// <remarks>This typically belongs to composition point invalidation</remarks>
        /// </summary>
        public static event Action OnResourceDisposing;

        /// <summary>
        /// Fire events for resource disposing.
        /// </summary>
        public static void DisposeResources()
        {
            if (OnResourceDisposing != null)
                OnResourceDisposing();
        }
    }
}
