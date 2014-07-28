using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MEFEditor.TypeSystem.Transactions
{
    /// <summary>
    /// Event used for reporting transaction changes
    /// </summary>
    /// <param name="transaction">Affected transaction</param>
    public delegate void TransactionEvent(Transaction transaction);

    /// <summary>
    /// Manager used for providing transactions and handling theire correct commits.
    /// </summary>
    public class TransactionManager
    {
        /// <summary>
        /// The stack of currently running transactions.
        /// </summary>
        private readonly Stack<Transaction> _transactionStack = new Stack<Transaction>();

        /// <summary>
        /// The index of active transactions.
        /// </summary>
        private readonly Dictionary<Transaction, List<TransactionAction>> _activeTransactions = new Dictionary<Transaction, List<TransactionAction>>();

        /// <summary>
        /// The actions attached with no transaction.
        /// </summary>
        private readonly Queue<TransactionAction> _rootActions = new Queue<TransactionAction>();

        /// <summary>
        /// Occurs when new transaction is started.
        /// </summary>
        public event TransactionEvent TransactionOpened;

        /// <summary>
        /// Occurs when progress of transaction is changed.
        /// </summary>
        public event TransactionEvent TransactionProgressChanged;

        /// <summary>
        /// Occurs when transaction is committed.
        /// </summary>
        public event TransactionEvent TransactionCommit;

        /// <summary>
        /// Gets the current transaction.
        /// </summary>
        /// <value>The current transaction.</value>
        public Transaction CurrentTransaction
        {
            get
            {
                if (_transactionStack.Count == 0)
                    return null;

                return _transactionStack.Peek();
            }
        }

        /// <summary>
        /// Attach after action to given transaction. After action
        /// will be executed after transaction commit.
        /// </summary>
        /// <param name="transaction">The transaction.</param>
        /// <param name="afterAction">The after action.</param>
        public void AttachAfterAction(Transaction transaction, TransactionAction afterAction)
        {
            if (transaction == null)
            {
                if (!isIncludedIn(afterAction, _rootActions))
                    _rootActions.Enqueue(afterAction);

                tryRunRootActions();
            }
            else
            {
                var actions = _activeTransactions[transaction];
                if (!isIncludedIn(afterAction, actions))
                    actions.Add(afterAction);
            }
        }

        /// <summary>
        /// Determines whether action is included in given enumeration.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="existingActions">The existing actions.</param>
        /// <returns><c>true</c> if action is included; otherwise, <c>false</c>.</returns>
        private bool isIncludedIn(TransactionAction action, IEnumerable<TransactionAction> existingActions)
        {
            foreach (var existingAction in existingActions)
            {
                if (action.IsIncludedIn(existingAction))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Internal handling of transaction ending.
        /// Note that ending of transaction has to be done through <see cref="Transaction.Commit" />.
        /// </summary>
        /// <param name="transaction">Transaction that is ended.</param>
        internal void EndTransaction(Transaction transaction)
        {
            var popped = _transactionStack.Pop();
            if (popped != transaction)
                //transaction stack unwiding
                EndTransaction(popped);

            var afterActions = _activeTransactions[transaction];
            _activeTransactions.Remove(transaction);

            foreach (var afterAction in afterActions)
            {
                afterAction.Run();
            }

            tryRunRootActions();

            if (TransactionCommit != null)
                TransactionCommit(transaction);
        }

        /// <summary>
        /// Reports progress of transaction.
        /// </summary>
        /// <param name="transaction">Transaction which progress has changed.</param>
        internal void ReportProgress(Transaction transaction)
        {
            if (TransactionProgressChanged != null)
                TransactionProgressChanged(transaction);
        }

        /// <summary>
        /// Tries the run root actions.
        /// </summary>
        private void tryRunRootActions()
        {
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

        /// <summary>
        /// Starts the new transaction with given description.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <returns>Started transaction.</returns>
        public Transaction StartNew(string description)
        {
            var transaction = new Transaction(this, description);
            _transactionStack.Push(transaction);
            _activeTransactions[transaction] = new List<TransactionAction>();

            if (TransactionOpened != null)
                TransactionOpened(transaction);

            return transaction;
        }


    }
}
