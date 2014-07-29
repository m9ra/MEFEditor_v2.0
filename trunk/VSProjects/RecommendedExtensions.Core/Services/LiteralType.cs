using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MEFEditor.Analyzing;
using MEFEditor.TypeSystem;

namespace RecommendedExtensions.Core.Services
{
    /// <summary>
    /// Implementation of type used by C# compiler for parsed type literals.
    /// It has to be present as direct type in Runtime, to compiler could works properly.
    /// </summary>
    public class LiteralType : Type
    {
        /// <summary>
        /// The stored type information.
        /// </summary>
        private readonly InstanceInfo _info;

        /// <summary>
        /// Initializes a new instance of the <see cref="LiteralType" /> class.
        /// </summary>
        /// <param name="info">The type information information.</param>
        /// <exception cref="System.ArgumentNullException">info</exception>
        internal LiteralType(InstanceInfo info)
        {
            if (info == null)
                throw new ArgumentNullException("info");

            _info = info;
        }

        /// <summary>
        /// Gets the <see cref="T:System.Reflection.Assembly" /> in which the type is declared. For generic types, gets the <see cref="T:System.Reflection.Assembly" /> in which the generic type is defined.
        /// </summary>
        /// <value>The assembly.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        public override System.Reflection.Assembly Assembly
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the assembly-qualified name of the <see cref="T:System.Type" />, which includes the name of the assembly from which the <see cref="T:System.Type" /> was loaded.
        /// </summary>
        /// <value>The name of the assembly qualified.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string AssemblyQualifiedName
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the type from which the current <see cref="T:System.Type" /> directly inherits.
        /// </summary>
        /// <value>The type of the base.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Type BaseType
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the fully qualified name of the <see cref="T:System.Type" />, including the namespace of the <see cref="T:System.Type" /> but not the assembly.
        /// </summary>
        /// <value>The full name.</value>
        public override string FullName
        {
            get { return _info.TypeName; }
        }

