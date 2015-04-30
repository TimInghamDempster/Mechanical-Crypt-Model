using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core;

namespace GameWorld
{
    public class CryptCC
    {
        IRenderer m_renderer;
        IRenderableScene m_scene;

        RenderArrays3d m_renderArrays;

        CellArrayCC m_cells;
        
        Random m_random;
        
        const float m_cryptRadius = 500.0f;
        const float m_cryptHeight = 3000.0f;
        const float m_betaCateninRequirement = 25.0f;
        const float m_separation = 1000.0f;
        const float m_betaCateninConsumptionPerTimestep = 0.5f;
        Colour[] m_baseColours;
        int[] m_colourCounts;

        public CryptCC(IRenderer renderer)
        {
            m_renderer = renderer;
            m_scene = m_renderer.GetNewScene();

            m_random = new Random();

            m_cells = new CellArrayCC();

            m_renderArrays = new RenderArrays3d();
            m_renderArrays.Positions = m_cells.Positions;
            m_renderArrays.Colours = m_cells.Colours;

            m_scene.CreateCamera();
            m_scene.SetCurrentCamera(0);

            m_scene.RenderArrays3d.Add(m_renderArrays);

            m_baseColours = new Colour[11];
            m_colourCounts = new int[11];

            m_baseColours[00] = new Colour() { R = 1.0f, G = 0.0f, B = 0.0f, A = 0.0f };
            m_baseColours[01] = new Colour() { R = 0.0f, G = 1.0f, B = 0.0f, A = 0.0f };
            m_baseColours[02] = new Colour() { R = 0.0f, G = 0.0f, B = 1.0f, A = 0.0f };
            m_baseColours[03] = new Colour() { R = 1.0f, G = 1.0f, B = 0.0f, A = 0.0f };
            m_baseColours[04] = new Colour() { R = 0.0f, G = 1.0f, B = 1.0f, A = 0.0f };
            m_baseColours[05] = new Colour() { R = 1.0f, G = 0.5f, B = 0.5f, A = 0.0f };
            m_baseColours[06] = new Colour() { R = 0.5f, G = 1.0f, B = 0.5f, A = 0.0f };
            m_baseColours[07] = new Colour() { R = 0.5f, G = 0.5f, B = 1.0f, A = 0.0f };
            m_baseColours[08] = new Colour() { R = 1.0f, G = 1.0f, B = 0.5f, A = 0.0f };
            m_baseColours[09] = new Colour() { R = 0.5f, G = 1.0f, B = 1.0f, A = 0.0f };
            m_baseColours[10] = new Colour() { R = 1.0f, G = 0.5f, B = 1.0f, A = 0.0f };

            m_cells.AddCell(new Vector3d(0.0f, -1.0f * m_cryptRadius, 0.0f), 0.0f, 100.0f, m_baseColours[0], 0);
            m_colourCounts[0]++;
            //m_cells.AddCell(new Vector3d(0.0f, 0.0f, 0.0f));

        }

        public void Tick()
        {
            UpdateWnt();
            DoGrowthPhase();
            DoCollisionAndMovement();
            DeleteTopCells();
            EnforceCryptWalls();
        }

        void DeleteTopCells()
        {
            for (int i = 0; i < m_cells.Positions.Count; i++)
            {
                Vector2d pos = new Vector2d(m_cells.Positions[i].X, m_cells.Positions[i].Z);
                if (pos.Length() >= m_cryptRadius * 2.0)
                {
                    m_colourCounts[m_cells.ColourIndices[i]]--;

                    int numActiveColours = 0;
                    foreach (int count in m_colourCounts)
                    {
                        if (count > 0)
                        {
                            numActiveColours++;
                        }
                    }

                    if (numActiveColours == 1)
                    {
                        m_colourCounts[m_cells.ColourIndices[0]] = 0;
                        for (int j = 0; j < m_cells.ColourIndices.Count; j++)
                        {
                            // If j == i we increment the colour count for the cell we
                            // are about to kill so we never get that colour back to 0
                            if (j != i)
                            {
                                int colourIndex = m_random.Next(11);
                                m_cells.ColourIndices[j] = colourIndex;
                                m_cells.Colours[j] = m_baseColours[colourIndex];
                                m_colourCounts[colourIndex]++;
                            }
                        }
                    }

                    m_cells.Remove(i);
                }
            }
        }

