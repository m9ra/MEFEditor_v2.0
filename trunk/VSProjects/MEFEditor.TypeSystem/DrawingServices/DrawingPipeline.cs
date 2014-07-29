using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Drawing;
using MEFEditor.Analyzing;
using MEFEditor.Analyzing.Editing;

using MEFEditor.TypeSystem.Runtime;

namespace MEFEditor.TypeSystem.DrawingServices
{
    /// <summary>
    /// Drawer used by pipeline for providing general drawing support
    /// </summary>
    /// <param name="instance">Instance which general drawing will be retrieved</param>
    public delegate void GeneralDrawer(DrawedInstance instance);

    /// <summary>
    /// Pipeline that is processing.
    /// </summary>
    public class DrawingPipeline
    {
        /// <summary>
        /// The result of analysis which instances will be drawn
        /// </summary>
        private readonly AnalyzingResult _result;

        /// <summary>
        /// The general drawer used for every instance to drawn
        /// </summary>
        private readonly GeneralDrawer _drawer;

        /// <summary>
        /// The queue of instances waiting for drawing
        /// </summary>
        private readonly Queue<DrawedInstance> _toDrawQueue = new Queue<DrawedInstance>();

        /// <summary>
        /// The already created drawing of instances
        /// </summary>
        private readonly Dictionary<Instance, DrawedInstance> _instanceDrawings = new Dictionary<Instance, DrawedInstance>();

        /// <summary>
        /// The corresponding <see cref="RuntimeAssembly"/>
        /// </summary>
        internal readonly RuntimeAssembly Runtime;

        /// <summary>
        /// Diagram definition in which context instances will be drawn.
        /// </summary>
        internal readonly DiagramDefinition Context;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingPipeline"/> class.
        /// </summary>
        /// <param name="drawer">The general drawer.</param>
        /// <param name="runtime">Corresponding <see cref="RuntimeAssembly"/>.</param>
        /// <param name="result">The analysis result which instances will be drawn.</param>
        public DrawingPipeline(GeneralDrawer drawer, RuntimeAssembly runtime, AnalyzingResult result)
        {
            Runtime = runtime;
            _result = result;
            _drawer = drawer;

            var initialView = new EditView(_result.CreateExecutionView());
            Context = new DiagramDefinition(initialView);

            foreach (var edit in runtime.GlobalEdits)
            {
                var drawingEdit = CreateEditDefinition(edit);
                Context.AddEdit(drawingEdit);
            }
        }

        /// <summary>
        /// Get drawing and force this drawing to be present in the diagram.
        /// </summary>
        /// <param name="instance">The drawn instance.</param>
        /// <returns>Instance drawing.</returns>
        public DrawedInstance GetDrawing(Instance instance)
        {
            var drawing = getDrawing(instance);

            //force drawing to be in the diagram
            drawing.CommitDrawing();
            return drawing;
        }

        /// <summary>
        /// Adds instance to drawing queue.
        /// </summary>
        /// <param name="instance">The instance to be added.</param>
        public void AddToDrawQueue(Instance instance)
        {
            getDrawing(instance);
        }

        /// <summary>
        /// Forces the display given instance in diagram.
        /// </summary>
        /// <param name="instance">The instance.</param>
        public void ForceDisplay(Instance instance)
        {
            getDrawing(instance).CommitDrawing();
        }

        /// <summary>
        /// Gets the drawing output.
        /// </summary>
        /// <returns>Output of drawing.</returns>
        public DiagramDefinition GetOutput()
        {
            processPipeline();
            registerInteraction();

            return Context;
        }

        /// <summary>
        /// Processes current pipeline.
        /// </summary>
        private void processPipeline()
        {
            while (_toDrawQueue.Count > 0)
            {
                var processedInstance = _toDrawQueue.Dequeue();
                generalDrawing(processedInstance);
                concreteDrawing(processedInstance);
            }
        }

        /// <summary>
        /// Run general drawing of instance.
        /// </summary>
        /// <param name="instance">The instance.</param>
        private void generalDrawing(DrawedInstance instance)
        {
            _drawer(instance);
        }

        /// <summary>
        /// Run concrete drawing of instance.
        /// </summary>
        /// <param name="instance">The drawn instance.</param>
        private void concreteDrawing(DrawedInstance instance)
        {
            var drawer = Runtime.GetDrawer(instance.WrappedInstance);
            if (drawer != null)
                drawer.Draw(instance);
        }

        /// <summary>
        /// Gets instance drawing.
        /// </summary>
        /// <param name="instance">The instance which drawing is requested.</param>
        /// <returns>Instance drawing.</returns>
        private DrawedInstance getDrawing(Instance instance)
        {
            DrawedInstance result;
            if (!_instanceDrawings.TryGetValue(instance, out result))
            {
                var runtimeTypeDefinition = Runtime.GetTypeDefinition(instance);

                result = new DrawedInstance(runtimeTypeDefinition, instance, this);
                _instanceDrawings[instance] = result;

                _toDrawQueue.Enqueue(result);
            }

            return result;
        }

        /// <summary>
        /// Register interaction with user.
        /// </summary>
        private void registerInteraction()
        {
            Context.OnDragStart += (item) =>
            {
                UserInteraction.DraggedInstance = _result.GetInstance(item.ID);
            };
        }

        /// <summary>
        /// Creates the edit definition.
        /// </summary>
        /// <param name="edit">The edit.</param>
        /// <returns>Definition of the edit.</returns>
        internal EditDefinition CreateEditDefinition(Edit edit)
        {
            return new EditDefinition(edit.Name, (view) => runEdit(edit, view as EditView), (v) => true);
        }


        /// <summary>
        /// Runs the edit on given view.
        /// </summary>
        /// <param name="edit">The edit.</param>
        /// <param name="view">The view.</param>
        /// <returns>Edited view.</returns>
        private EditViewBase runEdit(Edit edit, EditView view)
        {
            try
            {
                return view.Apply(edit.Transformation);
            }
            catch (Exception ex)
            {
                return view.Abort(ex.Message);
            }
        }
    }
}
