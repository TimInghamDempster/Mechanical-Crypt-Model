using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Core;

namespace GameWorld
{
    public class World
    {
        IRenderer m_renderer;
        IRenderableScene m_scene;

        RenderArrays m_bBoxRenderArrays;
        RenderArrays m_renderArrays;
        RenderArrays m_springRenderArrays;
        RenderArrays m_basisRenderArrays;
        BlobArrays m_blobs;
        DirectionalSpringArray m_directionalSprings;
        SpringArray m_springs;
        BBoxTree m_BBoxTree;
      
        List<Tuple<int, int>> m_collisions;
        CellDataArrays m_cellData;

        Random m_random;

        Colour[] m_baseColours;


        public World(IRenderer renderer)
        {
            m_baseColours = new Colour[12];

            m_baseColours[00] = new Colour() { R = 1.0f, G = 0.0f, B = 0.0f, A = 0.0f };
            m_baseColours[01] = new Colour() { R = 0.0f, G = 1.0f, B = 0.0f, A = 0.0f };
            m_baseColours[02] = new Colour() { R = 0.0f, G = 0.0f, B = 1.0f, A = 0.0f };
            m_baseColours[03] = new Colour() { R = 1.0f, G = 1.0f, B = 0.0f, A = 0.0f };
            m_baseColours[04] = new Colour() { R = 0.0f, G = 1.0f, B = 1.0f, A = 0.0f };
            m_baseColours[05] = new Colour() { R = 1.0f, G = 0.0f, B = 1.0f, A = 0.0f };
            m_baseColours[06] = new Colour() { R = 1.0f, G = 0.5f, B = 0.5f, A = 0.0f };
            m_baseColours[07] = new Colour() { R = 0.5f, G = 1.0f, B = 0.5f, A = 0.0f };
            m_baseColours[08] = new Colour() { R = 0.5f, G = 0.5f, B = 1.0f, A = 0.0f };
            m_baseColours[09] = new Colour() { R = 1.0f, G = 1.0f, B = 0.5f, A = 0.0f };
            m_baseColours[10] = new Colour() { R = 0.5f, G = 1.0f, B = 1.0f, A = 0.0f };
            m_baseColours[11] = new Colour() { R = 1.0f, G = 0.5f, B = 1.0f, A = 0.0f };

            m_random = new Random();

            m_renderer = renderer;

            m_scene = m_renderer.GetNewScene();

            m_scene.CreateCamera();
            m_scene.SetCurrentCamera(0);

            m_basisRenderArrays = new RenderArrays();
            m_springRenderArrays = new RenderArrays();
            m_bBoxRenderArrays = new RenderArrays();
            m_renderArrays = new RenderArrays();

            m_blobs = new BlobArrays();
            m_directionalSprings = new DirectionalSpringArray();
            m_collisions = new List<Tuple<int, int>>();
            m_cellData = new CellDataArrays();
            m_BBoxTree = new BBoxTree();
            m_springs = new SpringArray();

            m_renderArrays.Positions = m_blobs.Positions;
            m_renderArrays.Colours = m_blobs.Colours;

            m_bBoxRenderArrays.Colours.Add(m_baseColours[0]);
            m_bBoxRenderArrays.Positions.Add(new Vector2d());

            //InitVisualisationTest();

            for (int i = -0; i < 1; i++)
            {
                bool grow = i == 0 ? true : false;
                InitCell(new Vector2d() { X = (float)(250.0f * i + 150.0f), Y = 0 }, i + 4, grow);
            }
            //InitCell(new Vector2d() { X = -100.0f, Y = 0.0f }, 0, true);

            m_scene.RenderArrays.Add(m_renderArrays);
            //m_scene.RenderArrays.Add(m_bBoxRenderArrays);
            m_scene.RenderArrays.Add(m_springRenderArrays);
            m_scene.RenderArrays.Add(m_basisRenderArrays);
        }

        

