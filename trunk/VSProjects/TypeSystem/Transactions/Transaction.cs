using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeSystem.Transactions
{
    /// <summary>
    /// Transaction representing bunch of executed tasks that can be named
    /// for informing user about its progress.
    /// </summary>
    public class Transaction
    {
        /// <summary>
        /// Manager that creates current transaction
        /// </summary>
        private readonly TransactionManager _manager;

        /// <summary>
        /// Actions that will be processed after transaction end
        /// </summary>
        private readonly List<Action> _afterActions = new List<Action>();

        /// <summary>
        /// Description of transaction that can be displayed to the user
        /// </summary>
        public readonly string Description;

        /// <summary>
        /// Current description of progress that can be displayed to user. If no progress
        /// status is available is <c>null</c>.
        /// </summary>
        public string ProgressStatus { get; private set; }

        /// <summary>
        /// Determine that transaction is already running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Initialize <see cref="Transaction"/> object
        /// </summary>
        /// <param name="description">Description of transaction that can be displayed to the user</param>
        /// <param name="manager">Manager that creates current transaction</param>
        internal Transaction(TransactionManager manager, string description)
        {
            IsRunning = true;

            _manager = manager;
            Description = description;
        }

        /// <summary>
        /// Report progress status of processing current transaction
        /// </summary>
        /// <param name="statusDescription">Description of current status that can be displayed to the user</param>
        public void ReportProgress(string statusDescription)
        {
            ensureRunning();

            ProgressStatus = statusDescription;
        }

        /// <summary>
        /// End current transaction. After actions could be processed
        /// </summary>
        public void End()
        {
            ensureRunning();

            ProgressStatus = null;
            IsRunning = false;

            _manager.EndTransaction(this, _afterActions);
        }

        /// <summary>
        /// Ensure that current transaction is running
        /// </summary>
        private void ensureRunning()
        {
            if (!IsRunning)
                throw new InvalidOperationException("Cannot process requested operation on transaction that is not running");
        }
    }
}
