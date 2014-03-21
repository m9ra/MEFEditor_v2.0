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
        internal void EndTransaction(Transaction transaction, IEnumerable<Action> afterActions)
        {
            throw new NotImplementedException();
        }
    }
}