        void InitCell(Vector2d position, int cellIndex, bool isGrowing)
        {
            int cellId = m_cellData.CurrentSeparationMultiplier.Count;
            m_cellData.CurrentSeparationMultiplier.Add(1.0f);
            m_cellData.OriginalSurfaceArea.Add(CellDataArrays.IntraCellBlobSeparation);
            m_cellData.BlobIndices.Add(m_blobs.Positions.Count);
            m_cellData.SpringIndices.Add(m_springs.EndPointIndices.Count);
            m_cellData.IsGrowing.Add(isGrowing);

            int cellBBoxId = m_BBoxTree.AddNode(0, false);
            List<int> subCellBBIds = new List<int>(8);

            /*for (int i = 0; i < 8; i++)
            {
                subCellBBIds.Add(m_BBoxTree.AddNode(cellBBoxId, true));
            }*/

            var colour = m_baseColours[m_random.Next(12)];

            uint rootBlobIndex = (uint)m_blobs.Positions.Count;

            for (uint theta = 0; theta < CellDataArrays.BoundaryElementsInCell; theta++)
            {
                float fTheta = (float)Math.PI * 2.0f * (float)theta / (float)CellDataArrays.BoundaryElementsInCell;
                Vector2d pos = position;
                pos += new Vector2d((float)Math.Cos(fTheta), (float)Math.Sin(fTheta)) * CellDataArrays.CellIdealRadii;
             
                int blobId = m_blobs.CellIds.Count;
                m_blobs.AddBlob(
                    pos,
                    colour,
                    new Vector2d(),
                    CellDataArrays.IntraCellBlobSeparation,
                    cellId,
                    true,
                    -1);
                
                if(theta != 0)
                {
                    AddSpring(theta, theta - 1, 0.0001f, 2.0f);
                }
            }
            AddSpring(rootBlobIndex, rootBlobIndex + CellDataArrays.BoundaryElementsInCell - 1, 0.0001f, 2.0f);
        }

        void AddSpring(uint startIndex, uint endIndex, float stiffness, float restLength)
        {
            m_springs.StartPointIndices.Add(startIndex);
            m_springs.EndPointIndices.Add(endIndex);
            m_springs.RestLengths.Add(restLength);
            m_springs.Stiffnesses.Add(stiffness);
        }

        void AddDirectionalSpring(uint endIndex, uint startIndex, float stiffness, Vector2d restVector)
        {
            m_directionalSprings.EndPointIndices.Add(endIndex);
            m_directionalSprings.StartPointIndices.Add(startIndex);
            m_directionalSprings.Stiffnesses.Add(stiffness);
            restVector.X = Math.Abs(restVector.X);
            restVector.Y = Math.Abs(restVector.Y);
            m_directionalSprings.RestVectors.Add(restVector);
            m_directionalSprings.OriginalRestVectors.Add(restVector);
        }

        void InitVisualisationTest()
        {
            for (uint i = 0; i < 100; i++)
            {
                m_blobs.AddBlob(new Vector2d() { X = 1000.0f * (float)(m_random.NextDouble() - 0.5), Y = 1000.0f * (float)(m_random.NextDouble() - 0.5) },
                    m_baseColours[m_random.Next(12)],
                    new Vector2d() { X = 10.0f * (float)(m_random.NextDouble() - 0.5), Y = 10.0f * (float)(m_random.NextDouble() - 0.5) },
                    50.0f,
                    (int)i,
                    false,
                    0);
            }
        }

        public void Tick()
        {
            for (int i = 0; i < 100; i++)
            {
                KillMomentum();
                //GrowAndDivideCells();
                //m_BBoxTree.UpdateOverlapping(m_blobs.Positions, m_blobs.InteractionRadii, m_bBoxRenderArrays);
                //DoCollisionCellBB();
                DoCollisionBruteForce();
                m_springs.UpdateSprings(m_springRenderArrays, m_baseColours[1], m_blobs.Positions, m_blobs.Velocities);
                m_cellData.UpdateAreaSprings(m_blobs);
                UpdatePositions();
                
                if (m_cellData.BlobIndices.Count == 10)
                {
                    int a = 0;
                } if (m_cellData.BlobIndices.Count == 20)
                {
                    int a = 0;
                }
                if (m_cellData.BlobIndices.Count == 50)
                {
                    int a = 0;
                }
            }
            //DoEnergeticEvents();
        }

        void KillMomentum()
        {
            for (int i = 0; i < m_blobs.Velocities.Count; i++)
            {
                m_blobs.Velocities[i] *= 0.0f;
            }
        }

