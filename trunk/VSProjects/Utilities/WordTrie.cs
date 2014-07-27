using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    /// <summary>
    /// Implementation of trie using word nodes.
    /// </summary>
    /// <typeparam name="TKey">The type of the t key.</typeparam>
    /// <typeparam name="TValue">The type of the t value.</typeparam>
    public abstract class WordTrie<TKey, TValue>
    {
        /// <summary>
        /// The root node of trie
        /// </summary>
        private readonly TrieNode<TValue> _root = new TrieNode<TValue>();

        /// <summary>
        /// Split key into separate words that are used for accessing trie node.
        /// </summary>
        /// <param name="key">Key that is split.</param>
        /// <returns>Result of word split.</returns>
        protected abstract IEnumerable<string> wordSpliter(TKey key);

        /// <summary>
        /// Adds the value at specified key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if value was added, <c>false</c> otherwise.</returns>
        public bool Add(TKey key, TValue value)
        {
            var node = getNode(key, true);
            return node.Values.Add(value);
        }

        /// <summary>
        /// Removes all keys with given prefix.
        /// </summary>
        /// <param name="prefix">The prefix.</param>
        /// <returns>Values of removed nodes.</returns>
        public IEnumerable<TValue> RemoveWithPrefix(TKey prefix)
        {
            var result=new List<TValue>();

            var words = wordSpliter(prefix);
            var removeRoot= getNode(words, false);
            if (removeRoot == null)
                return result;

            //traverse all subnodes 
            var waitingNodes = new Queue<TrieNode<TValue>>();
            waitingNodes.Enqueue(removeRoot);

            while (waitingNodes.Count > 0)
            {
                var toReport = waitingNodes.Dequeue();

                result.AddRange(toReport.Values);

                foreach (var subNode in toReport.Suffixes)
                {
                    waitingNodes.Enqueue(subNode);
                }
            }

            //remove all matching nodes
            removeRoot.Clear();
            return result;
        }

        /// <summary>
        /// Gets node for given key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <param name="create">if set to <c>true</c> new node can be created.</param>
        /// <returns>The node.</returns>
        private TrieNode<TValue> getNode(TKey key, bool create = false)
        {
            var words = wordSpliter(key);

            return getNode(words, create);
        }

        /// <summary>
        /// Gets node for given words.
        /// </summary>
        /// <param name="words">The words.</param>
        /// <param name="create">if set to <c>true</c> new node can be created.</param>
        /// <returns>The node.</returns>
        private TrieNode<TValue> getNode(IEnumerable<string> words, bool create)
        {
            var current = _root;
            foreach (var word in words)
            {
                current = current.GetSuffix(word, create);

                if (current == null)
                    return null;
            }

            return current;
        }
    }
    

    /// <summary>
    /// Node used by <see cref="WordTrie{TKey,ValueType}"/>.
    /// </summary>
    /// <typeparam name="ValueType">The type of the value.</typeparam>
    class TrieNode<ValueType>
    {
        /// <summary>
        /// The stored suffixes.
        /// </summary>
        private readonly Dictionary<string, TrieNode<ValueType>> _suffixes = new Dictionary<string, TrieNode<ValueType>>();

        /// <summary>
        /// Gets the suffixes.
        /// </summary>
        /// <value>The suffixes.</value>
        internal IEnumerable<TrieNode<ValueType>> Suffixes { get { return _suffixes.Values; } }

        /// <summary>
        /// The values.
        /// </summary>
        internal readonly HashSet<ValueType> Values = new HashSet<ValueType>();

        /// <summary>
        /// Gets the suffix node of given word.
        /// </summary>
        /// <param name="word">The word.</param>
        /// <param name="create">if set to <c>true</c> can be created.</param>
        /// <returns>Suffix node.</returns>
        internal TrieNode<ValueType> GetSuffix(string word, bool create)
        {
            TrieNode<ValueType> result;

            if (!_suffixes.TryGetValue(word, out result) && create)
            {
                result = new TrieNode<ValueType>();
                _suffixes[word] = result;
            }

            return result;
        }

        /// <summary>
        /// Clears all stored data.
        /// </summary>
        internal void Clear()
        {
            _suffixes.Clear();
            Values.Clear();
        }
    }
}
