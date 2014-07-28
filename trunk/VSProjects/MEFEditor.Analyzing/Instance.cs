using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Utilities;
using MEFEditor.Analyzing.Editing;
using MEFEditor.Analyzing.Execution;


namespace MEFEditor.Analyzing
{
    /// <summary>
    /// Abstract representation of objects that are used during interpretation.
    /// </summary>
    public abstract class Instance
    {
        /// <summary>
        /// Storage for instance edits.
        /// </summary>
        private readonly List<Edit> _edits = new List<Edit>();

        /// <summary>
        /// Edits provided only in context of attaching instance
        /// <remarks>It is useful for container-&gt;child based edits</remarks>.
        /// </summary>
        private readonly MultiDictionary<Instance, Edit> _attachedEdits = new MultiDictionary<Instance, Edit>();

        /// <summary>
        /// Block where instance has been created if available, <c>null</c> otherwise.
        /// </summary>
        private ExecutedBlock _creationBlock;

        /// <summary>
        /// Info describing instance type.
        /// </summary>
        public readonly InstanceInfo Info;

        /// <summary>
        /// Determine unique ID during analyzing context. During execution ID may changed.
        /// </summary>
        /// <value>The identifier.</value>
        public string ID { get; private set; }

        /// <summary>
        /// Determine that instance has been used as argument of entry method.
        /// </summary>
        /// <value><c>true</c> if this instance is entry instance; otherwise, <c>false</c>.</value>
        public bool IsEntryInstance { get; internal set; }

        /// <summary>
        /// Navigation action at creation instruction if available, <c>null</c> otherwise.
        /// </summary>
        /// <value>The creation navigation.</value>
        public NavigationAction CreationNavigation { get; private set; }

        /// <summary>
        /// Block where instance has been created if available, <c>null</c> otherwise.
        /// </summary>
        /// <value>The creation block.</value>
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
        /// Determine that instance is dirty. It means that its state may not be correctly analyzed.
        /// This is usually caused by unknown operation processing.
        /// </summary>
        /// <value><c>true</c> if this instance is dirty; otherwise, <c>false</c>.</value>
        public bool IsDirty { get; internal set; }

        /// <summary>
        /// Available edit actions for current instance.
        /// </summary>
        /// <value>The edits.</value>
        public IEnumerable<Edit> Edits { get { return _edits; } }

        /// <summary>
        /// Instances that has attached edits to current instance.
        /// </summary>
        /// <value>The attaching instances.</value>
        public IEnumerable<Instance> AttachingInstances { get { return _attachedEdits.Keys; } }

        /// <summary>
        /// Direct value representation of instance when overriden.
        /// </summary>
        /// <value>The direct value.</value>
        public abstract object DirectValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Instance"/> class.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <exception cref="System.ArgumentNullException">info</exception>
        internal Instance(InstanceInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException("info");
            }
            Info = info;
        }

        /// <summary>
        /// Adds the edit to instance.
        /// </summary>
        /// <param name="edit">The edit.</param>
        internal void AddEdit(Edit edit)
        {
            if (!edit.IsEmpty)
            {
                _edits.Add(edit);
            }
        }

        /// <summary>
        /// Attach the edit to current instance from attaching instance.
        /// Attached edit is valid only in context of attachingInstance.
        /// </summary>
        /// <param name="attachingInstance">The attaching instance.</param>
        /// <param name="attachedEdit">The attached edit.</param>
        internal void AttachEdit(Instance attachingInstance, Edit attachedEdit)
        {
            if (!attachedEdit.IsEmpty)
            {
                //only non-empty edits will be attached
                _attachedEdits.Add(attachingInstance, attachedEdit);
            }
        }

        /// <summary>
        /// Removes the specified edit.
        /// </summary>
        /// <param name="edit">The edit.</param>
        internal void RemoveEdit(Edit edit)
        {
            _edits.Remove(edit);
        }

        /// <summary>
        /// Gets the edits attached by given instance.
        /// </summary>
        /// <param name="attachingInstance">The attaching instance.</param>
        /// <returns>IEnumerable&lt;Edit&gt;.</returns>
        public IEnumerable<Edit> GetAttachedEdits(Instance attachingInstance)
        {
            return _attachedEdits.Get(attachingInstance);
        }

        /// <summary>
        /// Set default id for instance. Default ID can be overriden by hinted one.
        /// </summary>
        /// <param name="defaultID">ID used as default, if none better is hinted.</param>
        internal void SetDefaultID(string defaultID)
        {
            ID = defaultID;
        }

        /// <summary>
        /// Hints the identifier.
        /// </summary>
        /// <param name="hint">The hint.</param>
        /// <param name="context">The context.</param>
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

        /// <summary>
        /// Hints the creation navigation.
        /// </summary>
        /// <param name="navigationAction">The navigation action.</param>
        internal void HintCreationNavigation(NavigationAction navigationAction)
        {
            if (CreationNavigation != null)
                return;

            CreationNavigation = navigationAction;
        }
    }
}
