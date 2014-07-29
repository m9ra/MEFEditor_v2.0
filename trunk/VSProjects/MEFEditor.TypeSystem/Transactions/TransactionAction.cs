using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEFEditor.TypeSystem.Transactions
{
    /// <summary>
    /// Predicate used for determination of inclusion of
    /// </summary>
    /// <param name="action">Action that</param>
    /// <returns><c>true</c> if action is included within corresponding action, <c>false</c> otherwise</returns>
    public delegate bool IsIncludedPredicate(TransactionAction action);

    /// <summary>
    /// Represents action that can be attached to transaction. 
    /// After transaction ending the action is executed.
    /// </summary>
    public class TransactionAction
    {
        /// <summary>
        /// The stored action.
        /// </summary>
        private readonly Action _action;

        /// <summary>
        /// The predicate determining whether action is
        /// included in transaction already.
        /// </summary>
        private readonly IsIncludedPredicate _predicate;

        /// <summary>
        /// The stored keys that helps action identification.
        /// </summary>
        private readonly object[] _keys;

        /// <summary>
        /// Name of transaction action.
        /// </summary>
        public readonly string Name;


        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAction" /> class.
        /// </summary>
        /// <param name="action">The action that will be executed.</param>
        /// <param name="name">The name of transaction action.</param>
        /// <param name="predicate">The predicate determining whether action is
        /// included in transaction already.</param>
        /// <param name="keys">The stored keys that helps action identification.</param>
        public TransactionAction(Action action, string name, IsIncludedPredicate predicate, params object[] keys)
        {
            _action = action;
            Name = name;
            _predicate = predicate;
            //defensive copy
            _keys = keys.ToArray();
        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        internal void Run()
        {
            _action();
        }

        /// <summary>
        /// Determines whether action is included in transaction.
        /// </summary>
        /// <param name="coveringAction">The action that is tested for action cover.</param>
        /// <returns><c>true</c> if action is included false; otherwise, <c>false</c>.</returns>
        internal bool IsIncludedIn(TransactionAction coveringAction)
        {
            //incompatible keys
            if (_keys.Length != coveringAction._keys.Length)
                return false;

            //different keys
            for (var i = 0; i < _keys.Length; ++i)
            {
                if (!_keys[i].Equals(coveringAction._keys[i]))
                    return false;
            }

            return _predicate(coveringAction);
        }
    }
}
