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

        private readonly Queue<TransactionAction> _rootActions = new Queue<TransactionAction>();

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
            if (transaction == null)
            {
                _rootActions.Enqueue(afterAction);
            }
            else
            {
                _activeTransactions[transaction].Add(afterAction);
            }
        }

        /// <summary>
        /// Note that ending of transaction has to be done through <see cref="Transaction.Commit"/>
        /// </summary>
        /// <param name="transaction">Transaction that is ended</param>
        internal void EndTransaction(Transaction transaction)
        {
            var popped = _transactionStack.Pop();
            if (popped != transaction)
                throw new NotImplementedException("Auto commit transaction stack unwiding");

            var afterActions = _activeTransactions[transaction];
            _activeTransactions.Remove(transaction);

            foreach (var afterAction in afterActions)
            {
                afterAction.Run();
            }

            var otherTransactions = _transactionStack.Count > 0;
            if (otherTransactions)
                return;

            //run root actions at last
            while (_rootActions.Count > 0)
            {
                var action = _rootActions.Dequeue();
                action.Run();
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
