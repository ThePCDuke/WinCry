using System.Collections;

namespace WinCry.Models
{
    /// <summary>
    /// Interface containing all methods to be implemented
    /// by string search algorithm
    /// </summary>
    public interface IStringSearchAlgorithm
    {
        /// <summary>
        /// List of keywords to search for
        /// </summary>
        string[] Keywords { get; set; }

        /// <summary>
        /// Searches passed text and returns all occurrences of any keyword
        /// </summary>
        /// <param name="text">Text to search</param>
        /// <returns>Array of occurrences</returns>
        StringSearchResult[] FindAll(string text);

        /// <summary>
        /// Searches passed text and returns first occurrence of any keyword
        /// </summary>
        /// <param name="text">Text to search</param>
        /// <returns>First occurrence of any keyword (or StringSearchResult.Empty if text doesn't contain any keyword)</returns>
        StringSearchResult FindFirst(string text);

        /// <summary>
        /// Searches passed text and returns true if text contains any keyword
        /// </summary>
        /// <param name="text">Text to search</param>
        /// <returns>True when text contains any keyword</returns>
        bool ContainsAny(string text);
    }

    /// <summary>
    /// Structure containing results of search 
    /// (keyword and position in original text)
    /// </summary>
    public struct StringSearchResult
    {
        /// <summary>
        /// Initialize string search result
        /// </summary>
        /// <param name="index">Index in text</param>
        /// <param name="keyword">Found keyword</param>
        public StringSearchResult(int index, string keyword)
        {
            Index = index; Keyword = keyword;
        }

        /// <summary>
        /// Returns index of found keyword in original text
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Returns keyword found by this result
        /// </summary>
        public string Keyword { get; }

        /// <summary>
        /// Returns empty search result
        /// </summary>
        public static StringSearchResult Empty => new StringSearchResult(-1, "");
    }

    /// <summary>
    /// Class for searching string for one or multiple 
    /// keywords using efficient Aho-Corasick search algorithm
    /// </summary>
    public class StringSearch : IStringSearchAlgorithm
    {
        /// <summary>
        /// Tree node representing character and its 
        /// transition and failure function
        /// </summary>
        class TreeNode
        {
            /// <summary>
            /// Initialize tree node with specified character
            /// </summary>
            /// <param name="parent">Parent node</param>
            /// <param name="c">Character</param>
            public TreeNode(TreeNode parent, char c)
            {
                Char = c; Parent = parent;
                _results = new ArrayList();
                Results = new string[] { };

                Transitions = new TreeNode[] { };
                _transHash = new Hashtable();
            }

            /// <summary>
            /// Adds pattern ending in this node
            /// </summary>
            /// <param name="result">Pattern</param>
            public void AddResult(string result)
            {
                if (_results.Contains(result))
                    return;

                _results.Add(result);
                Results = (string[])_results.ToArray(typeof(string));
            }

            /// <summary>
            /// Adds trabsition node
            /// </summary>
            /// <param name="node">Node</param>
            public void AddTransition(TreeNode node)
            {
                _transHash.Add(node.Char, node);
                TreeNode[] ar = new TreeNode[_transHash.Values.Count];
                _transHash.Values.CopyTo(ar, 0);
                Transitions = ar;
            }

            /// <summary>
            /// Returns transition to specified character (if exists)
            /// </summary>
            /// <param name="c">Character</param>
            /// <returns>Returns TreeNode or null</returns>
            public TreeNode GetTransition(char c)
            {
                return (TreeNode)_transHash[c];
            }

            /// <summary>
            /// Returns true if node contains transition to specified character
            /// </summary>
            /// <param name="c">Character</param>
            /// <returns>True if transition exists</returns>
            public bool ContainsTransition(char c)
            {
                return GetTransition(c) != null;
            }

            private ArrayList _results;
            private Hashtable _transHash;

            /// <summary>
            /// Character
            /// </summary>
            public char Char { get; }

            /// <summary>
            /// Parent tree node
            /// </summary>
            public TreeNode Parent { get; }

            /// <summary>
            /// Failure function - descendant node
            /// </summary>
            public TreeNode Failure { get; set; }

            /// <summary>
            /// Transition function - list of descendant nodes
            /// </summary>
            public TreeNode[] Transitions { get; private set; }

            /// <summary>
            /// Returns list of patterns ending by this letter
            /// </summary>
            public string[] Results { get; private set; }
        }

        /// <summary>
        /// Root of keyword tree
        /// </summary>
        private TreeNode _root;

        /// <summary>
        /// Keywords to search for
        /// </summary>
        private string[] _keywords;

        /// <summary>
        /// Initialize search algorithm (Build keyword tree)
        /// </summary>
        /// <param name="keywords">Keywords to search for</param>
        public StringSearch(string[] keywords)
        {
            Keywords = keywords;
        }

