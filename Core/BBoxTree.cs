using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class BBoxTree
    {
        public List<Vector2d> MinBounds;
        public List<Vector2d> MaxBounds;
        public List<List<int>> Children;
        public List<bool> IsLeaf;
        public List<int> Parents;
        public List<int> Depths;
        public List<List<int>> IndicesByLayer;
        public List<List<int>> OverlappingBBoxIndices;
        public List<bool> HasMoved;

        const float padding = 10.0f;

#if DEBUG
        static int d_numBoxTestsThisFrame;
        static int d_numBoxOverlapsThisFrame;
        static int d_frameCount = 0;
        static int d_cumulativeBoxTests;
#endif

        public BBoxTree()
        {
            MinBounds = new List<Vector2d>();
            MaxBounds = new List<Vector2d>();
            Children = new List<List<int>>();
            IsLeaf = new List<bool>();
            Parents = new List<int>();
            IndicesByLayer = new List<List<int>>();
            Depths = new List<int>();
            OverlappingBBoxIndices = new List<List<int>>();
            HasMoved = new List<bool>();

            // Add a root node
            MinBounds.Add(Vector2d.MaxVector());
            MaxBounds.Add(Vector2d.MinVector());
            Children.Add(new List<int>());
            IsLeaf.Add(false);
            IndicesByLayer.Add(new List<int>());
            IndicesByLayer[0].Add(0);
            Depths.Add(0);
            Parents.Add(-1);
            HasMoved.Add(false);
            
            // Kinda kludgy here.  We only test intersection with boxes
            // that are children of boxes that overlap our parent, not
            // the other children of our parent.  Since we reject those
            // collisions anyway that would just be wasted cycles.  The
            // problem is that the root element has no siblings and we
            // want to test the layer 1 elements against their siblings.
            OverlappingBBoxIndices.Add(new List<int>());
            OverlappingBBoxIndices[0].Add(0);
        }

        public void UpdateOverlapping(List<Vector2d> objectPoisitions, List<float> objectRadii, RenderArrays bBoxRenderArrays)
        {
#if DEBUG
            d_numBoxTestsThisFrame = 0;
            d_frameCount++;
#endif
            // Walk the tree breadth-fisrt from the bottom and update the boumding boxes

            bBoxRenderArrays.Positions.Clear();
            bBoxRenderArrays.Colours.Clear();

            for (int depth = IndicesByLayer.Count - 1; depth > 0; depth--)
            {
                if (depth == IndicesByLayer.Count - 1)
                {
                    foreach (int boxIndex in IndicesByLayer[depth])
                    {
                        Vector2d min = Vector2d.MaxVector();
                        Vector2d max = Vector2d.MinVector();

                        foreach (int blobIndex in Children[boxIndex])
                        {
                            Vector2d pos = objectPoisitions[blobIndex];
                            float radius = objectRadii[blobIndex];
                            min = Vector2d.ComponentWiseMin(min, pos - radius);
                            max = Vector2d.ComponentWiseMax(max, pos + radius);
                        }

                        if (min.EitherComponentLessThan(MinBounds[boxIndex]) || max.EitherComponentGreaterThan(MaxBounds[boxIndex]))
                        {
                            HasMoved[boxIndex] = true;
                            MinBounds[boxIndex] = min - padding;
                            MaxBounds[boxIndex] = max + padding;
                        }
                        else
                        {
                            HasMoved[boxIndex] = false;
                        }

                        /*bBoxRenderArrays.Colours.Add(new Colour() { A = 0.0f, R = 0.5f, B = 0.5f, G = 0.5f });
                        bBoxRenderArrays.Positions.Add(min);
                        bBoxRenderArrays.Colours.Add(new Colour() { A = 0.0f, R = 0.5f, B = 0.5f, G = 0.5f });
                        bBoxRenderArrays.Positions.Add(max);*/
                    }
                }
                else
                {
                    foreach (int boxIndex in IndicesByLayer[depth])
                    {
                        Vector2d min = Vector2d.MaxVector();
                        Vector2d max = Vector2d.MinVector();

                        foreach (int childIndex in Children[boxIndex])
                        {
                            min = Vector2d.ComponentWiseMin(min, MinBounds[childIndex]);
                            max = Vector2d.ComponentWiseMax(max, MaxBounds[childIndex]);
                        }

                        if (MinBounds[boxIndex] != min || MaxBounds[boxIndex] != max)
                        {
                            HasMoved[boxIndex] = true;
                            MinBounds[boxIndex] = min;
                            MaxBounds[boxIndex] = max;
                        }
                        else
                        {
                            HasMoved[boxIndex] = false;
                        }

                        /*bBoxRenderArrays.Colours.Add(new Colour() { A = 0.0f, R = 0.5f, B = 0.5f, G = 0.5f });
                        bBoxRenderArrays.Positions.Add(min);
                        bBoxRenderArrays.Colours.Add(new Colour() { A = 0.0f, R = 0.5f, B = 0.5f, G = 0.5f });
                        bBoxRenderArrays.Positions.Add(max);*/
                    }
                }
            }

            // Walk bredath-first from the top and find lists of overlaps.
            for (int depth = 1; depth < IndicesByLayer.Count; depth++)
            {
                foreach (var bBoxIndex in IndicesByLayer[depth])
                {
                    var parentIndex = Parents[bBoxIndex];

                    foreach (var parentOverlap in OverlappingBBoxIndices[parentIndex])
                    {
                        foreach (var boxToTestAgainst in Children[parentOverlap])
                        {
                            if (HasMoved[bBoxIndex] || HasMoved[boxToTestAgainst])
                            {
                                for (int i = 0; i < OverlappingBBoxIndices[bBoxIndex].Count; i++)
                                {
                                    int oldOverlap = OverlappingBBoxIndices[bBoxIndex][i];
                                    if (OverlappingBBoxIndices[parentIndex].Contains(Parents[oldOverlap]) == false)
                                    {
                                        OverlappingBBoxIndices[bBoxIndex].Remove(oldOverlap);
                                        i--;
                                    }
                                }
                                if (boxToTestAgainst != bBoxIndex)
                                {
                                    int boxToHoldCollision = Math.Min(bBoxIndex, boxToTestAgainst);
                                    int boxNotHoldingcollision = Math.Max(bBoxIndex, boxToTestAgainst);

                                    if (AreOverlapping(MinBounds[bBoxIndex], MaxBounds[bBoxIndex], MinBounds[boxToTestAgainst], MaxBounds[boxToTestAgainst]))
                                    {
                                        if (OverlappingBBoxIndices[boxToHoldCollision].Contains(boxNotHoldingcollision) == false)
                                        {
                                            OverlappingBBoxIndices[boxToHoldCollision].Add(boxNotHoldingcollision);
                                        }
                                    }
                                    else
                                    {
                                        OverlappingBBoxIndices[boxToHoldCollision].Remove(boxNotHoldingcollision);
                                    }
                                }
                            }
                        }
                    }
                }
            }

#if DEBUG
            if (d_frameCount >= 100)
            {
                d_frameCount = 0;
                int d_averageBoxTests = d_cumulativeBoxTests / 100;
                d_cumulativeBoxTests = 0;
            }
            d_numBoxOverlapsThisFrame = 0;
            List<Tuple<int, int>> d_foundCollisions = new List<Tuple<int,int>>();
            for (int i = 0; i < OverlappingBBoxIndices.Count; i++)
            {
                var overlaps = OverlappingBBoxIndices[i];
                d_numBoxOverlapsThisFrame += overlaps.Count;
                foreach(var overlap in overlaps)
                {
            //        d_foundCollisions.Add(new Tuple<int,int>(i, overlap));
                }
            }

        /*    for (int i = 0; i < d_foundCollisions.Count; i++)
            {
                for (int j = i + 1; j < d_foundCollisions.Count; j++)
                {
                    var col1 = d_foundCollisions[i];
                    var col2 = d_foundCollisions[j];
                    if ((col1.Item1 == col2.Item1 && col1.Item2 == col2.Item2) ||
                        (col1.Item1 == col2.Item2 && col1.Item2 == col2.Item1))
                    {
                        System.Diagnostics.Debug.Assert(false, "Error: same pair of AABBs recorded as overlapping twice");
                    }
                }
            }*/
#endif
        }

        static bool AreOverlapping(Vector2d min1, Vector2d max1, Vector2d min2, Vector2d max2)
        {
#if DEBUG
            d_numBoxTestsThisFrame++;
            d_cumulativeBoxTests++;
#endif

            Vector2d widthHeight1 = max1 - min1;
            Vector2d widthHeight2 = max2 - min2;
            Vector2d center1 = (max1 + min1) / 2.0f;
            Vector2d center2 = (max2 + min2) / 2.0f;

            float testWidth = widthHeight1.X + widthHeight2.X;
            float testHeight = widthHeight1.Y + widthHeight2.Y;

            return (Math.Abs(center1.X - center2.X) * 2.0f <= testWidth &&
                Math.Abs(center1.Y - center2.Y) * 2.0f <= testHeight);
        }

        public void AddChildIndexToNode(int nodeIndex, int childIndex)
        {
            Children[nodeIndex].Add(childIndex);
        }

        public int AddNode(int parent, bool isLeaf)
        {
            int newNodeIndex = Children.Count;

            Children[parent].Add(newNodeIndex);
            Children.Add(new List<int>());
            OverlappingBBoxIndices.Add(new List<int>());
            MinBounds.Add(new Vector2d());
            MaxBounds.Add(new Vector2d());
            Parents.Add(parent);
            IsLeaf.Add(isLeaf);
            Depths.Add(Depths[parent] + 1);
            HasMoved.Add(false);

            while (IndicesByLayer.Count <= Depths[newNodeIndex])
            {
                IndicesByLayer.Add(new List<int>());
            }
            IndicesByLayer[Depths[newNodeIndex]].Add(newNodeIndex);

            return newNodeIndex;
        }
    }
}
