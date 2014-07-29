using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Drawing;

using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Editing;

using MEFEditor.TypeSystem.Runtime;
using MEFEditor.TypeSystem.DrawingServices;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Builder for creating <see cref="Instance" /> drawings. 
    /// </summary>
    public class InstanceDrawer
    {
        /// <summary>
        /// The built instance drawing.
        /// </summary>
        public readonly DrawedInstance Instance;

        /// <summary>
        /// Initializes a new instance of the <see cref="InstanceDrawer" /> class.
        /// </summary>
        /// <param name="instance">The instance drawing to be built.</param>
        internal InstanceDrawer(DrawedInstance instance)
        {
            Instance = instance;
        }


        /// <summary>
        /// Draw join between given connectors.
        /// </summary>
        /// <param name="from">Source connector.</param>
        /// <param name="to">Target connector.</param>
        /// <returns>Drawn join.</returns>
        public JoinDefinition DrawJoin(ConnectorDefinition from, ConnectorDefinition to)
        {
            var join = new JoinDefinition(from, to);

            Instance.Context.DrawJoin(join);

            return join;
        }


        /// <summary>
        /// Get instance drawing which drawing pipeline will be processed
        /// (before or after current drawing is processed).
        /// </summary>
        /// <param name="instance">The instance which drawing is requested.</param>
        /// <returns>Drawing of given instance.</returns>
        public DrawedInstance GetInstanceDrawing(Instance instance)
        {
            return Instance.Pipeline.GetDrawing(instance);
        }

        /// <summary>
        /// Creates representation of edit that can be displayed by MEFEditor.Drawing library.
        /// </summary>
        /// <param name="edit">The edit which representation will be created.</param>
        /// <returns>Created representation.</returns>
        public EditDefinition CreateEditDefinition(Edit edit)
        {
            return new EditDefinition(edit.Name, (view) => runEdit(edit, view as EditView), (v) => true);
        }

        /// <summary>
        /// Runs the given edit on given view.
        /// </summary>
        /// <param name="edit">The edit that will be.</param>
        /// <param name="view">View where edit will be applied.</param>
        /// <returns>EditViewBase.</returns>
        private EditViewBase runEdit(Edit edit, EditView view)
        {
            var runtime = Instance.Pipeline.Runtime;

            return runtime.RunEdit(edit, view);
        }

        /// <summary>
        /// Force showing instance in diagram. Even if
        /// it is not included.
        /// </summary>
        public void ForceShow()
        {
            Instance.CommitDrawing();
        }

        /// <summary>
        /// Sets value of given property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        public void SetProperty(string property, string value)
        {
            Instance.SetProperty(property, value);
        }

        /// <summary>
        /// Publishes the field so it will be available for drawing definition.
        /// </summary>
        /// <param name="publishingName">Name of property where published value will be stored.</param>
        /// <param name="field">The published field.</param>
        public void PublishField(string publishingName, Runtime.Field field)
        {
            Instance.PublishField(publishingName, field);
        }

        /// <summary>
        /// Adds the slot definition to instance drawing.
        /// </summary>
        /// <returns>Created slot.</returns>
        public SlotDefinition AddSlot()
        {
            return Instance.AddSlot();
        }
    }
}