        void DoCollisionAndMovement()
        {
            for (int i = 0; i < m_cells.Positions.Count; i++)
            {
                for (int j = i; j < m_cells.Positions.Count; j++)
                {
                    var outerPos = m_cells.Positions[i];
                    var innerPos = m_cells.Positions[j];
                    var delta = outerPos - innerPos;
                    var separation = delta.Length();

                    if (separation < m_separation * 1.1f)
                    {
                        float restitution = m_separation - separation;
                        restitution /= 100.0f;
                        if (separation < 0.1f)
                        {
                            separation = 0.1f;
                            delta.X = (float)m_random.NextDouble() - 0.5f;
                            delta.Y = (float)m_random.NextDouble() - 0.5f;
                            delta.Z = (float)m_random.NextDouble() - 0.5f;
                        }

                        m_cells.Positions[i] += delta * restitution / separation;
                        m_cells.Positions[j] -= delta * restitution / separation;
                    }
                }
            }
        }

        void EnforceCryptWalls()
        {
            for (int i = 0; i < m_cells.Positions.Count; i++)
            {
                var pos = m_cells.Positions[i];
                //pos.Y += 20.0f;

                // guard against 0 length vector division
                if (pos.X == 0.0f && pos.Z == 0.0f)
                {
                    pos.X = 0.1f;
                }

                // Don't fall through the bottom of the crypt
                if (pos.Y < -1.0f * m_cryptRadius)
                {
                    pos.Y = -1.0f * m_cryptRadius + 10.0f;
                }

                if (pos.Y > m_cryptHeight - m_cryptRadius)
                {
                    float apatureRadius = m_cryptRadius * 2;
                    
                    Vector2d virtualSphereDirection = new Vector2d(pos.X, pos.Z);
                    virtualSphereDirection /= virtualSphereDirection.Length();
                    virtualSphereDirection *= apatureRadius;

                    Vector3d virtualSpherePosition = new Vector3d(virtualSphereDirection.X, m_cryptHeight - m_cryptRadius, virtualSphereDirection.Y);

                    Vector3d sphereRelativeCellPosition = pos - virtualSpherePosition;
                    sphereRelativeCellPosition /= sphereRelativeCellPosition.Length();
                    sphereRelativeCellPosition *= apatureRadius - m_cryptRadius;

                    pos = virtualSpherePosition + sphereRelativeCellPosition;
                }
                else if (pos.Y > 0.0f)
                {
                    Vector2d final;
                    Vector2d pos2d = new Vector2d(pos.X, pos.Z);
                    Vector2d normalised = pos2d / pos2d.Length();
                    final = normalised * m_cryptRadius;

                    pos.X = final.X;
                    pos.Z = final.Y;
                }
                else
                {
                    Vector3d normalised = pos / pos.Length();
                    pos = normalised * m_cryptRadius;
                }
                
                m_cells.Positions[i] = pos;
            }
        }

        void DoGrowthPhase()
        {
            for (int i = 0; i < m_cells.CycleStages.Count; i++)
            {
                if (m_cells.CycleStages[i] == CellCycleStage.G)
                {
                    m_cells.GrowthStageCurrentTimes[i]++;

                    if (m_cells.GrowthStageCurrentTimes[i] > m_cells.GrowthStageRequiredTimes[i])
                    {
                        m_cells.GrowthStageCurrentTimes[i] = 0.0f;
                        m_cells.CycleStages[i] = CellCycleStage.G0;
                        m_cells.BetaCatenin[i] = 0.0f;

                        Vector3d newPos = m_cells.Positions[i];
                        newPos.X += 5.0f - ((float)m_random.NextDouble() * 10.0f);
                        newPos.Y += 5.0f - ((float)m_random.NextDouble() * 10.0f);
                        newPos.Z += 5.0f - ((float)m_random.NextDouble() * 10.0f);

                        if (newPos.Y < -1.0f * m_cryptRadius)
                        {
                            newPos.Y = -1.0f * m_cryptRadius + 0.1f;
                        }

                        m_cells.Colours[i] = m_baseColours[m_cells.ColourIndices[i]];

                        m_cells.AddCell(newPos, m_cells.BetaCatenin[i], 50.0f + ((float)m_random.NextDouble() * 50.0f), m_cells.Colours[i], m_cells.ColourIndices[i]);
                        m_colourCounts[m_cells.ColourIndices[i]]++;
                    }
                }
            }
        }

        void UpdateWnt()
        {
            for (int i = 0; i < m_cells.Positions.Count; i++)
            {
                float yPos = m_cells.Positions[i].Y;
                float height = yPos + m_cryptRadius;
                float wntAmount = (m_cryptHeight - height) / m_cryptHeight;

                m_cells.BetaCatenin[i] += wntAmount - m_betaCateninConsumptionPerTimestep;

                if (m_cells.BetaCatenin[i] < 0.0f)
                {
                    m_cells.BetaCatenin[i] = 0.0f;
                }

                if (m_cells.BetaCatenin[i] > m_betaCateninRequirement)
                {
                    m_cells.CycleStages[i] = CellCycleStage.G;
                    m_cells.Colours[i] = new Colour() { A = 1.0f, R = 1.0f, B = 1.0f, G = 0.0f };
                }
            }
        }
    }
}
