using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ExtensionPoints;
using Example_extensions_referenced;

using MEFEditor;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;


namespace Main
{





    class Program
    {
        [Import]
        ILayout compositionResult=null;

        [ImportMany]
        ILogger[] loggers=null;
              
        [CompositionPoint]
        void Compose()
        {
            var wrongLayout = new WrongLayout();
            var consoleLogger = new ConsoleLogger();
            var typeCatalog = new TypeCatalog(typeof(SimpleLayout));
            var aggregateCatalog = new AggregateCatalog();
            var directoryCatalog = new DirectoryCatalog("Extensions");
            aggregateCatalog.Catalogs.Add(directoryCatalog);
            aggregateCatalog.Catalogs.Add(typeCatalog);
            var compositionContainer = new CompositionContainer(aggregateCatalog);
            compositionContainer.ComposeParts(this);
        }
















        static void Main(string[] args)
        {
            var prg = new Program();
            prg.Compose();

            string outputHTML =prg.compositionResult==null ?"Composition wasn't completed": prg.compositionResult.GetPageHTML;

            if(prg.loggers!=null)
            foreach(var logger in prg.loggers)
                logger.Log("Output HTML: "+outputHTML);

            var server = new HTTPServer(outputHTML);
        }
    }
}
