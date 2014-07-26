using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Runtime.InteropServices;

using EnvDTE;
using EnvDTE80;

using TypeSystem;

using AssemblyProviders.CSharp.LanguageDefinitions;

namespace AssemblyProviders.ProjectAssembly
{
    /// <summary>
    /// Extensions for CodeElement objects.
    /// </summary>
    static class CodeElementExtensions
    {
        /// <summary>
        /// Get fullname of given attribute, without throwing excpetions.
        /// </summary>
        /// <param name="attribute">Attribute which fullname fill be returned.</param>
        /// <returns>Null if excpetion occur, otherwise attributes fullname.</returns>
        public static string SafeFullname(this CodeAttribute attribute)
        {
            return (attribute as CodeElement).SafeFullname();
        }

        /// <summary>
        /// Get fullname of given element, without throwing excpetions.
        /// </summary>
        /// <param name="element">Element which fullname fill be returned.</param>
        /// <returns>Null if excpetion occur, otherwise elements fullname.</returns>
        public static string SafeFullname(this CodeElement element)
        {
            try
            {
                return element.FullName;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get TypeDescriptor defined by given element.       
        /// </summary>
        /// <param name="element">Element which type will be resolved.</param>
        /// <returns>Result type fullname of given element.</returns>
        public static TypeDescriptor ResolveType(this CodeElement element)
        {
            if (element == null) return null;

            try
            {
                CodeTypeRef typeRef;
                switch (element.Kind)
                {
                    case vsCMElement.vsCMElementFunction:
                        typeRef = (element as CodeFunction).Type;
                        break;
                    case vsCMElement.vsCMElementParameter:
                        typeRef = (element as CodeParameter).Type;
                        break;
                    case vsCMElement.vsCMElementVariable:
                        typeRef = (element as CodeVariable).Type;
                        break;
                    case vsCMElement.vsCMElementProperty:
                        typeRef = (element as CodeProperty).Type;
                        break;
                    default:
                        throw new NotSupportedException("This element is not supported for type resolving");
                }
                return resolveTypeRef(typeRef);
            }
            catch (COMException)
            {
                return null;
            }
        }

        /// <summary>
        /// Resolve given type ref
        /// </summary>
        /// <param name="typeRef"></param>
        /// <returns></returns>
        private static TypeDescriptor resolveTypeRef(CodeTypeRef typeRef)
        {
            if (typeRef == null)
                return null;

            switch (typeRef.TypeKind)
            {
                case vsCMTypeRef.vsCMTypeRefArray:
                    var resolved = resolveTypeRef(typeRef.ElementType);
                    if (resolved == null)
                        return null;

                    return TypeDescriptor.ArrayInfo.MakeGeneric(new Dictionary<string, string>() { 
                        {"",resolved.TypeName}, {"",typeRef.Rank.ToString() }
                    });

                case vsCMTypeRef.vsCMTypeRefVoid:
                    return TypeDescriptor.Void;

                default:
                    return descriptorFromFullName(typeRef.AsFullName);

            }
        }

        /// <summary>
        /// Get body of element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static string GetBody(this CodeElement element)
        {
            try
            {
                string text = null;
                if (element is CodeFunction) text = getText(element, vsCMPart.vsCMPartBody);
                if (element is CodeVariable) text = (element as CodeVariable).InitExpression as string;
                if (element is CodeAttributeArgument) text = getAttribArgText(element as CodeAttributeArgument);
                if (element is CodeAttribute) text = getAttribText(element as CodeAttribute);
                return text;
            }
            catch (COMException)
            {
                return null;
            }
        }

        /// <summary>
        /// Determine if property is auto generated.
        /// </summary>
        /// <param name="prop">Property to be tested.</param>
        /// <returns>True if prop is auto generated property.</returns>
        public static bool IsAutoProperty(this CodeProperty prop)
        {
            try
            {
                var pGetter = prop.Getter as CodeFunction;
                var pSetter = prop.Setter as CodeFunction;

                if (pGetter == null || pSetter == null)
                    //auto generated property has to have both setter, getter
                    return false;

                if (pGetter.Language == CSharpSyntax.LanguageID)
                {
                    //C# can determine auto properties faster than by throwing exceptions
                    var start = pGetter.StartPoint.CreateEditPoint();
                    var text = start.GetText(pGetter.EndPoint);

                    return !text.Contains("{");
                }

                if ((pGetter as CodeElement).GetBody() == null)
                    return true;

                return false;

            }
            catch (COMException)
            {
                //we cant access to property -> will be autogenerated.            
                return true;
            }
        }


        public static CodeClass DeclaringClass(this CodeElement element)
        {
            if (element == null)
                return null;

            switch (element.Kind)
            {
                case vsCMElement.vsCMElementClass:
                    return element as CodeClass;
                case vsCMElement.vsCMElementProperty:
                    return DeclaringClass(element as CodeProperty) as CodeClass;
                case vsCMElement.vsCMElementVariable:
                    return DeclaringClass(element as CodeVariable);
                case vsCMElement.vsCMElementFunction:
                    return DeclaringClass(element as CodeFunction);
                case vsCMElement.vsCMElementAttribute:
                    return DeclaringClass(element as CodeAttribute);

                default:
                    return null;
            }
        }

        public static CodeClass DeclaringClass(this CodeAttribute element)
        {
            var parent = element.Parent as CodeElement;
            if (parent == null)
                return null;

            return parent.DeclaringClass();
        }


        public static CodeClass DeclaringClass(this CodeFunction element)
        {
            var parent = element.Parent as CodeElement;
            if (parent == null)
                return null;

            switch (parent.Kind)
            {
                case vsCMElement.vsCMElementClass:
                    return parent as CodeClass;

                case vsCMElement.vsCMElementProperty:
                    return DeclaringClass(parent as CodeProperty) as CodeClass;

                default:
                    return null;
            }
        }

        public static CodeType DeclaringType(this CodeFunction element)
        {
            var declaringClass = element.DeclaringClass();
            if (declaringClass != null)
                return declaringClass as CodeType;

            var parent = element.Parent as CodeElement;
            if (parent == null)
                return null;

            switch (parent.Kind)
            {
                case vsCMElement.vsCMElementClass:
                case vsCMElement.vsCMElementEnum:
                case vsCMElement.vsCMElementInterface:
                    return parent as CodeType;

                default:
                    return null;
            }
        }

        public static bool IsVirtual(this CodeFunction element)
        {
            var fn = element as CodeFunction2;
            if (fn == null)
                return false;

            switch (fn.OverrideKind)
            {
                case vsCMOverrideKind.vsCMOverrideKindVirtual:
                case vsCMOverrideKind.vsCMOverrideKindOverride:
                case vsCMOverrideKind.vsCMOverrideKindAbstract:
                    return true;
                default:
                    return false;
            }
        }

        public static CodeType DeclaringClass(this CodeProperty element)
        {
            var property2 = element as CodeProperty2;
            if (property2 != null)
                return property2.Parent2 as CodeType;

            return element.Parent as CodeType;
        }

        public static CodeClass DeclaringClass(this CodeVariable element)
        {
            return element.Parent as CodeClass;
        }

        /// <summary>
        /// Get context type for given element.
        /// </summary>
        /// <param name="el">Element which context type is resolved.</param>
        /// <returns>Fullname of context type.</returns>
        public static TypeDescriptor GetContextType(this CodeElement el)
        {
            if (el.IsTypeDefinition())
            {
                return descriptorFromFullName(el.FullName);
            }
            else
            {
                return el.ResolveType();
            }
        }

        /// <summary>
        /// Get attributes defined on given element.
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public static CodeElements GetAttributes(this CodeElement el)
        {
            if (el == null) return null;
            switch (el.Kind)
            {
                case vsCMElement.vsCMElementFunction:
                    return (el as CodeFunction).Attributes;
                case vsCMElement.vsCMElementClass:
                    return (el as CodeClass).Attributes;
                case vsCMElement.vsCMElementVariable:
                    return (el as CodeVariable).Attributes;
                case vsCMElement.vsCMElementProperty:
                    return (el as CodeProperty).Attributes;
                case vsCMElement.vsCMElementInterface:
                    return (el as CodeInterface).Attributes;
            }

            return null;
        }

        /// <summary>
        /// Determine if given element is TypeDefinition.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool IsTypeDefinition(this CodeElement element)
        {
            switch (element.Kind)
            {
                case vsCMElement.vsCMElementClass:
                case vsCMElement.vsCMElementStruct:
                case vsCMElement.vsCMElementInterface:
                case vsCMElement.vsCMElementEnum:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determine if element is shared.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool Shared(this CodeElement element)
        {
            bool result;
            if (Bool<CodeClass2>(element, (cls) => cls.IsShared, out result)) return result;

            return false;
        }

        /// <summary>
        /// Return all ancestors of given element.
        /// </summary>
        /// <param name="el"></param>
        /// <returns></returns>
        public static CodeElement[] Ancestors(this CodeElement el)
        {
            CodeElement[] els;

            if (Els<CodeClass>(el, (cls) => getElements(cls.Bases, cls.ImplementedInterfaces), out els)) return els;
            if (Els<CodeInterface>(el, (intf) => getElements(intf.Bases), out els)) return els;

            return null;
        }


        private static CodeElement[] getElements(params CodeElements[] elsParam)
        {
            var res = new List<CodeElement>();

            foreach (var els in elsParam)
                foreach (CodeElement el in els)
                {
                    res.Add(el);
                }

            return res.ToArray();
        }

        private static TypeDescriptor descriptorFromFullName(string fullname)
        {
            return TypeDescriptor.Create(fullname);
        }

        private static string getText(CodeElement el, vsCMPart part)
        {
            if (!el.ProjectItem.IsOpen) el.ProjectItem.Open();

            var editPoint = el.GetStartPoint(part).CreateEditPoint();
            return editPoint.GetText(el.EndPoint).Replace("\r", "");
        }
        private static string getAttribArgText(CodeAttributeArgument atrArg)
        {
            if (atrArg.Name == "") return atrArg.Value;
            else return string.Format("{0} = {1}", atrArg.Name, atrArg.Value);
        }
        private static string getAttribText(CodeAttribute atr)
        {
            var args = new StringBuilder();
            foreach (CodeElement child in atr.Children)
            {
                if (args.Length > 0) args.Append(',');
                args.Append(child.GetBody());
            }

            return string.Format("new {0}({1});", atr.FullName, args);
        }

        private static bool Bool<T>(object o, Func<T, bool> handler, out bool result)
        {
            return Converter<T, bool>(o, handler, out result);
        }

        private static bool Els<T>(object o, Func<T, CodeElement[]> handler, out CodeElement[] result)
        {
            return Converter<T, CodeElement[]>(o, handler, out result);
        }

        private static bool Converter<T, TResult>(object obj, Func<T, TResult> handler, out TResult result)
        {
            if (obj != null && obj is T)
            {
                result = handler((T)obj);
                return true;
            }
            result = default(TResult);
            return false;
        }
    }
}
