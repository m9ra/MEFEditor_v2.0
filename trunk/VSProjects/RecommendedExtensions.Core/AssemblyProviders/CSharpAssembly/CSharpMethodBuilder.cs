using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using EnvDTE;
using EnvDTE80;

using MEFEditor.TypeSystem;

using RecommendedExtensions.Core.Languages.CSharp;
using RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly;
using RecommendedExtensions.Core.AssemblyProviders.ProjectAssembly.MethodBuilding;

namespace RecommendedExtensions.Core.AssemblyProviders.CSharpAssembly
{

    /// <summary>
    /// Method builder enahnced for C# - it added support for class member initializers
    /// and indexers.
    /// </summary>
    public class CSharpMethodBuilder : MethodItemBuilder
    {
        #region Builders

        /// <summary>
        /// Build <see cref="MethodItem"/> from given <see cref="CodeClass2"/> element
        /// </summary>
        /// <param name="element">Method definition element</param>
        /// <param name="buildGetter">Determine that getter or setter should be builded</param>
        /// <returns>Built method</returns>
        protected MethodItem BuildFrom(CodeClass2 element)
        {
            var isImplicitCtor = Naming.IsParamLessCtor(MethodInfo.MethodID) || Naming.IsClassCtor(MethodInfo.MethodID);

            if (isImplicitCtor)
            {
                return buildImplicitCtor(element);
            }

            return buildInitializer(element);
        }

        /// <summary>
        /// Build implicit ctor for given class
        /// </summary>
        /// <param name="element">Class which ctor will be created</param>
        /// <returns>Created ctor</returns>
        private MethodItem buildImplicitCtor(CodeClass2 element)
        {
            var initializerName = MethodInfo.IsStatic ? CSharpSyntax.MemberStaticInitializer : CSharpSyntax.MemberInitializer;
            var initializerInfo = DeclaringAssembly.InfoBuilder.Build(element as CodeElement, initializerName);
            var ctorGenerator = new DirectGenerator((c) =>
            {
                //implicit ctor only calls inicializer
                c.DynamicCall(initializerInfo.MethodID, c.CurrentArguments[0]);
            });
            return new MethodItem(ctorGenerator, MethodInfo);
        }

        /// <summary>
        /// Build inicializer for given class
        /// </summary>
        /// <param name="element">Class which initializer will be created</param>
        /// <returns>Created initializer</returns>
        private MethodItem buildInitializer(CodeClass2 element)
        {
            var initializerSource = new StringBuilder();
            initializerSource.AppendLine("{");
            foreach (var child in element.Children)
            {
                var initializable = child as CodeVariable2;
                if (
                    initializable == null ||
                    initializable.InitExpression == null ||
                    initializable.IsShared != MethodInfo.IsStatic
                    )
                    continue;

                initializerSource.Append(initializable.Name);
                initializerSource.Append(" = ");
                initializerSource.Append(initializable.InitExpression);
                initializerSource.AppendLine(";");
            }

            initializerSource.Append("}");
            var sourceCode = initializerSource.ToString();
            var namespaces = DeclaringAssembly.GetNamespaces(element as CodeElement);

            var activation = new ParsingActivation(sourceCode, MethodInfo, new string[0], namespaces);
            RegisterActivation(activation, element as CodeElement);
            var generator = new SourceMethodGenerator(activation, DeclaringAssembly.ParsingProvider);

            return new MethodItem(generator, MethodInfo);
        }

        #endregion

        #region Visitor overrides

        /// <inheritdoc />
        public override void VisitProperty(CodeProperty e)
        {
            //indexers items are handled in same way as properties
            base.VisitProperty(e);
        }

        /// <inheritdoc />
        public override void VisitClass(CodeClass2 e)
        {
            Result(BuildFrom(e));
        }

        #endregion
    }
}
