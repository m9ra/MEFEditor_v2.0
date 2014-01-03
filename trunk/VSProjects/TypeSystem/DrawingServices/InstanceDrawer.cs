using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drawing;

using Analyzing;
using Analyzing.Editing;

using TypeSystem.Runtime;
using TypeSystem.DrawingServices;

namespace TypeSystem
{
    public class InstanceDrawer
    {
        private readonly RuntimeTypeDefinition _definition;

        public readonly DrawedInstance Instance;

        internal InstanceDrawer(RuntimeTypeDefinition definition, DrawedInstance instance)
        {
            _definition = definition;
            Instance = instance;
        }


        /// <summary>
        /// TODO better join point resolvings
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public JoinDefinition DrawJoin(ConnectorDefinition from, ConnectorDefinition to)
        {
            var join = new JoinDefinition(from, to);

            Instance.Context.DrawJoin(join);

            return join;
        }


        /// <summary>
        /// Get instance drawing which drawing pipeline will be processed
        /// (before or after current drawing is processed)
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public DrawedInstance GetInstanceDrawing(Instance instance)
        {
            return Instance.Pipeline.GetDrawing(instance);
        }

        public EditDefinition CreateEditDefinition(Edit edit)
        {
            return new EditDefinition(edit.Name, (view) => runEdit(edit, view as EditView), () => false);
        }


        private EditViewBase runEdit(Edit edit, EditView view)
        {
            return _definition.RunEdit(Instance.WrappedInstance, edit, view);
        }


        public void ForceShow()
        {
            Instance.CommitDrawing();
        }

        public void SetProperty(string property, string value)
        {
            Instance.SetProperty(property, value);
        }

        public void PublishField(string p, Runtime.Field field)
        {
            Instance.PublishField(p, field);
        }

        public SlotDefinition AddSlot()
        {
            return Instance.AddSlot();
        }
    }
}