        /// <summary>
        /// Gets the GUID associated with the <see cref="T:System.Type" />.
        /// </summary>
        /// <value>The unique identifier.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Guid GUID
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// When overridden in a derived class, implements the <see cref="P:System.Type.Attributes" /> property and gets a bitmask indicating the attributes associated with the <see cref="T:System.Type" />.
        /// </summary>
        /// <returns>A <see cref="T:System.Reflection.TypeAttributes" /> object representing the attribute set of the <see cref="T:System.Type" />.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override System.Reflection.TypeAttributes GetAttributeFlagsImpl()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, searches for a constructor whose parameters match the specified argument types and modifiers, using the specified binding constraints and the specified calling convention.
        /// </summary>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null.</param>
        /// <param name="binder">An object that defines a set of properties and enables binding, which can involve selection of an overloaded method, coercion of argument types, and invocation of a member through reflection.-or- A null reference (Nothing in Visual Basic), to use the <see cref="P:System.Type.DefaultBinder" />.</param>
        /// <param name="callConvention">The object that specifies the set of rules to use regarding the order and layout of arguments, how the return value is passed, what registers are used for arguments, and the stack is cleaned up.</param>
        /// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the constructor to get.-or- An empty array of the type <see cref="T:System.Type" /> (that is, Type[] types = new Type[0]) to get a constructor that takes no parameters.</param>
        /// <param name="modifiers">An array of <see cref="T:System.Reflection.ParameterModifier" /> objects representing the attributes associated with the corresponding element in the <paramref name="types" /> array. The default binder does not process this parameter.</param>
        /// <returns>A <see cref="T:System.Reflection.ConstructorInfo" /> object representing the constructor that matches the specified requirements, if found; otherwise, null.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override System.Reflection.ConstructorInfo GetConstructorImpl(System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Reflection.CallingConventions callConvention, Type[] types, System.Reflection.ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, searches for the constructors defined for the current <see cref="T:System.Type" />, using the specified BindingFlags.
        /// </summary>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null.</param>
        /// <returns>An array of <see cref="T:System.Reflection.ConstructorInfo" /> objects representing all constructors defined for the current <see cref="T:System.Type" /> that match the specified binding constraints, including the type initializer if it is defined. Returns an empty array of type <see cref="T:System.Reflection.ConstructorInfo" /> if no constructors are defined for the current <see cref="T:System.Type" />, if none of the defined constructors match the binding constraints, or if the current <see cref="T:System.Type" /> represents a type parameter in the definition of a generic type or generic method.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override System.Reflection.ConstructorInfo[] GetConstructors(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, returns the <see cref="T:System.Type" /> of the object encompassed or referred to by the current array, pointer or reference type.
        /// </summary>
        /// <returns>The <see cref="T:System.Type" /> of the object encompassed or referred to by the current array, pointer, or reference type, or null if the current <see cref="T:System.Type" /> is not an array or a pointer, or is not passed by reference, or represents a generic type or a type parameter in the definition of a generic type or generic method.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Type GetElementType()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, returns the <see cref="T:System.Reflection.EventInfo" /> object representing the specified event, using the specified binding constraints.
        /// </summary>
        /// <param name="name">The string containing the name of an event which is declared or inherited by the current <see cref="T:System.Type" />.</param>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null.</param>
        /// <returns>The object representing the specified event that is declared or inherited by the current <see cref="T:System.Type" />, if found; otherwise, null.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override System.Reflection.EventInfo GetEvent(string name, System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, searches for events that are declared or inherited by the current <see cref="T:System.Type" />, using the specified binding constraints.
        /// </summary>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null.</param>
        /// <returns>An array of <see cref="T:System.Reflection.EventInfo" /> objects representing all events that are declared or inherited by the current <see cref="T:System.Type" /> that match the specified binding constraints.-or- An empty array of type <see cref="T:System.Reflection.EventInfo" />, if the current <see cref="T:System.Type" /> does not have events, or if none of the events match the binding constraints.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override System.Reflection.EventInfo[] GetEvents(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Searches for the specified field, using the specified binding constraints.
        /// </summary>
        /// <param name="name">The string containing the name of the data field to get.</param>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null.</param>
        /// <returns>An object representing the field that matches the specified requirements, if found; otherwise, null.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override System.Reflection.FieldInfo GetField(string name, System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, searches for the fields defined for the current <see cref="T:System.Type" />, using the specified binding constraints.
        /// </summary>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null.</param>
        /// <returns>An array of <see cref="T:System.Reflection.FieldInfo" /> objects representing all fields defined for the current <see cref="T:System.Type" /> that match the specified binding constraints.-or- An empty array of type <see cref="T:System.Reflection.FieldInfo" />, if no fields are defined for the current <see cref="T:System.Type" />, or if none of the defined fields match the binding constraints.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override System.Reflection.FieldInfo[] GetFields(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, searches for the specified interface, specifying whether to do a case-insensitive search for the interface name.
        /// </summary>
        /// <param name="name">The string containing the name of the interface to get. For generic interfaces, this is the mangled name.</param>
        /// <param name="ignoreCase">true to ignore the case of that part of <paramref name="name" /> that specifies the simple interface name (the part that specifies the namespace must be correctly cased).-or- false to perform a case-sensitive search for all parts of <paramref name="name" />.</param>
        /// <returns>An object representing the interface with the specified name, implemented or inherited by the current <see cref="T:System.Type" />, if found; otherwise, null.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Type GetInterface(string name, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, gets all the interfaces implemented or inherited by the current <see cref="T:System.Type" />.
        /// </summary>
        /// <returns>An array of <see cref="T:System.Type" /> objects representing all the interfaces implemented or inherited by the current <see cref="T:System.Type" />.-or- An empty array of type <see cref="T:System.Type" />, if no interfaces are implemented or inherited by the current <see cref="T:System.Type" />.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Type[] GetInterfaces()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, searches for the members defined for the current <see cref="T:System.Type" />, using the specified binding constraints.
        /// </summary>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null.</param>
        /// <returns>An array of <see cref="T:System.Reflection.MemberInfo" /> objects representing all members defined for the current <see cref="T:System.Type" /> that match the specified binding constraints.-or- An empty array of type <see cref="T:System.Reflection.MemberInfo" />, if no members are defined for the current <see cref="T:System.Type" />, or if none of the defined members match the binding constraints.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override System.Reflection.MemberInfo[] GetMembers(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, searches for the specified method whose parameters match the specified argument types and modifiers, using the specified binding constraints and the specified calling convention.
        /// </summary>
        /// <param name="name">The string containing the name of the method to get.</param>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null.</param>
        /// <param name="binder">An object that defines a set of properties and enables binding, which can involve selection of an overloaded method, coercion of argument types, and invocation of a member through reflection.-or- A null reference (Nothing in Visual Basic), to use the <see cref="P:System.Type.DefaultBinder" />.</param>
        /// <param name="callConvention">The object that specifies the set of rules to use regarding the order and layout of arguments, how the return value is passed, what registers are used for arguments, and what process cleans up the stack.</param>
        /// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the method to get.-or- An empty array of the type <see cref="T:System.Type" /> (that is, Type[] types = new Type[0]) to get a method that takes no parameters.-or- null. If <paramref name="types" /> is null, arguments are not matched.</param>
        /// <param name="modifiers">An array of <see cref="T:System.Reflection.ParameterModifier" /> objects representing the attributes associated with the corresponding element in the <paramref name="types" /> array. The default binder does not process this parameter.</param>
        /// <returns>An object representing the method that matches the specified requirements, if found; otherwise, null.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override System.Reflection.MethodInfo GetMethodImpl(string name, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, System.Reflection.CallingConventions callConvention, Type[] types, System.Reflection.ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, searches for the methods defined for the current <see cref="T:System.Type" />, using the specified binding constraints.
        /// </summary>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null.</param>
        /// <returns>An array of <see cref="T:System.Reflection.MethodInfo" /> objects representing all methods defined for the current <see cref="T:System.Type" /> that match the specified binding constraints.-or- An empty array of type <see cref="T:System.Reflection.MethodInfo" />, if no methods are defined for the current <see cref="T:System.Type" />, or if none of the defined methods match the binding constraints.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override System.Reflection.MethodInfo[] GetMethods(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, searches for the specified nested type, using the specified binding constraints.
        /// </summary>
        /// <param name="name">The string containing the name of the nested type to get.</param>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null.</param>
        /// <returns>An object representing the nested type that matches the specified requirements, if found; otherwise, null.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Type GetNestedType(string name, System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, searches for the types nested in the current <see cref="T:System.Type" />, using the specified binding constraints.
        /// </summary>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null.</param>
        /// <returns>An array of <see cref="T:System.Type" /> objects representing all the types nested in the current <see cref="T:System.Type" /> that match the specified binding constraints (the search is not recursive), or an empty array of type <see cref="T:System.Type" />, if no nested types are found that match the binding constraints.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Type[] GetNestedTypes(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, searches for the properties of the current <see cref="T:System.Type" />, using the specified binding constraints.
        /// </summary>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null.</param>
        /// <returns>An array of <see cref="T:System.Reflection.PropertyInfo" /> objects representing all properties of the current <see cref="T:System.Type" /> that match the specified binding constraints.-or- An empty array of type <see cref="T:System.Reflection.PropertyInfo" />, if the current <see cref="T:System.Type" /> does not have properties, or if none of the properties match the binding constraints.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override System.Reflection.PropertyInfo[] GetProperties(System.Reflection.BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, searches for the specified property whose parameters match the specified argument types and modifiers, using the specified binding constraints.
        /// </summary>
        /// <param name="name">The string containing the name of the property to get.</param>
        /// <param name="bindingAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted.-or- Zero, to return null.</param>
        /// <param name="binder">An object that defines a set of properties and enables binding, which can involve selection of an overloaded member, coercion of argument types, and invocation of a member through reflection.-or- A null reference (Nothing in Visual Basic), to use the <see cref="P:System.Type.DefaultBinder" />.</param>
        /// <param name="returnType">The return type of the property.</param>
        /// <param name="types">An array of <see cref="T:System.Type" /> objects representing the number, order, and type of the parameters for the indexed property to get.-or- An empty array of the type <see cref="T:System.Type" /> (that is, Type[] types = new Type[0]) to get a property that is not indexed.</param>
        /// <param name="modifiers">An array of <see cref="T:System.Reflection.ParameterModifier" /> objects representing the attributes associated with the corresponding element in the <paramref name="types" /> array. The default binder does not process this parameter.</param>
        /// <returns>An object representing the property that matches the specified requirements, if found; otherwise, null.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override System.Reflection.PropertyInfo GetPropertyImpl(string name, System.Reflection.BindingFlags bindingAttr, System.Reflection.Binder binder, Type returnType, Type[] types, System.Reflection.ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, implements the <see cref="P:System.Type.HasElementType" /> property and determines whether the current <see cref="T:System.Type" /> encompasses or refers to another type; that is, whether the current <see cref="T:System.Type" /> is an array, a pointer, or is passed by reference.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Type" /> is an array, a pointer, or is passed by reference; otherwise, false.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override bool HasElementTypeImpl()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, invokes the specified member, using the specified binding constraints and matching the specified argument list, modifiers and culture.
        /// </summary>
        /// <param name="name">The string containing the name of the constructor, method, property, or field member to invoke.-or- An empty string ("") to invoke the default member. -or-For IDispatch members, a string representing the DispID, for example "[DispID=3]".</param>
        /// <param name="invokeAttr">A bitmask comprised of one or more <see cref="T:System.Reflection.BindingFlags" /> that specify how the search is conducted. The access can be one of the BindingFlags such as Public, NonPublic, Private, InvokeMethod, GetField, and so on. The type of lookup need not be specified. If the type of lookup is omitted, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static are used.</param>
        /// <param name="binder">An object that defines a set of properties and enables binding, which can involve selection of an overloaded method, coercion of argument types, and invocation of a member through reflection.-or- A null reference (Nothing in Visual Basic), to use the <see cref="P:System.Type.DefaultBinder" />. Note that explicitly defining a <see cref="T:System.Reflection.Binder" /> object may be required for successfully invoking method overloads with variable arguments.</param>
        /// <param name="target">The object on which to invoke the specified member.</param>
        /// <param name="args">An array containing the arguments to pass to the member to invoke.</param>
        /// <param name="modifiers">An array of <see cref="T:System.Reflection.ParameterModifier" /> objects representing the attributes associated with the corresponding element in the <paramref name="args" /> array. A parameter's associated attributes are stored in the member's signature. The default binder processes this parameter only when calling a COM component.</param>
        /// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> object representing the globalization locale to use, which may be necessary for locale-specific conversions, such as converting a numeric String to a Double.-or- A null reference (Nothing in Visual Basic) to use the current thread's <see cref="T:System.Globalization.CultureInfo" />.</param>
        /// <param name="namedParameters">An array containing the names of the parameters to which the values in the <paramref name="args" /> array are passed.</param>
        /// <returns>An object representing the return value of the invoked member.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override object InvokeMember(string name, System.Reflection.BindingFlags invokeAttr, System.Reflection.Binder binder, object target, object[] args, System.Reflection.ParameterModifier[] modifiers, System.Globalization.CultureInfo culture, string[] namedParameters)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, implements the <see cref="P:System.Type.IsArray" /> property and determines whether the <see cref="T:System.Type" /> is an array.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Type" /> is an array; otherwise, false.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override bool IsArrayImpl()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, implements the <see cref="P:System.Type.IsByRef" /> property and determines whether the <see cref="T:System.Type" /> is passed by reference.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Type" /> is passed by reference; otherwise, false.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override bool IsByRefImpl()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, implements the <see cref="P:System.Type.IsCOMObject" /> property and determines whether the <see cref="T:System.Type" /> is a COM object.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Type" /> is a COM object; otherwise, false.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override bool IsCOMObjectImpl()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, implements the <see cref="P:System.Type.IsPointer" /> property and determines whether the <see cref="T:System.Type" /> is a pointer.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Type" /> is a pointer; otherwise, false.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override bool IsPointerImpl()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, implements the <see cref="P:System.Type.IsPrimitive" /> property and determines whether the <see cref="T:System.Type" /> is one of the primitive types.
        /// </summary>
        /// <returns>true if the <see cref="T:System.Type" /> is one of the primitive types; otherwise, false.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        protected override bool IsPrimitiveImpl()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the module (the DLL) in which the current <see cref="T:System.Type" /> is defined.
        /// </summary>
        /// <value>The module.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        public override System.Reflection.Module Module
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Gets the namespace of the <see cref="T:System.Type" />.
        /// </summary>
        /// <value>The namespace.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        public override string Namespace
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Indicates the type provided by the common language runtime that represents this type.
        /// </summary>
        /// <value>The type of the underlying system.</value>
        /// <exception cref="System.NotImplementedException"></exception>
        public override Type UnderlyingSystemType
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// When overridden in a derived class, returns an array of custom attributes applied to this member and identified by <see cref="T:System.Type" />.
        /// </summary>
        /// <param name="attributeType">The type of attribute to search for. Only attributes that are assignable to this type are returned.</param>
        /// <param name="inherit">true to search this member's inheritance chain to find the attributes; otherwise, false. This parameter is ignored for properties and events; see Remarks.</param>
        /// <returns>An array of custom attributes applied to this member, or an array with zero elements if no attributes assignable to <paramref name="attributeType" /> have been applied.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, returns an array of all custom attributes applied to this member.
        /// </summary>
        /// <param name="inherit">true to search this member's inheritance chain to find the attributes; otherwise, false. This parameter is ignored for properties and events; see Remarks.</param>
        /// <returns>An array that contains all the custom attributes applied to this member, or an array with zero elements if no attributes are defined.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// When overridden in a derived class, indicates whether one or more attributes of the specified type or of its derived types is applied to this member.
        /// </summary>
        /// <param name="attributeType">The type of custom attribute to search for. The search includes derived types.</param>
        /// <param name="inherit">true to search this member's inheritance chain to find the attributes; otherwise, false. This parameter is ignored for properties and events; see Remarks.</param>
        /// <returns>true if one or more instances of <paramref name="attributeType" /> or any of its derived types is applied to this member; otherwise, false.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }


        /// <summary>
        /// Gets the name of the current member.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get
            {
                var path = new PathInfo(_info.TypeName);
                return path.Name;
            }
        }
    }
}