        void GrowAndDivideCells()
        {
            for (int i = 0; i < m_cellData.IsGrowing.Count; i++)
            {
                if (m_cellData.IsGrowing[i] == true)
                {
                    if (m_cellData.CurrentSeparationMultiplier[i] < 2.0)
                    {
                        m_cellData.CurrentSeparationMultiplier[i] += 0.00005f;
                        int startSpring = m_cellData.SpringIndices[i];
                        
                        for (int currentSpring = startSpring; currentSpring < startSpring + 52; currentSpring++)
                        {
                            m_directionalSprings.RestVectors[currentSpring] = new Vector2d() { X = m_directionalSprings.OriginalRestVectors[currentSpring].X * m_cellData.CurrentSeparationMultiplier[i], Y = m_directionalSprings.OriginalRestVectors[currentSpring].Y };
                        }
                    }
                    else
                    {
                        m_cellData.CurrentSeparationMultiplier[i] = 1.0f;
                        //m_cellData.IsGrowing[i] = false;
                        int startSpring = m_cellData.SpringIndices[i];
                        for (int currentSpring = startSpring; currentSpring < startSpring + 52; currentSpring++)
                        {
                            m_directionalSprings.RestVectors[currentSpring] = m_directionalSprings.OriginalRestVectors[currentSpring];
                        }

                        int newCell = DuplicateCell(i);

                        int startBlob = m_cellData.BlobIndices[i];
                        for (int y = 0; y < 8; y++)
                        {
                            int firstBlobInRow = startBlob + y * 4;
                            Vector2d rowLeftPosition = m_blobs.Positions[firstBlobInRow];
                            Vector2d delta = m_blobs.Positions[firstBlobInRow + 1] - rowLeftPosition;
                            m_blobs.Positions[firstBlobInRow + 1] = rowLeftPosition + (delta / 3.0f);
                            m_blobs.Positions[firstBlobInRow +2] = rowLeftPosition + (delta * 2.0f / 3.0f);
                            m_blobs.Positions[firstBlobInRow+3] = rowLeftPosition +  delta;
                        }

                        startBlob = m_cellData.BlobIndices[newCell];
                        for (int y = 0; y < 8; y++)
                        {
                            int firstBlobInRow = startBlob + y * 4;
                            Vector2d rowLeftPosition = m_blobs.Positions[firstBlobInRow + 2];
                            Vector2d delta = m_blobs.Positions[firstBlobInRow + 3] - rowLeftPosition;
                            m_blobs.Positions[firstBlobInRow + 0] = rowLeftPosition;
                            m_blobs.Positions[firstBlobInRow + 1] = rowLeftPosition + (delta / 3.0f);
                            m_blobs.Positions[firstBlobInRow + 2] = rowLeftPosition + (delta * 2.0f / 3.0f);
                        }
                    }
                }
            }
        }

        int DuplicateCell(int cellIndex)
        {
            int newCellIndex = m_cellData.BlobIndices.Count;
            int startBlobIndex = m_cellData.BlobIndices[cellIndex];
            int newCellStartBlobIndex = m_blobs.Positions.Count;
            m_cellData.BlobIndices.Add(newCellStartBlobIndex);
            m_cellData.CurrentSeparationMultiplier.Add(1.0f);
            m_cellData.OriginalSurfaceArea.Add(CellDataArrays.IntraCellBlobSeparation);
            m_cellData.IsGrowing.Add(false);
            m_cellData.SpringIndices.Add(m_directionalSprings.StartPointIndices.Count);

            int cellBBoxId = m_BBoxTree.AddNode(0, false);
            int bboxDelta = m_BBoxTree.Parents.Count - m_BBoxTree.Parents[m_blobs.BBoxIndices[startBlobIndex]] - 1;
            for (int i = 0; i < 8; i++)
            {
                m_BBoxTree.AddNode(cellBBoxId, true);
            }

            int blobDelta = newCellStartBlobIndex - startBlobIndex;

            Colour colour = m_baseColours[m_random.Next(m_baseColours.Count())];

            for (int i = 0; i < 32; i++)
            {
                int bboxIndex = m_blobs.BBoxIndices[startBlobIndex + i] + bboxDelta;

                m_BBoxTree.AddChildIndexToNode(bboxIndex, m_blobs.Positions.Count);

                m_blobs.AddBlob(m_blobs.Positions[startBlobIndex + i],
                    colour,
                    new Vector2d(),
                    CellDataArrays.IntraCellBlobSeparation,
                    newCellIndex,
                    m_blobs.CollideableBlobs[startBlobIndex + i],
                    bboxIndex);
            }

            int existingSpringIndex = m_cellData.SpringIndices[cellIndex];
            for (int i = 0; i < 52; i++)
            {
                m_directionalSprings.RestVectors.Add(m_directionalSprings.RestVectors[existingSpringIndex + i]);
                m_directionalSprings.Stiffnesses.Add(m_directionalSprings.Stiffnesses[existingSpringIndex + i]);
                m_directionalSprings.EndPointIndices.Add((uint)(m_directionalSprings.EndPointIndices[existingSpringIndex + i] + blobDelta));
                m_directionalSprings.StartPointIndices.Add((uint)(m_directionalSprings.StartPointIndices[existingSpringIndex + i] + blobDelta));
            }
            
            return newCellIndex;
        }

