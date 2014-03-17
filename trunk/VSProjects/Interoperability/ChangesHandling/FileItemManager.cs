﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Runtime.InteropServices;

using EnvDTE;
using EnvDTE80;

namespace Interoperability
{
    /// <summary>
    /// Provide element events in sourcecode - wrapper over LineChanged event
    /// </summary>
    class FileItemManager
    {
        Document _doc;
        int _docLength;

        ElementNode _root;

        /// <summary>
        /// All namespaces available via usings
        /// </summary>
        HashSet<string> _namespaces = new HashSet<string>();

        /// <summary>
        /// Storage for namespaces explored when checking elements
        /// </summary>
        HashSet<string> _checkedNamespaces = new HashSet<string>();

        HashSet<ElementChange> _queue = new HashSet<ElementChange>();
        HashSet<ElementChange> _removed = new HashSet<ElementChange>();

        internal readonly VisualStudioServices VS;

        internal FileItemManager(VisualStudioServices vs, FileCodeModel file)
        {
            VS = vs;
            _doc = file.Parent.Document;

            /*   var fullpath = _doc.Path + _doc.Name;
               var str = File.ReadAllText(fullpath).Replace("\r", "");
               _docLength =str.Length; //Document interface doesnt support EndPoint used for length determining by TextDocument*/


            /*       if (_doc == null) throw new NullReferenceException("_doc cannot be null");

                   var textDoc = (TextDocument)_doc.Object();*/
            var textDoc = file.Parent.Object as TextDocument;
            //    _docLength = textDoc.EndPoint.AbsoluteCharOffset;


            _root = new ElementNode(file.CodeElements, this);
            FlushChanges();
        }

        /// <summary>
        /// handle given changes - make some logic on these changes
        /// </summary>
        /// <param name="changes"></param>
        private void registerChanges(List<ElementChange> changes)
        {
            foreach (var change in changes)
            {
                if (change.Kind == ChangeKind.removed)
                {
                    //element cannot be added before removed, because of add is registered at the end in _root.CheckElements
                    //!!!!BUT ELEMENT CAN BE REMOVED AND THEN ADDED!!!! ->be carefull on order for firing events
                    //element also cannot be changed and then added, without removing before added
                    _queue.Remove(change); //removing change equals to all changes
                    _removed.Add(change);
                }
                else _queue.Add(change);
            }

        }

        /// <summary>
        /// modify wrapped element tree according to line change
        /// </summary>
        /// <param name="change"></param>
        internal void LineChanged(LineChange change)
        {
            var shortening = _docLength - change.DocumentLength;
            _docLength = change.DocumentLength;

            change.Shortening = shortening;
            registerChanges(_root.ApplyEdit(change));
        }

        /// <summary>
        /// Check for mismatches in LineChanged handling and fire handlers for all registered changes
        /// </summary>
        internal void FlushChanges()
        {
            //exception can occur until something is changed - dont need register special handlers
            VS.CodeModelExceptions(() =>
            {
                //check for mistakes in LineChanged handling
                registerChanges(_root.CheckChildren());
                bool nsChange;
                if (_namespaces.Count == _checkedNamespaces.Count)
                {
                    _namespaces.IntersectWith(_checkedNamespaces);
                    if (_namespaces.Count == _checkedNamespaces.Count) nsChange = false;
                    else nsChange = true;
                }
                else nsChange = true;

                if (nsChange)
                {
                    _namespaces.Clear(); //refresh available namespaces
                    _namespaces.UnionWith(_checkedNamespaces);
                    _checkedNamespaces.Clear();

                    //dont need report change when solution is loading ->all elements are newly added
                    registerChanges(_root.NamespaceChange());
                }
            }, "flushinng changes by FileItemManager");

            //exception during firing handlers
            VS.ExecutingExceptions(() => fireHandlers(), "firing handlers for file");
        }

        private void fireHandlers()
        {
            //removing has to be before other changes -> remove can be caused via LineChanged, but should be repaired in CheckChildren phase
            foreach (var rem in _removed)
                onElementRemoved(rem.Node);

            foreach (var chg in _queue)
                switch (chg.Kind)
                {
                    case ChangeKind.added:
                        onElementAdd(chg.Node);
                        break;
                    case ChangeKind.changed:
                        onElementChanged(chg.Node);
                        break;
                }

            _removed.Clear();
            _queue.Clear();
        }

        private void logSpan(string description, ElementNode node)
        {
            string id = "";

            if (!node.Removed) id = node.Element.FullName;

            //    Log.Message(description + ": " +node.Element.GetHashCode()+ " " + id);
            VS.Log.Message(description + ": " + id);
        }

        /// <summary>
        /// when change is noticed in registered span, this element is fired ->element is possibly changed
        /// </summary>
        /// <param name="node"></param>
        private void onElementChanged(ElementNode node)
        {
            logSpan("Changed", node);
            throw new NotImplementedException("Element has changed");
        }

        /// <summary>
        /// when new element is registered, this handler is called
        /// </summary>
        /// <param name="node"></param>
        private void onElementAdd(ElementNode node)
        {
            logSpan("Added", node);
           
            throw new NotImplementedException("Element has changed");
        }

        /// <summary>
        /// when element, which has been reported as added is dirty->removed, this handler is called on its wrapper
        /// </summary>
        /// <param name="node"></param>
        private void onElementRemoved(ElementNode node)
        {
            logSpan("Removed", node);

            throw new NotImplementedException("Element has been removed");
        }

        internal void Disconnect()
        {
            registerChanges(_root.RemoveChildren());
            _root = null;
            fireHandlers();
        }


        /// <summary>
        /// Owned elements reports all available namespaces - because of checking changes of namespaces
        /// Namespaces has changing hashcodes, even if no namespace change was made
        /// </summary>
        /// <param name="import"></param>
        internal void ReportNamespace(CodeImport import)
        {
            var ns = import.Namespace;
            _checkedNamespaces.Add(ns);
        }

        /// <summary>
        /// Return all available namespaces - can change during time
        /// </summary>
        public IEnumerable<string> Namespaces
        {
            get
            {
                return _namespaces;
            }
        }
    }
}