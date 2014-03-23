using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeSystem.Transactions
{
    /// <summary>
    /// Manager used for processing transactions
    /// </summary>
    class TransactionManager
    {
        private readonly Stack<Transaction> _transactionStack = new Stack<Transaction>();
        private readonly Dictionary<Transaction, List<TransactionAction>> _activeTransactions = new Dictionary<Transaction, List<TransactionAction>>();


        internal Transaction CurrentTransaction
        {
            get
            {
                if (_transactionStack.Count == 0)
                    return null;

                return _transactionStack.Peek();
            }
        }

        internal void AttachAfterAction(Transaction transaction, TransactionAction afterAction)
        {
            _activeTransactions[transaction].Add(afterAction);
        }

        internal void EndTransaction(Transaction transaction)
        {
            if (_transactionStack.Pop() != transaction)
                throw new NotImplementedException("Auto commit transaction stack unwiding");
            
            var afterActions = _activeTransactions[transaction];
            _activeTransactions.Remove(transaction);

            foreach (var afterAction in afterActions)
            {
                afterAction.Run();
            }
        }

        internal Transaction StartNew(string description)
        {
            var transaction = new Transaction(this, description);
            _transactionStack.Push(transaction);
            _activeTransactions[transaction] = new List<TransactionAction>();

            return transaction;
        }
    }
}
