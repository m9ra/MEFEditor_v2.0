using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Diagnostics;
using System.ComponentModel.Composition;

using Analyzing;
using Analyzing.Editing;

using TypeSystem;
using TypeSystem.Runtime;

namespace UserExtensions
{
    [Export(typeof(RuntimeTypeDefinition))]
    public class DiagnosticDefinition : DataTypeDefinition
    {
        //datová položka pro časovač
        protected Field<Stopwatch> Watch;

        //datová položka pro uložené instance
        protected Field<List<Instance>> Instances;

        //datová položka pro zprávu
        protected Field<string> Message;

        public DiagnosticDefinition()
        {
            //nastavíme jméno reprezentovaného typu
            FullName = "MEFEditor.Diagnostic";

            //a také nastavíme typ od kterého dědíme
            //poznamenejme, že object je defaultní předek
            //zde je však uveden pro ukázku použití API
            ForcedSubTypes = new[] { typeof(object) };

            //nakonec přidáme editaci na vytvoření typu
            AddCreationEdit("Add Diagnostic");
        }

        public void _method_ctor()
        {
            //inicializujeme datové položky instance
            Message.Value = "DefaultMessage";
            Watch.Value = new Stopwatch();
            Instances.Value = new List<Instance>();
        }

        public void _set_Message(string message)
        {
            //nastavíme hodnotu pro zprávu
            Message.Value = message;
        }

        public string _get_Message()
        {
            //vrátíme hodnotu pro zprávu
            return Message.Value;
        }

        public void _method_Start()
        {
            //spustíme měření času
            Watch.Value.Start();
        }

        public void _method_Stop()
        {
            //zastavíme měření času
            Watch.Value.Stop();
        }

        [ReturnType(typeof(int))]
        public Instance _method_Accept(params Instance[] accepted)
        {
            //nastavíme akceptování instancí za poslední argument
            AcceptAsLastArgument(componentAccepter);

            //všechny argumenty přidáme do seznamu
            for (var i = 0; i < accepted.Length; ++i)
            {
                //zpracovávaný argument
                var acceptedInstance = accepted[i];
                //označíme jako logické dítě získané z parametrického volání metody
                ReportParamChildAdd(i, acceptedInstance, "Accepted child", true);

                //následně argument uložíme do seznamu
                Instances.Value.Add(acceptedInstance);
            }

            //získáme celkový počet uložených instancí
            int nativeCount = Instances.Value.Count;
            //vytvoříme přímou instanci pro zadaný počet
            Instance wrappedCount = Context.Machine.CreateDirectInstance(nativeCount);

            //vytvořenou instanci vrátíme jako výsledek            
            return wrappedCount;
        }


        protected override void draw(InstanceDrawer drawer)
        {
            //přidáme definici slotu, ve kterém se budou 
            //zobrazovat uložené instance
            var instanceSlot = drawer.AddSlot();

            //vytvořený slot naplníme instancemi
            foreach (var instance in Instances.Value)
            {
                //pro každou instanci musíme získat reprezentaci
                //jejího zobrazení
                var instanceDrawing = drawer.GetInstanceDrawing(instance);
                //do slotu patří pouze reference na vytvořené zobrazení
                instanceSlot.Add(instanceDrawing.Reference);
            }

            //předáme informace které vypíšeme uživateli
            drawer.PublishField("message", Message);
            drawer.SetProperty("time", Watch.Value.ElapsedMilliseconds + "ms");

            //vynutíme zobrazení této instance
            //ve schématu kompozice
            drawer.ForceShow();
        }

        private object componentAccepter(ExecutionView view)
        {
            //instanci, kterou uživatel přesunul myší, získáme následovně
            var toAccept = UserInteraction.DraggedInstance;

            //zjistíme, zda se jedná o komponentu
            var componentInfo = Services.GetComponentInfo(toAccept.Info);
            //akceptovat chceme pouze komponenty
            if (componentInfo == null)
            {
                //pokud se o komponentu nejedná, sdělíme uživateli
                //proč nelze instanci akceptovat a editaci zrušíme
                view.Abort("Can only accept components");
                return null;
            }

            //pokud akceptujeme komponentu, 
            //získáme proměnnou, ve které je dostupná
            //a vrátíme ji jako hodnotu k akceptování
            return Edits.GetVariableFor(toAccept, view); ;
        }
    }
}
