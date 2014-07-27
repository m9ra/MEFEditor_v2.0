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
    /// <returns><c>true</c> if action is included within correspondign action, <c>false</c> otherwise</returns>
    public delegate bool IsIncludedPredicate(TransactionAction action);

    public class TransactionAction
    {
        private readonly Action _action;

        private readonly IsIncludedPredicate _predicate;

        private readonly object[] _keys;

        public readonly string Name;


        public TransactionAction(Action action, string name, IsIncludedPredicate predicate, params object[] keys)
        {
            _action = action;
            Name = name;
            _predicate = predicate;
            //defensive copy
            _keys = keys.ToArray();
        }

        internal void Run()
        {
            _action();
        }

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
