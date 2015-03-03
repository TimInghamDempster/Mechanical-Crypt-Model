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
        const float m_wntRequirement = 100.0f;
        const float m_separation = 1000.0f;

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

            m_cells.AddCell(new Vector3d(0.0f, -1.0f * m_cryptRadius, 0.0f), 0.0f, 100.0f);
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
                var pos = m_cells.Positions[i];
                if (pos.Y > m_cryptHeight - m_cryptRadius)
                {
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

                    if (separation < m_separation)
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

                if (pos.Y > 0.0f)
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
                        m_cells.Wnt[i] /= 2.0f;

                        Vector3d newPos = m_cells.Positions[i];
                        newPos.X += 5.0f - ((float)m_random.NextDouble() * 10.0f);
                        newPos.Y += 5.0f - ((float)m_random.NextDouble() * 10.0f);
                        newPos.Z += 5.0f - ((float)m_random.NextDouble() * 10.0f);

                        if (newPos.Y < -1.0f * m_cryptRadius)
                        {
                            newPos.Y = -1.0f * m_cryptRadius + 0.1f;
                        }

                        m_cells.AddCell(newPos, m_cells.Wnt[i], 50.0f + ((float)m_random.NextDouble() * 50.0f));
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

                m_cells.Wnt[i] += wntAmount;

                if (m_cells.Wnt[i] > m_wntRequirement)
                {
                    m_cells.CycleStages[i] = CellCycleStage.G;
                }
            }
        }
    }
}
