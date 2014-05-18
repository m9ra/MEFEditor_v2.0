using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Utilities
{
    /// <summary>
    /// Implementation of trie using word nodes
    /// </summary>
    public abstract class WordTrie<TKey, TValue>
    {
        private readonly TrieNode<TValue> _root = new TrieNode<TValue>();

        /// <summary>
        /// Split key into separate words that are used for accessing trie node
        /// </summary>
        /// <param name="key">Key that is split</param>
        /// <returns>Result of word split</returns>
        protected abstract IEnumerable<string> wordSpliter(TKey key);

        public bool Add(TKey key, TValue value)
        {
            var node = getNode(key, true);
            return node.Values.Add(value);
        }

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

        private TrieNode<TValue> getNode(TKey key, bool create = false)
        {
            var words = wordSpliter(key);

            return getNode(words, create);
        }

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




    class TrieNode<ValueType>
    {
        private readonly Dictionary<string, TrieNode<ValueType>> _suffixes = new Dictionary<string, TrieNode<ValueType>>();

        internal IEnumerable<TrieNode<ValueType>> Suffixes { get { return _suffixes.Values; } }

        internal readonly HashSet<ValueType> Values = new HashSet<ValueType>();

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

        internal void Clear()
        {
            _suffixes.Clear();
            Values.Clear();
        }
    }
}