        void DoContinuingCollision(int firstIndex, int secondIndex)
        {
            Vector2d pos1 = m_blobs.Positions[firstIndex];
            Vector2d pos2 = m_blobs.Positions[secondIndex];
            Vector2d delta = pos1 - pos2;
            float desiredSeparation = (m_blobs.InteractionRadii[firstIndex] + m_blobs.InteractionRadii[secondIndex]) * 0.9f;
            float actualSeparation = (float)Math.Sqrt(Vector2d.DotProduct(delta, delta));
            if (actualSeparation < desiredSeparation)
            {
                float deltaSeparation = (desiredSeparation - actualSeparation) / 2.0f;
                Vector2d normalisedDelta = delta / actualSeparation;

                m_blobs.Positions[firstIndex] += normalisedDelta * deltaSeparation;
                m_blobs.Positions[secondIndex] -= normalisedDelta * deltaSeparation;
            }
        }

        void StartCollision(int firstIndex, int secondIndex)
        {
          //  m_blobs.Colours[firstIndex] = m_baseColours[1];
           // m_blobs.Colours[secondIndex] = m_baseColours[1];
            m_blobs.CollisionCount[firstIndex]++;
            m_blobs.CollisionCount[secondIndex]++;
            var delta = (m_blobs.Positions[firstIndex] - m_blobs.Positions[secondIndex]) *0.9f;
            AddSpring((uint)firstIndex, (uint)secondIndex, 0.01f, delta.Length());
        }

        void EndCollision(int firstIndex, int secondIndex)
        {
            for (int i = 0; i < m_springs.StartPointIndices.Count; i++)
            {
                if (m_springs.StartPointIndices[i] == firstIndex)
                {
                    if (m_springs.EndPointIndices[i] == secondIndex)
                    {
                        m_springs.StartPointIndices.RemoveAt(i);
                        m_springs.EndPointIndices.RemoveAt(i);
                        m_springs.Stiffnesses.RemoveAt(i);
                        m_springs.RestLengths.RemoveAt(i);
                    }
                }
            }
            m_blobs.CollisionCount[firstIndex]--;
            m_blobs.CollisionCount[secondIndex]--;

            if (m_blobs.CollisionCount[firstIndex] == 0)
            {
                //m_blobs.Colours[firstIndex] = m_baseColours[2];
            }
            if (m_blobs.CollisionCount[secondIndex] == 0)
            {
                // m_blobs.Colours[secondIndex] = m_baseColours[2];
            }
        }

        void DoCollisionCellBB()
        {
#if DEBUG
            int d_blobTests = 0;
            int d_actualCollisions = 0;
#endif

            foreach (var collisionSet in m_blobs.CurrentCollisions)
            {
                collisionSet.Clear();
            }

            foreach (int bBoxIndex in m_BBoxTree.IndicesByLayer[m_BBoxTree.IndicesByLayer.Count - 1])
            {
                foreach (var overlappingBBox in m_BBoxTree.OverlappingBBoxIndices[bBoxIndex])
                {
                    foreach (int blobIndex in m_BBoxTree.Children[bBoxIndex])
                    {
                        foreach (int otherBlob in m_BBoxTree.Children[overlappingBBox])
                        {
                            Vector2d delta = m_blobs.Positions[blobIndex] - m_blobs.Positions[otherBlob];
                            float distSquared = Vector2d.DotProduct(delta, delta);
                            float intRadiusSq = m_blobs.InteractionRadii[blobIndex] + m_blobs.InteractionRadii[otherBlob];
                            intRadiusSq *= intRadiusSq;
                            if (distSquared < intRadiusSq)
                            {
#if DEBUG
                                d_actualCollisions++;
#endif
                                int blobTrackingThisCollision = Math.Min(blobIndex, otherBlob);
                                int blobNotTrackingCollision = Math.Max(blobIndex, otherBlob);
                                m_blobs.CurrentCollisions[blobTrackingThisCollision].Add(blobNotTrackingCollision);

                                if (m_blobs.PersistentCollisions[blobTrackingThisCollision].Contains(blobNotTrackingCollision) == false)
                                {
                                    m_blobs.PersistentCollisions[blobTrackingThisCollision].Add(blobNotTrackingCollision);
                                    StartCollision(blobIndex, otherBlob);
                                }
                                DoContinuingCollision(blobIndex, otherBlob);
                            }
#if DEBUG
                            d_blobTests++;
#endif
                        }
                    }
                }
            }
            for (int i = 0; i < m_blobs.PersistentCollisions.Count; i++)
            {
                List<int> toRemove = new List<int>();
                foreach (var otherBlob in m_blobs.PersistentCollisions[i])
                {
                    if (m_blobs.CurrentCollisions[i].Contains(otherBlob) == false)
                    {
                        toRemove.Add(otherBlob);
                        EndCollision(i, otherBlob);
                    }
                }
                foreach (int blob in toRemove)
                {
                    m_blobs.PersistentCollisions[i].Remove(blob);
                }
            }
        }

