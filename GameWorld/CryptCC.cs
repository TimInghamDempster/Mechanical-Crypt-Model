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
        CryptArrayCC m_crypts;
        
        Random m_random;
        
        const float m_cryptRadius = 500.0f;
        const float m_cryptHeight = 3000.0f;
        const float m_flutingRadius = 500.0f;
        const float m_betaCateninRequirement = 20.0f;
        public const float m_separation = 500.0f;
        const float m_betaCateninConsumptionPerTimestep = 0.5f;
        const float m_anoikisProbabilityPerTimestep = 0.002f;
        const float m_membraneSeparationToTriggerAnoikis = 10.0f;
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

            m_cells.AddCell(new Vector3d(1000.0f, -1.0f * m_cryptHeight, 1000.0f), 0.0f, 100.0f, m_baseColours[0], 0, 0, m_separation, CellCycleStage.G0);
            m_cells.AddCell(new Vector3d(1000.0f, -1.0f * m_cryptHeight, -1000.0f), 0.0f, 100.0f, m_baseColours[1], 1, 1, m_separation, CellCycleStage.G0);
            m_cells.AddCell(new Vector3d(-1000.0f, -1.0f * m_cryptHeight, 1000.0f), 0.0f, 100.0f, m_baseColours[2], 2, 2, m_separation, CellCycleStage.G0);
            m_cells.AddCell(new Vector3d(-1000.0f, -1.0f * m_cryptHeight, -1000.0f), 0.0f, 100.0f, m_baseColours[3], 3, 3, m_separation, CellCycleStage.G0);
            m_colourCounts[0]++;
            m_colourCounts[1]++;
            m_colourCounts[2]++;
            m_colourCounts[3]++;

            m_crypts = new CryptArrayCC();
            m_crypts.Add(new Vector3d(1000, 0, 1000));
            m_crypts.Add(new Vector3d(1000, 0, -1000));
            m_crypts.Add(new Vector3d(-1000, 0, 1000));
            m_crypts.Add(new Vector3d(-1000, 0, -1000));
        }

        public void Tick()
        {
            m_crypts.PreTick();
            UpdateWnt();
            DoGrowthPhase();
            DoCollisionAndMovement();
            DoAnoikis();
            EnforceCryptWalls();
        }

        void DoAnoikis()
        {
            for (int i = 0; i < m_cells.Positions.Count; i++)
            {
                if (m_cells.Active[i])
                {
                    if (m_cells.OffMembraneDistance[i] > m_membraneSeparationToTriggerAnoikis)
                    {
                        m_cells.Remove(i);
                    }
                    // Fixed probability rule.
                    /*Vector3d pos = m_cells.Positions[i];

                    if (pos.Y > -1.0f * m_flutingRadius)
                    {
                        if (m_random.NextDouble() < m_anoikisProbabilityPerTimestep)
                        {
                            m_cells.Remove(i);
                            i--;
                        }
                    }*/
                }
            }
        }

        void DeleteTopCells()
        {
            for (int i = 0; i < m_cells.Positions.Count; i++)
            {
                if (m_cells.Active[i])
                {
                    Vector2d pos = new Vector2d(m_cells.Positions[i].X, m_cells.Positions[i].Z);
                    if (pos.Length() >= m_cryptRadius + m_flutingRadius)
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
        }

        void DoCollisionAndMovement()
        {
            for (int i = 0; i < m_cells.Positions.Count; i++)
            {
                if (m_cells.Active[i])
                {
                    for (int j = i + 1; j < m_cells.Positions.Count; j++)
                    {
                        if (m_cells.Active[j])
                        {
                            var outerPos = m_cells.Positions[i];
                            var innerPos = m_cells.Positions[j];
                            var delta = outerPos - innerPos;
                            var separation = delta.Length();


                            float targetSeparation = m_cells.Radii[i] + m_cells.Radii[j];

                            if (j == m_cells.ChildPointIndices[i])
                            {
                                float growthFactor = m_cells.GrowthStageCurrentTimes[i] / m_cells.GrowthStageRequiredTimes[i];
                                targetSeparation *= growthFactor;
                            }
                            else if (i == m_cells.ChildPointIndices[j])
                            {
                                float growthFactor = m_cells.GrowthStageCurrentTimes[j] / m_cells.GrowthStageRequiredTimes[j];
                                targetSeparation *= growthFactor;
                            }

                            if (separation < targetSeparation)
                            {
                                float restitution = targetSeparation - separation;
                                restitution /= 100.0f;
                                if (separation < 0.1f)
                                {
                                    separation = 0.1f;
                                    delta.X = (float)m_random.NextDouble() - 0.5f;
                                    delta.Y = (float)m_random.NextDouble() - 0.5f;
                                    delta.Z = (float)m_random.NextDouble() - 0.5f;
                                }

                                int cryptId1 = (int)m_cells.CryptIds[i];
                                int cryptId2 = (int)m_cells.CryptIds[j];

                                Vector3d mCryptPos1 = m_crypts.m_cryptPositions[cryptId1];
                                Vector3d mCryptPos2 = m_crypts.m_cryptPositions[cryptId2];

                                Vector3d force = delta * restitution / separation;
                                Vector3d cryptForce = force;
                                cryptForce.Y = 0.0f;

                                m_crypts.m_cellularity[cryptId1]++;
                                m_crypts.m_cellularity[cryptId2]++;

                                if ((mCryptPos1 - outerPos).Length() < m_cryptRadius + m_flutingRadius)
                                {
                                    m_crypts.m_forces[cryptId1] += cryptForce;
                                }

                                if ((mCryptPos2 - innerPos).Length() < m_cryptRadius + m_flutingRadius)
                                {
                                    m_crypts.m_forces[cryptId2] -= cryptForce;
                                }

                                m_cells.Positions[i] += force;
                                m_cells.Positions[j] -= force;
                            }
                        }
                    }
                }
            }
        }

        void EnforceCryptWalls()
        {
            for (int i = 0; i < m_cells.Positions.Count; i++)
            {
                if (m_cells.Active[i])
                {
                    int cryptId = (int)m_cells.CryptIds[i];
                    var pos = m_cells.Positions[i];
                    pos -= m_crypts.m_cryptPositions[cryptId];

                    bool isAboveBasementMembrane = false;

                    // guard against 0 length vector division
                    if (pos.X == 0.0f && pos.Z == 0.0f)
                    {
                        pos.X = 0.1f;
                    }

                    // Don't fall through the bottom of the crypt
                    if (pos.Y < -1.0f * m_cryptHeight)
                    {
                        pos.Y = -1.0f * m_cryptHeight + 10.0f;
                    }

                    Vector2d pos2d = new Vector2d(pos.X, pos.Z);

                    if (pos2d.Length() > m_cryptRadius + m_flutingRadius)
                    {
                        if (pos.Y > 0.0f)
                        {
                            isAboveBasementMembrane = true;
                        }
                        pos.Y = 0.0f;
                    }
                    else if (pos.Y > m_flutingRadius * -1.0f)
                    {
                        Vector2d virtualSphereDirection = new Vector2d(pos.X, pos.Z);
                        virtualSphereDirection /= virtualSphereDirection.Length();
                        virtualSphereDirection *= m_cryptRadius + m_flutingRadius;

                        Vector3d virtualSpherePosition = new Vector3d(virtualSphereDirection.X, m_flutingRadius * -1.0f, virtualSphereDirection.Y);

                        Vector3d sphereRelativeCellPosition = pos - virtualSpherePosition;
                        isAboveBasementMembrane = sphereRelativeCellPosition.Length() > m_flutingRadius;
                        sphereRelativeCellPosition /= sphereRelativeCellPosition.Length();
                        sphereRelativeCellPosition *= m_flutingRadius;

                        pos = virtualSpherePosition + sphereRelativeCellPosition;
                    }
                    else if (pos.Y > (m_cryptHeight - m_cryptRadius) * -1.0f)
                    {
                        Vector2d final;
                        Vector2d normalised = pos2d / pos2d.Length();
                        final = normalised * m_cryptRadius;

                        isAboveBasementMembrane = pos2d.Length() < m_cryptRadius;

                        pos.X = final.X;
                        pos.Z = final.Y;
                    }
                    else
                    {
                        Vector3d nicheCentre = new Vector3d(0.0f, (m_cryptHeight - m_cryptRadius) * -1.0f, 0.0f);
                        Vector3d positionRelativeToNicheCentre = pos - nicheCentre;
                        isAboveBasementMembrane = positionRelativeToNicheCentre.Length() < m_cryptRadius;
                        positionRelativeToNicheCentre = positionRelativeToNicheCentre / positionRelativeToNicheCentre.Length();
                        pos = positionRelativeToNicheCentre * m_cryptRadius + nicheCentre;
                    }

                    pos += m_crypts.m_cryptPositions[cryptId];
                    Vector3d delta = m_cells.Positions[i] - pos;

                    m_cells.OffMembraneDistance[i] = delta.Length();

                    if (isAboveBasementMembrane == false)
                    {
                        m_cells.OffMembraneDistance[i] *= -1.0f;
                    }

                    m_cells.Positions[i] = pos;
                }
            }
        }

        void DoGrowthPhase()
        {
            for (int i = 0; i < m_cells.CycleStages.Count; i++)
            {
                if (m_cells.Active[i])
                {
                    if (m_cells.CycleStages[i] == CellCycleStage.G)
                    {
                        m_cells.GrowthStageCurrentTimes[i]++;

                        if (m_cells.GrowthStageCurrentTimes[i] > m_cells.GrowthStageRequiredTimes[i])
                        {
                            m_cells.GrowthStageCurrentTimes[i] = 0.0f;
                            m_cells.CycleStages[i] = CellCycleStage.G0;
                            m_cells.BetaCatenin[i] = 0.0f;
                            m_cells.Colours[i] = m_baseColours[m_cells.ColourIndices[i]];

                            int childIndex = m_cells.ChildPointIndices[i];
                            m_cells.GrowthStageCurrentTimes[childIndex] = 0.0f;
                            m_cells.CycleStages[childIndex] = CellCycleStage.G0;
                            m_cells.BetaCatenin[childIndex] = 0.0f;
                            m_cells.Colours[childIndex] = m_baseColours[m_cells.ColourIndices[childIndex]];

                            m_cells.ChildPointIndices[i] = -1;
                        }
                    }
                }
            }
        }

        void UpdateWnt()
        {
            for (int i = 0; i < m_cells.Positions.Count; i++)
            {
                if (m_cells.Active[i] && m_cells.CycleStages[i] == CellCycleStage.G0)
                {
                    float height = m_cells.Positions[i].Y + m_cryptHeight;
                    float wntAmount = 1.0f - height / m_cryptHeight;

                    m_cells.BetaCatenin[i] += wntAmount - m_betaCateninConsumptionPerTimestep;

                    if (m_cells.BetaCatenin[i] < 0.0f)
                    {
                        m_cells.BetaCatenin[i] = 0.0f;
                    }

                    if (m_cells.BetaCatenin[i] > m_betaCateninRequirement)
                    {
                        m_cells.CycleStages[i] = CellCycleStage.G;
                        m_cells.Colours[i] = new Colour() { A = 1.0f, R = 1.0f, G = 0.0f, B = 1.0f };

                        Vector3d newPos = m_cells.Positions[i];
                        newPos.X += 5.0f - ((float)m_random.NextDouble() * 10.0f);
                        newPos.Y += 5.0f - ((float)m_random.NextDouble() * 10.0f);
                        newPos.Z += 5.0f - ((float)m_random.NextDouble() * 10.0f);

                        if (newPos.Y < -1.0f * m_cryptHeight)
                        {
                            newPos.Y = -1.0f * m_cryptHeight + 0.1f;
                        }

                        int childId = m_cells.AddCell(newPos, m_cells.BetaCatenin[i], 50.0f + ((float)m_random.NextDouble() * 50.0f), m_cells.Colours[i], m_cells.ColourIndices[i], m_cells.CryptIds[i], m_separation, CellCycleStage.Child);
                        m_cells.CycleStages[childId] = CellCycleStage.Child;
                        m_colourCounts[m_cells.ColourIndices[i]]++;

                        m_cells.ChildPointIndices[i] = childId;
                    }
                }
            }
        }
    }
}
