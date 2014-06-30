using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

using MEFEditor;


using ExtensibleApp.Interfaces;

namespace ExtensibleApp
{
    class App
    {
        [Import]
        internal IPlugin Plugin = null;

        [Import("Logger")]
        internal ILogger Logger=null;

        [CompositionPoint]
        internal static App Compose()
        {
            //collect components
            var catalog1 = new DirectoryCatalog("./Extensions", "*.dll");
            var catalog2 = new TypeCatalog(typeof(ConsoleLogger));

            var aggregateCatalog = new AggregateCatalog();
            aggregateCatalog.Catalogs.Add(catalog1);
            aggregateCatalog.Catalogs.Add(catalog2);

            //compose application
            var app = new App();
            var container = new CompositionContainer(aggregateCatalog);
            container.ComposeParts(app);

            return app;
        }

        internal void Run()
        {
            Logger.Write("Imported plugin: " + Plugin.Name);
        }
    }
}
