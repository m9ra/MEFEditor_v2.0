using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;
using MEFEditor.TypeSystem.Runtime;



namespace UserExtensions
{
    public class SimpleAssemblyProvider : AssemblyProvider
    {
        //jméno reprezentované assembly
        private readonly string _name;

        //cesta k souboru který "definuje" assembly
        private readonly string _fullPath;

        //typ který je definován reprezentovanou assembly
        private readonly TypeDescriptor _declaringType;

        //zde jsou uchovány všechny metody, které assembly definuje
        private readonly HashedMethodContainer _methods;

        public SimpleAssemblyProvider(string testFileFullPath)
        {
            //uchováme cestu k "definujícímu" souboru
            _fullPath = testFileFullPath;

            //jméno assembly odvodíme od názvu souboru
            _name = Path.GetFileName(_fullPath);

            //připravíme kontejner kam vložíme definovanou metodu
            _methods = new HashedMethodContainer();

            //vytvoření metody začneme přípravou typu, kde je definovaná
            _declaringType = TypeDescriptor.Create("MEFEditor.ProviderTest");
            //určíme jméno metody
            var methodName = "GetDefiningAssemblyName";
            //návratový typ metody
            var returnType = TypeDescriptor.Create<string>();

            //z definovaných údajů můžeme vytvořit popis
            //metody, která nebude mít žádné parametry a bude statická
            var methodInfo = new TypeMethodInfo(
                _declaringType, methodName, returnType,
                ParameterTypeInfo.NoParams,
                isStatic: true,
                methodTypeArguments: TypeDescriptor.NoDescriptors);

            //k dokončení definice metody stačí vytvořit
            //generátor jejích analyzačních instrukcí
            var methodGenerator = new DirectedGenerator(emitDirector);

            //definovanou metodu vytvoříme
            var method = new MethodItem(methodGenerator, methodInfo);
            //aby byla metoda dohledatelná, musíme ji ještě zaregistrovat
            _methods.AddItem(method);
        }

        private void emitDirector(EmitterBase emitter)
        {
            //emitujeme instrukci pro uložení jména
            //assembly do proměnné
            emitter.AssignLiteral("result", _name);

            //instrukce pro vrácení uložené hodnoty
            emitter.Return("result");
        }

        protected override string getAssemblyFullPath()
        {
            //vrátíme cestu k definujícímu souboru
            return _fullPath;
        }

        protected override string getAssemblyName()
        {
            //vrátíme jméno reprezentované assembly
            return _name;
        }

        public override SearchIterator CreateRootIterator()
        {
            //vytvoříme iterátor, který dokáže procházet
            //definované metody
            return new HashedIterator(_methods);
        }

        public override GeneratorBase GetMethodGenerator(MethodID method)
        {
            //zkusíme nalézt metodu dle zadaného ID
            return _methods.AccordingId(method);
        }

        public override InheritanceChain GetInheritanceChain(PathInfo typePath)
        {
            //informace o dědičnosti poskytujeme pouze
            //pro náš definovaný typ
            if (typePath.Signature == _declaringType.TypeName)
            {
                //definovaný typ je potomkem typu object
                InheritanceChain baseType = TypeServices.GetChain(TypeDescriptor.ObjectInfo);
                return TypeServices.CreateChain(_declaringType, new[] { baseType });
            }

            //dotaz se týkal typu, který nedefinujeme
            return null;
        }

        protected override void loadComponents()
        {
            //nedefinujeme žádné komponenty
        }

        public override MethodID GetGenericImplementation(MethodID methodID, PathInfo methodSearchPath, PathInfo implementingTypePath, out PathInfo alternativeImplementer)
        {
            //nedefinujeme žádný generický interface
            alternativeImplementer = null;
            return null;
        }

        public override GeneratorBase GetGenericMethodGenerator(MethodID method, PathInfo searchPath)
        {
            //nedefinujeme žádné generické metody
            return null;
        }

        public override MethodID GetImplementation(MethodID method, TypeDescriptor dynamicInfo, out TypeDescriptor alternativeImplementer)
        {
            //nedefinujeme žádný interface
            alternativeImplementer = null;
            return null;
        }
    }
}