        /// <summary>
        /// Initialize search algorithm with no keywords
        /// (Use Keywords property)
        /// </summary>
        public StringSearch()
        { }

        /// <summary>
        /// Build tree from specified keywords
        /// </summary>
        void BuildTree()
        {
            // Build keyword tree and transition function
            _root = new TreeNode(null, ' ');
            foreach (string p in _keywords)
            {
                // add pattern to tree
                TreeNode nd = _root;
                foreach (char c in p)
                {
                    TreeNode ndNew = null;
                    foreach (TreeNode trans in nd.Transitions)
                    {
                        if (trans.Char == c)
                        {
                            ndNew = trans;
                            break;
                        }
                    }

                    if (ndNew == null)
                    {
                        ndNew = new TreeNode(nd, c);
                        nd.AddTransition(ndNew);
                    }
                    nd = ndNew;
                }
                nd.AddResult(p);
            }

            // Find failure functions
            ArrayList nodes = new ArrayList();
            // level 1 nodes - fail to root node
            foreach (TreeNode nd in _root.Transitions)
            {
                nd.Failure = _root;
                foreach (TreeNode trans in nd.Transitions) nodes.Add(trans);
            }
            // other nodes - using BFS
            while (nodes.Count != 0)
            {
                ArrayList newNodes = new ArrayList();
                foreach (TreeNode nd in nodes)
                {
                    TreeNode r = nd.Parent.Failure;
                    char c = nd.Char;

                    while (r != null && !r.ContainsTransition(c)) r = r.Failure;
                    if (r == null)
                        nd.Failure = _root;
                    else
                    {
                        nd.Failure = r.GetTransition(c);
                        foreach (string result in nd.Failure.Results)
                            nd.AddResult(result);
                    }

                    // add child nodes to BFS list 
                    foreach (TreeNode child in nd.Transitions)
                        newNodes.Add(child);
                }
                nodes = newNodes;
            }
            _root.Failure = _root;
        }

        /// <summary>
        /// Keywords to search for (setting this property is slow, because
        /// it requieres rebuilding of keyword tree)
        /// </summary>
        public string[] Keywords
        {
            get { return _keywords; }
            set
            {
                _keywords = value;
                BuildTree();
            }
        }

        /// <summary>
        /// Searches passed text and returns all occurrences of any keyword
        /// </summary>
        /// <param name="text">Text to search</param>
        /// <returns>Array of occurrences</returns>
        public StringSearchResult[] FindAll(string text)
        {
            ArrayList ret = new ArrayList();
            TreeNode ptr = _root;
            int index = 0;

            while (index < text.Length)
            {
                TreeNode trans = null;
                while (trans == null)
                {
                    trans = ptr.GetTransition(text[index]);
                    if (ptr == _root) break;
                    if (trans == null) ptr = ptr.Failure;
                }
                if (trans != null) ptr = trans;

                foreach (string found in ptr.Results)
                    ret.Add(new StringSearchResult(index - found.Length + 1, found));
                index++;
            }
            return (StringSearchResult[])ret.ToArray(typeof(StringSearchResult));
        }

        /// <summary>
        /// Searches passed text and returns first occurrence of any keyword
        /// </summary>
        /// <param name="text">Text to search</param>
        /// <returns>First occurrence of any keyword (or StringSearchResult.Empty if text doesn't contain any keyword)</returns>
        public StringSearchResult FindFirst(string text)
        {
            ArrayList ret = new ArrayList();
            TreeNode ptr = _root;
            int index = 0;

            while (index < text.Length)
            {
                TreeNode trans = null;
                while (trans == null)
                {
                    trans = ptr.GetTransition(text[index]);
                    if (ptr == _root) break;
                    if (trans == null) ptr = ptr.Failure;
                }
                if (trans != null) ptr = trans;

                foreach (string found in ptr.Results)
                    return new StringSearchResult(index - found.Length + 1, found);
                index++;
            }
            return StringSearchResult.Empty;
        }

        /// <summary>
        /// Searches passed text and returns true if text contains any keyword
        /// </summary>
        /// <param name="text">Text to search</param>
        /// <returns>True when text contains any keyword</returns>
        public bool ContainsAny(string text)
        {
            TreeNode ptr = _root;
            int index = 0;

            while (index < text.Length)
            {
                TreeNode trans = null;
                while (trans == null)
                {
                    trans = ptr.GetTransition(text[index]);
                    if (ptr == _root) break;
                    if (trans == null) ptr = ptr.Failure;
                }
                if (trans != null) ptr = trans;

                if (ptr.Results.Length > 0) return true;
                index++;
            }
            return false;
        }
    }
}