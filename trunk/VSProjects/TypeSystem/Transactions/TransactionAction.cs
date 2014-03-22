using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeSystem.Transactions
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


        internal TransactionAction(Action action, string name, IsIncludedPredicate predicate, params object[] keys)
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
    }
}