        // Leave in as reference implementation
        void DoCollisionBruteForce()
        {
            var newCollisionList = new List<Tuple<int, int>>(m_collisions.Count);
            var collsionsToRemove = new List<Tuple<int, int>>();
            var collsionsToAdd = new List<Tuple<int, int>>();

            int numChecks = 0;
            for (int outer = 0; outer < m_blobs.Positions.Count; outer++)
            {
                if (m_blobs.CollideableBlobs[outer] == false)
                {
                   // continue;
                }
                for (int inner = outer + 1; inner < m_blobs.Positions.Count; inner++)
                {
                    numChecks++;
                    if (m_blobs.CellIds[inner] != m_blobs.CellIds[outer])// && m_collideableBlobs[inner] == true)
                    {
                        Vector2d delta = m_blobs.Positions[inner] - m_blobs.Positions[outer];
                        float distSquared = Vector2d.DotProduct(delta, delta);
                        float intRadiusSq = m_blobs.InteractionRadii[outer] + m_blobs.InteractionRadii[inner];
                        intRadiusSq *= intRadiusSq;
                        if (distSquared < intRadiusSq)
                        {
                            newCollisionList.Add(new Tuple<int, int>(inner, outer));
                        }
                    }
                }
            }


            foreach (var newCollision in newCollisionList)
            {
                bool found = false;
                foreach (var oldCollision in m_collisions)
                {
                    if (oldCollision.Item1 == newCollision.Item1 && oldCollision.Item2 == newCollision.Item2)
                    {
                        found = true;
                    }
                }
                if (found == true)
                {
                    DoContinuingCollision(newCollision.Item1, newCollision.Item2);
                }
                else
                {
                    collsionsToAdd.Add(newCollision);
                    StartCollision(newCollision.Item1, newCollision.Item2);
                }
            }
            foreach (var collisionToAdd in collsionsToAdd)
            {
                m_collisions.Add(collisionToAdd);
            }

            var removeIndices = new List<int>();
            for (int index = 0; index < m_collisions.Count; index++)
            {
                var oldCollision = m_collisions[index];
                bool found = false;
                foreach (var newCollision in newCollisionList)
                {
                    if (newCollision.Item1 == oldCollision.Item1 && newCollision.Item2 == oldCollision.Item2)
                    {
                        found = true;
                    }
                }
                if (found == false)
                {
                    EndCollision(oldCollision.Item1, oldCollision.Item2);
                    removeIndices.Add(index);
                }
            }
            int totalRemoved = 0;
            foreach(var index in removeIndices)
            {
                m_collisions.RemoveAt(index - totalRemoved);
                totalRemoved++;
            }
        } 

        void UpdatePositions()
        {
            for (uint i = 0; i < m_blobs.Positions.Count; i++)
            {
                Vector2d pos = m_blobs.Positions[(int)i];
                Vector2d vel = m_blobs.Velocities[(int)i];

                /*if (vel.Length() > 1.0f)
                {
                    vel /= vel.Length();
                    vel *= 1.0f;
                    m_blobs.Velocities[(int)i] = vel;
                }*/

                pos += m_blobs.Velocities[(int)i];
                m_blobs.Positions[(int)i] = pos;
                if (pos.X > 1024.0f)
                {
                    m_blobs.Positions[(int)i] = new Vector2d() { X = 1024.0f, Y = pos.Y };
                }
                if (pos.X < -1024.0f)
                {
                    m_blobs.Positions[(int)i] = new Vector2d() { X = -1024.0f, Y = pos.Y };
                }
                if (pos.Y > 768.0f)
                {
                    m_blobs.Positions[(int)i] = new Vector2d() { X = pos.X, Y = 768.0f };
                }
                if (pos.Y < -768.0f)
                {
                    m_blobs.Positions[(int)i] = new Vector2d() { X = pos.X, Y = -768.0f };
                }
            }
        }
    }
}
