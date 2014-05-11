using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Utilities;
using Analyzing.Editing;
using Analyzing.Execution;


namespace Analyzing
{
    public abstract class Instance
    {
        /// <summary>
        /// Storage for instance edits
        /// </summary>
        private readonly List<Edit> _edits = new List<Edit>();

        /// <summary>
        /// Edits provided only in context of attaching instance
        /// <remarks>It is usefull for container->child based edits</remarks>
        /// </summary>
        private readonly MultiDictionary<Instance, Edit> _attachedEdits = new MultiDictionary<Instance, Edit>();

        /// <summary>
        /// Block where instance has been created if available, <c>null</c> otherwise
        /// </summary>
        private ExecutedBlock _creationBlock;
        /// <summary>
        /// Info describing instance type
        /// </summary>
        public readonly InstanceInfo Info;

        /// <summary>
        /// Determine unique ID during analyzing context. During execution ID may changed.        
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// Navigation action at creation instruction if available, <c>null</c> otherwise
        /// </summary>
        public NavigationAction CreationNavigation { get; private set; }

        /// <summary>
        /// Block where instance has been created if available, <c>null</c> otherwise
        /// </summary>
        public ExecutedBlock CreationBlock
        {
            get { return _creationBlock; }
            internal set
            {
                if (_creationBlock != null)
                    return;
                _creationBlock = value;
            }
        }

        /// <summary>
        /// Determine that instnace is dirty. It means that its state may not be correctly analyzed.
        /// This is usually caused by unknown operation processing
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// Available edit actions for current instance
        /// </summary>
        public IEnumerable<Edit> Edits { get { return _edits; } }

        /// <summary>
        /// Instances that has attached edits to current instance
        /// </summary>
        public IEnumerable<Instance> AttachingInstances { get { return _attachedEdits.Keys; } }

        /// <summary>
        /// Direct value representation of instance when overriden
        /// </summary>
        public abstract object DirectValue { get; }

        internal Instance(InstanceInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            Info = info;
        }

        internal void AddEdit(Edit edit)
        {
            if (!edit.IsEmpty)
            {
                _edits.Add(edit);
            }
        }

        internal void AttachEdit(Instance attachingInstance, Edit attachedEdit)
        {
            if (!attachedEdit.IsEmpty)
            {
                //only non-empty edits will be attached
                _attachedEdits.Add(attachingInstance, attachedEdit);
            }
        }

        internal void RemoveEdit(Edit edit)
        {
            _edits.Remove(edit);
            //TODO remove attached edits
        }

        public IEnumerable<Edit> GetAttachedEdits(Instance attachingInstance)
        {
            return _attachedEdits.Get(attachingInstance);
        }

        /// <summary>
        /// Set default id for instance. Default ID can be overriden by hinted one.
        /// </summary>
        /// <param name="defaultID">ID used as default, if none better is hinted</param>
        internal void SetDefaultID(string defaultID)
        {
            ID = defaultID;
        }

        internal void HintID(string hint, AnalyzingContext context)
        {
            if (

                 hint.StartsWith("$") ||
                 !ID.StartsWith("$")
                )
                return;

            var idHint = context.Machine.CreateID(hint);
            var oldID = ID;
            ID = idHint;

            context.Machine.ReportIDChange(oldID);
        }

        internal void HintCreationNavigation(NavigationAction navigationAction)
        {
            if (CreationNavigation != null)
                return;

            CreationNavigation = navigationAction;
        }

    }
}
