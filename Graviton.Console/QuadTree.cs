using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Graviton.Common.Drawing;


namespace Graviton.Common.Indexing
{
    public class QuadTree<T>
    {
        public QuadTree(int maxLevels, RectangleF root)
        {
            Build(maxLevels, new SizeF() { Height = 0f, Width = 0f }, root);
        }


        public QuadTree(SizeF minimumQuadSize, RectangleF root)
        {
            Build(int.MaxValue, minimumQuadSize, root);
        }

        public Quad Root { get; private set; }

        public Quad FindFirst(RectangleF target)
        {
            Quad quad = Root;
            return FindFirst(quad, target);
        }

        public Quad FindFirst(float x, float y)
        {
            Quad quad = Root;
            return FindFirst(quad, new RectangleF() { X = x, Y = y });
        }

        private Quad FindFirst(Quad quad, RectangleF target)
        {
            if (quad.Bounds.Intersects(target))
            {
                if (quad.IsLeaf) return quad;

                for(int x = 0; x < 2; x++)
                {
                    for(int y = 0; y < 2; y++)
                    {
                        var found = FindFirst(quad.Quads[x, y], target);
                        if (found != null) return found;
                    }
                }
            }

            return null;
        }

        public IEnumerable<Quad> FindAll(float x, float y)
        {
            Quad quad = Root;
            return FindAll(quad, new RectangleF() { X = x, Y = y });
        }

        public IEnumerable<Quad> FindAll(RectangleF target)
        {
            Quad quad = Root;
            return FindAll(quad, target);
        }

        private IEnumerable<Quad> FindAll(Quad quad, RectangleF target)
        {
            if (quad.Bounds.Intersects(target))
            {
                if (quad.IsLeaf)
                {
                    yield return quad;
                }
                else
                {
                    for (int x = 0; x < 2; x++)
                    {
                        for (int y = 0; y < 2; y++)
                        {
                            foreach (var found in FindAll(quad.Quads[x, y], target))
                                yield return found;
                        }
                    }
                }
            }
        }

        public void Traverse(Action<Quad> action, Func<Quad, bool> selector = null)
        {
            if (selector == null)
                selector = (q) => true;
            _Traverse(Root, action, selector);
        }

        private void _Traverse(Quad quad, Action<Quad> action, Func<Quad, bool> selector)
        {
            if (selector(quad))
                action(quad);

            if (!quad.IsLeaf)
            {
                for (int x = 0; x < 2; x++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        _Traverse(quad.Quads[x, y], action, selector);
                    }
                }
            }
        }

        private void Build(int maxLevels, SizeF minSize, RectangleF root)
        {
            Root = new Quad()
            {
                Bounds = root
            };
            Divide(Root, maxLevels, minSize, 0);
        }

        private void Divide(Quad quad, int maxLevels, SizeF minSize, int depth)
        {
            if (depth == maxLevels) return;
            depth++;

            var quadSize = new SizeF()
            {
                Height = quad.Bounds.Height / 2f,
                Width = quad.Bounds.Width / 2f
            };

            if (quadSize.Height < minSize.Height || quadSize.Width < minSize.Width) return;

            quad.Quads = new Quad[2,2];
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    quad.Quads[x, y] = new Quad()
                    {
                        Bounds = new RectangleF() { X = x * quadSize.Width + quad.Bounds.X, Y = y * quadSize.Height + quad.Bounds.Y, Width = quadSize.Width, Height = quadSize.Height }
                    };
                    Divide(quad.Quads[x,y], maxLevels, minSize, depth);
                }
            }
        }

        public class Quad
        {
            public Quad()
            {
                Items = new List<T>();
            }

            public RectangleF Bounds;
            public Quad[,] Quads;
            public bool IsLeaf { get { return Quads == null; } }
            public List<T> Items { get; private set; }
        }
    }
}
