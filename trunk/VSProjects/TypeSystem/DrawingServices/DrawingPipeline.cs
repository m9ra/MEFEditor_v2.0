using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Drawing;
using Analyzing;

using TypeSystem.Runtime;

namespace TypeSystem.DrawingServices
{
    /// <summary>
    /// Pipeline that is processing 
    /// </summary>
    public class DrawingPipeline
    {
        private readonly AnalyzingResult _result;

        private readonly RuntimeAssembly _runtime;

        internal readonly DiagramDefinition Context;

        private readonly Queue<DrawedInstance> _toDrawQueue = new Queue<DrawedInstance>();

        private readonly Dictionary<Instance, DrawedInstance> _instanceDrawings = new Dictionary<Instance, DrawedInstance>();


        public DrawingPipeline(RuntimeAssembly runtime, AnalyzingResult result)
        {
            _runtime = runtime;
            _result = result;

            var initialView = new EditView(_result.CreateExecutionView());
            Context = new DiagramDefinition(initialView);
        }

        internal DrawedInstance GetDrawing(Instance instance)
        {
            DrawedInstance result;
            if (!_instanceDrawings.TryGetValue(instance, out result))
            {
                result = new DrawedInstance(instance, this);
                _instanceDrawings[instance] = result;

                _toDrawQueue.Enqueue(result);
            }

            return result;
        }

        public void AddToDrawQueue(Instance instance)
        {
            GetDrawing(instance);            
        }

        public DiagramDefinition GetOutput()
        {
            processPipeline();
            registerInteraction();

            return Context;
        }

        private void processPipeline()
        {
            while (_toDrawQueue.Count > 0)
            {
                var processedInstance = _toDrawQueue.Dequeue();
                generalDrawing(processedInstance);
                concreteDrawing(processedInstance);
            }
        }

        private void generalDrawing(DrawedInstance instance)
        {
            //throw new NotImplementedException();
        }

        private void concreteDrawing(DrawedInstance instance)
        {
            var drawer = _runtime.GetDrawer(instance.WrappedInstance);
            if (drawer != null)
                drawer.Draw(instance);
        }

        /// <summary>
        /// TODO: Type system will be more connected with drawing assembly
        /// </summary>
        /// <param name="result"></param>
        /// <param name="diagram"></param>
        private void registerInteraction()
        {
            Context.OnDragStart += (item) =>
            {
                UserInteraction.DraggedInstance = _result.GetInstance(item.ID);
            };
        }
    }
}
