using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeSystem.Transactions
{
    /// <summary>
    /// Manager used for processing transactions
    /// </summary>
    public class TransactionManager
    {
        private readonly Stack<Transaction> _transactionStack = new Stack<Transaction>();
        private readonly Dictionary<Transaction, List<TransactionAction>> _activeTransactions = new Dictionary<Transaction, List<TransactionAction>>();


        public Transaction CurrentTransaction
        {
            get
            {
                if (_transactionStack.Count == 0)
                    return null;

                return _transactionStack.Peek();
            }
        }

        public void AttachAfterAction(Transaction transaction, TransactionAction afterAction)
        {
            _activeTransactions[transaction].Add(afterAction);
        }

        public void EndTransaction(Transaction transaction)
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

        public Transaction StartNew(string description)
        {
            var transaction = new Transaction(this, description);
            _transactionStack.Push(transaction);
            _activeTransactions[transaction] = new List<TransactionAction>();

            return transaction;
        }
    }
}
