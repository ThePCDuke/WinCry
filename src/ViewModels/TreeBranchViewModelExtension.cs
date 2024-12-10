using System.Collections.Generic;
using System.Linq;

namespace WinCry.ViewModels
{
    public static class TreeBranchViewModelExtension
    {
        /// <summary>
        /// Enumerates all nodes in the tree
        /// </summary>
        /// <typeparam name="T">Type</typeparam>
        /// <param name="root">Root node of the tree</param>
        /// <returns></returns>
        public static IEnumerable<TreeBranchViewModel<T>> Flatten<T>(this TreeBranchViewModel<T> root)
        {
            var stack = new Stack<IEnumerator<TreeBranchViewModel<T>>>();
            stack.Push(root.GetEnumerator());

            yield return root;

            while (stack.Any())
            {
                var node = stack.Pop();
                while (node.MoveNext())
                {
                    yield return node.Current;
                    if (node.Current.Any())
                    {
                        stack.Push(node);
                        stack.Push(node.Current.GetEnumerator());
                        break;
                    }
                }
            }
        }
    }
}
