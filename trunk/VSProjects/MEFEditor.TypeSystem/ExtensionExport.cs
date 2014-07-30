using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MEFEditor.Drawing;
using MEFEditor.TypeSystem.Runtime;
using MEFEditor.TypeSystem.DrawingServices;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Factory method for exported drawing definitions.
    /// </summary>
    /// <param name="item"><see cref="DiagramItem" /> which drawing is required.</param>
    /// <returns>Created <see cref="ContentDrawing" />.</returns>
    public delegate ContentDrawing ExportedDrawingFactory(DiagramItem item);

    /// <summary>
    /// Factory method for exported assembly providers.
    /// </summary>
    /// <param name="assemblyKey">Key defining assembly</param>
    /// <returns><see cref="AssemblyProvider" /> if can be created from given key, <c>null</c> otherwise.</returns>
    public delegate AssemblyProvider ExportedAssemblyProviderFactory(object assemblyKey);

    /// <summary>
    /// Method used for initialization of every drawing instance definition.
    /// </summary>
    /// <param name="instance">Instance which drawing definition is created.</param>
    /// <param name="info">Component information for drawn instance.</param>
    public delegate void GeneralDrawingDefinitionProvider(DrawedInstance instance,ComponentInfo info);

    /// <summary>
    /// Handler used for logging events
    /// </summary>
    /// <param name="category">Category of logged message</param>
    /// <param name="message">Logged message</param>
    public delegate void OnLogEvent(string category, string message);

    /// <summary>
    /// Provides ability to export extensions into <see cref="MEFEditor" />. It has to be
    /// marked with <see cref="System.ComponentModel.Composition.ExportAttribute"/> with
    /// <see cref="ExtensionExport"/> contract.
    /// </summary>
    public abstract class ExtensionExport
    {
        /// <summary>
        /// Here are stored default log messages of exporter.
        /// </summary>
        private readonly StringBuilder _log = new StringBuilder();

        /// <summary>
        /// Currently available runtime, where type definitions will be loaded into.
        /// </summary>
        private RuntimeAssembly _currentRuntime;

        /// <summary>
        /// <see cref="AssemblyProvider" /> factories that were collected during registering.
        /// </summary>
        private List<ExportedAssemblyProviderFactory> _exportedProviders = new List<ExportedAssemblyProviderFactory>();

        /// <summary>
        /// <see cref="ContentDrawing" /> factories that were collected during registering indexed by associated type.
        /// </summary>
        private Dictionary<string, ExportedDrawingFactory> _exportedDrawers = new Dictionary<string, ExportedDrawingFactory>();

        /// <summary>
        /// <see cref="AssemblyProvider" /> factories that were collected during registering.
        /// </summary>
        /// <value>The exported providers.</value>
        public IEnumerable<ExportedAssemblyProviderFactory> ExportedProviders { get { return _exportedProviders; } }

        /// <summary>
        /// <see cref="ContentDrawing" /> factories that were collected during registering.
        /// </summary>
        /// <value>The exported drawers.</value>
        public IEnumerable<KeyValuePair<string, ExportedDrawingFactory>> ExportedDrawers { get { return _exportedDrawers; } }

        /// <summary>
        /// Gets general drawing definition provider that has been exported.
        /// </summary>
        /// <value>The exported general drawing definition provider.</value>
        public GeneralDrawingDefinitionProvider ExportedGeneralDrawingDefinitionProvider { get; private set; }

        /// <summary>
        /// Event fired whenever new message is logged.
        /// </summary>
        public event OnLogEvent OnLog;

        /// <summary>
        /// When overridden register required extensions for runtime.
        /// </summary>
        protected abstract void Register();

        /// <summary>
        /// Load all registered exports. Type definitions are added into runtime,
        /// factories are collected to <see cref="ExportedProviders" /> and <see cref="ExportedDrawers" />.
        /// </summary>
        /// <param name="runtime">Runtime where type definitions will be loaded into.</param>
        /// <exception cref="System.ArgumentNullException">runtime</exception>
        public void LoadExports(RuntimeAssembly runtime)
        {
            _exportedProviders.Clear();
            _exportedDrawers.Clear();

            if (runtime == null)
                throw new ArgumentNullException("runtime");

            try
            {
                _currentRuntime = runtime;
                Register();
            }
            finally
            {
                _currentRuntime = null;
            }
        }

        /// <summary>
        /// Exports Type as direct definition to <see cref="RuntimeAssembly"/>.
        /// </summary>
        /// <typeparam name="Type">The type of the type.</typeparam>
        protected void ExportAsDirectDefinition<Type>()
        {
            var definition = new DirectTypeDefinition(typeof(Type));
            ExportDefinition(definition);
        }

        /// <summary>
        /// Exports the <see cref="DataTypeDefinition"/> to <see cref="RuntimeAssembly"/>.
        /// </summary>
        /// <typeparam name="TypeDefinition">The type of the data type definion.</typeparam>
        protected void ExportDefinition<TypeDefinition>()
            where TypeDefinition : DataTypeDefinition
        {
            ExportDefinitionWithDrawing<TypeDefinition>(null);
        }


        /// <summary>
        /// Exports <see cref="DataTypeDefinition"/> with drawing registered for same type.
        /// </summary>
        /// <typeparam name="TypeDefinition">The type of the data type definion.</typeparam>
        /// <param name="drawing">The exported drawing.</param>
        protected void ExportDefinitionWithDrawing<TypeDefinition>(ExportedDrawingFactory drawing)
            where TypeDefinition : DataTypeDefinition
        {
            DataTypeDefinition definition;
            try
            {
                definition = Activator.CreateInstance<TypeDefinition>();
            }
            catch (Exception)
            {
                Error("Export of {0} skipped because it cannot be constructed, it must have public parameter less constructor", typeof(TypeDefinition).FullName);
                return;
            }

            ExportDefinition(definition);
            if (drawing != null)
                ExportDrawing(definition.FullName, drawing);
        }

        /// <summary>
        /// Exports the given <see cref="DirectTypeDefinition"/> to <see cref="RuntimeAssembly"/>.
        /// </summary>
        /// <param name="definition">The direct type definition.</param>
        protected void ExportDefinition(DirectTypeDefinition definition)
        {
            Message("Exporting {0} for direct type {1}", definition, definition.DirectType);
            _currentRuntime.AddDirectDefinition(definition);
        }

        /// <summary>
        /// Exports the given <see cref="DataTypeDefinition"/> to <see cref="RuntimeAssembly"/>.
        /// </summary>
        /// <param name="definition">The data type definition.</param>
        protected void ExportDefinition(DataTypeDefinition definition)
        {
            Message("Exporting {0} for data type {1}", definition, definition.FullName);
            _currentRuntime.AddDefinition(definition);
        }

        /// <summary>
        /// Exports the drawing factory for given type.
        /// </summary>
        /// <typeparam name="RegisteredType">The type of the registered type.</typeparam>
        /// <param name="drawing">The exported drawing.</param>
        protected void ExportDrawing<RegisteredType>(ExportedDrawingFactory drawing)
        {
            ExportDrawing(typeof(RegisteredType).FullName, drawing);
        }

        /// <summary>
        /// Exports the general drawing factory that is used for every drawn instance.
        /// </summary>
        /// <param name="drawing">The exported drawing.</param>
        protected void ExportGeneralDrawing(ExportedDrawingFactory drawing)
        {
            ExportDrawing("", drawing);
        }


        /// <summary>
        /// Exports the general drawing definition provider that is called on every <see cref="Analyzing.Instance"/> that will be drawn.
        /// </summary>
        /// <param name="provider">The exported provider.</param>
        protected void ExportGeneralDrawingDefinitionProvider(GeneralDrawingDefinitionProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            ExportedGeneralDrawingDefinitionProvider = provider;
            Message("Exporting general drawing definition provider {0}", provider);
        }

        /// <summary>
        /// Exports the drawing factory for type with given type name.
        /// </summary>
        /// <param name="registeredTypeName">Name of the registered type.</param>
        /// <param name="drawing">The exported drawing.</param>
        protected void ExportDrawing(string registeredTypeName, ExportedDrawingFactory drawing)
        {
            if (registeredTypeName == "")
            {
                Message("Exporting {0} as general drawing", drawing);
            }
            else
            {
                Message("Exporting {0} for type {1}", drawing, registeredTypeName);
            }

            _exportedDrawers[registeredTypeName] = drawing;
        }

        /// <summary>
        /// Exports the assembly factory that provide assemblies for keys of given type.
        /// </summary>
        /// <typeparam name="KeyType">The type of supported assembly key.</typeparam>
        /// <param name="factory">The exported factory.</param>
        protected void ExportAssemblyFactory<KeyType>(Func<KeyType, AssemblyProvider> factory)
        {
            Message("Exporting factory based on type {0}", typeof(KeyType));

            ExportAssemblyFactory((obj) =>
            {
                if (!(obj is KeyType))
                    return null;

                var key = (KeyType)obj;
                return factory(key);
            });
        }

        /// <summary>
        /// Exports the assembly factory that provide assemblies from given assembly keys.
        /// </summary>
        /// <param name="factory">The exported factory.</param>
        protected void ExportAssemblyFactory(ExportedAssemblyProviderFactory factory)
        {
            _exportedProviders.Add(factory);
        }

        #region Logging routines

        /// <summary>
        /// Method used for logging during extension registering.
        /// </summary>
        /// <param name="category">Category that is registered.</param>
        /// <param name="format">Format of logged entry.</param>
        /// <param name="args">Format arguments.</param>
        protected virtual void Log(string category, string format, params object[] args)
        {
            var message = string.Format(format, args);
            _log.AppendLine("[" + category + "]" + message);

            if (OnLog != null)
                OnLog(category, message);
        }

        /// <summary>
        /// Method used for message logging during extension registering.
        /// </summary>
        /// <param name="format">Format of logged message.</param>
        /// <param name="args">Format arguments.</param>
        protected void Message(string format, params object[] args)
        {
            Log("MESSAGE", format, args);
        }

        /// <summary>
        /// Method used for error logging during extension registering.
        /// </summary>
        /// <param name="format">Format of logged error.</param>
        /// <param name="args">Format arguments.</param>
        protected void Error(string format, params object[] args)
        {
            Log("ERROR", format, args);
        }

        #endregion
    }
}
