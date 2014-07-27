using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MEFEditor.Drawing;
using MEFEditor.TypeSystem.Runtime;

namespace MEFEditor.TypeSystem
{
    /// <summary>
    /// Factory method for exported drawing definitions
    /// </summary>
    /// <param name="item"><see cref="DiagramItem"/> which drawing is required</param>
    /// <returns>Created <see cref="ContentDrawing"/></returns>
    public delegate ContentDrawing ExportedDrawingFactory(DiagramItem item);

    /// <summary>
    /// Factory method for exported assembly providers
    /// </summary>
    /// <param name="assemblyKey">Key defining assembly</param>
    /// <returns><see cref="AssemblyProvider"/> if can be created from given key, <c>null</c> otherwise.</returns>
    public delegate AssemblyProvider ExportedAssemblyProviderFactory(object assemblyKey);

    /// <summary>
    /// Handler used for logging events
    /// </summary>
    /// <param name="category">Category of logged message</param>
    /// <param name="message">Logged message</param>
    public delegate void OnLogEvent(string category, string message);

    /// <summary>
    /// Provides ability to export extension into <see cref="RuntimeAssembly"/>
    /// </summary>
    public abstract class ExtensionExport
    {
        /// <summary>
        /// Here are stored default log messages of exporter
        /// </summary>
        private readonly StringBuilder _log = new StringBuilder();

        /// <summary>
        /// Currently available runtime, where type definitions will be loaded into
        /// </summary>
        private RuntimeAssembly _currentRuntime;

        /// <summary>
        /// <see cref="AssemblyProvider"/> factories that were collected during registering
        /// </summary>
        private List<ExportedAssemblyProviderFactory> _exportedProviders = new List<ExportedAssemblyProviderFactory>();

        /// <summary>
        /// <see cref="ContentDrawing"/> factories that were collected during registering
        /// </summary>
        private Dictionary<string, ExportedDrawingFactory> _exportedDrawers = new Dictionary<string, ExportedDrawingFactory>();

        /// <summary>
        /// <see cref="AssemblyProvider"/> factories that were collected during registering
        /// </summary>
        public IEnumerable<ExportedAssemblyProviderFactory> ExportedProviders { get { return _exportedProviders; } }

        /// <summary>
        /// <see cref="ContentDrawing"/> factories that were collected during registering
        /// </summary>
        public IEnumerable<KeyValuePair<string, ExportedDrawingFactory>> ExportedDrawers { get { return _exportedDrawers; } }

        /// <summary>
        /// Event fired whenever new message is logged
        /// </summary>
        public event OnLogEvent OnLog;

        /// <summary>
        /// When overriden register required extensions for runtime
        /// </summary>
        protected abstract void Register();

        /// <summary>
        /// Load all registered exports. Type definitions are added into runtime,
        /// factories are collected to <see cref="ExportedProviders"/> and <see cref="ExportedDrawers"/>
        /// </summary>
        /// <param name="runtime">Runtime where type definitions will be loaded into.</param>
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

        protected void ExportAsDirectDefinition<Type>()
        {
            var definition = new DirectTypeDefinition(typeof(Type));
            ExportDefinition(definition);
        }

        protected void ExportDefinition<TypeDefiniton>()
            where TypeDefiniton : DataTypeDefinition
        {
            ExportDefinitionWithDrawing<TypeDefiniton>(null);
        }


        protected void ExportDefinitionWithDrawing<TypeDefiniton>(ExportedDrawingFactory drawing)
            where TypeDefiniton : DataTypeDefinition
        {
            DataTypeDefinition definition;
            try
            {
                definition = Activator.CreateInstance<TypeDefiniton>();
            }
            catch (Exception)
            {
                Error("Exported {0} skipped because it cannot be constructed, it must have public parameter less constructor", typeof(TypeDefiniton).FullName);
                return;
            }

            ExportDefinition(definition);
            if (drawing != null)
                ExportDrawing(definition.FullName, drawing);
        }

        protected void ExportDefinition(DirectTypeDefinition definition)
        {
            Message("Exporting {0} for direct type {1}", definition, definition.DirectType);
            _currentRuntime.AddDirectDefinition(definition);
        }

        protected void ExportDefinition(DataTypeDefinition definition)
        {
            Message("Exporting {0} for data type {1}", definition, definition.FullName);
            _currentRuntime.AddDefinition(definition);
        }

        protected void ExportDrawing<RegisteredType>(ExportedDrawingFactory drawing)
        {
            ExportDrawing(typeof(RegisteredType).FullName, drawing);
        }

        protected void ExportGeneralDrawing(ExportedDrawingFactory drawing)
        {
            ExportDrawing("", drawing);
        }

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

        protected void ExportAssemblyFactory(ExportedAssemblyProviderFactory factory)
        {
            _exportedProviders.Add(factory);
        }

        #region Logging routines

        /// <summary>
        /// Method used for logging during extension registering
        /// </summary>
        /// <param name="category">Category that is registered</param>
        /// <param name="format">Format of logged entry</param>
        /// <param name="args">Format arguments</param>
        protected virtual void Log(string category, string format, params object[] args)
        {
            var message = string.Format(format, args);
            _log.AppendLine("[" + category + "]" + message);

            if (OnLog != null)
                OnLog(category, message);
        }

        /// <summary>
        /// Method used for message logging during extension registering
        /// </summary>
        /// <param name="format">Format of logged message</param>
        /// <param name="args">Format arguments</param>
        protected void Message(string format, params object[] args)
        {
            Log("MESSAGE", format, args);
        }

        /// <summary>
        /// Method used for error logging during extension registering
        /// </summary>
        /// <param name="format">Format of logged error</param>
        /// <param name="args">Format arguments</param>
        protected void Error(string format, params object[] args)
        {
            Log("ERROR", format, args);
        }

        #endregion
    }
}
