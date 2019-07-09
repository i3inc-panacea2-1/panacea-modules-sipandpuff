using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.SipAndPuff
{
    class Tree<T>
    {
        public T Value { get; }

        public Tree<T> Parent { get; private set; }

        public Tree(T node, Tree<T> parent)
        {
            Value = node;
            Parent = parent;
            _children = new List<Tree<T>>();
        }

        public Tree<T> Root
        {
            get
            {
                if (Parent == null) return this;
                return Parent.Root;
            }
        }

        public bool IsLeaf { get => _children.Count == 0; }

        public bool IsNode { get => !IsLeaf; }

        public bool IsRoot { get => Parent == null; }

        List<Tree<T>> _children;
        public IReadOnlyList<Tree<T>> Children { get => _children.AsReadOnly(); }

        public void Add(Tree<T> item)
        {
            item.Parent = this;
            _children.Add(item);
        }

        public Tree<T> Reduce(Func<Tree<T>, bool> condition)
        {
            var lst = Children.Select(c => c.Reduce(condition)).Where(c => c != null).ToList();
            var @new = new Tree<T>(Value, Parent)
            {
                _children = lst
            };
            if (!condition(@new))
            {
                return null;
            }
            if (lst.Count == 1)
            {
                return lst.First();
            }
            return @new;
        }

        public int IndexOfChild(Tree<T> child)
        {
            return _children.IndexOf(child);
        }
    }
}
