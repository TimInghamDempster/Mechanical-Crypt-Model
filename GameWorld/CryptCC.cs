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

		RenderArrays3d m_renderArraysAnoikis;
		RenderArrays3d m_renderArraysCells;

		CellArrayCC m_cells;
		CryptArrayCC m_crypts;

		Int64 framecount;

		Random m_random;

		System.IO.StreamWriter outfile;

		const int m_finalFrame = 500 * 200;

		const float SecondsPerTimestep = 60.0f;

		const int m_numCryptsPerSide = 1;
		const float m_initialCryptSeparation = 2000.0f;
		const float m_fieldHalfSize = (float)m_numCryptsPerSide / 2.0f * m_initialCryptSeparation + 100.0f;

		const float m_averageGrowthTimeSeconds = 108000.0f; // == 30 hours for complete cell cycle
		const float m_averageGrowthTimesteps = m_averageGrowthTimeSeconds / SecondsPerTimestep;

		const float m_cryptRadius = 500.0f;
		const float m_cellsPerColumn = 80.0f; // Look up reference for this, lots in Potten
		const float m_flutingRadius = 500.0f;
		const float m_betaCateninRequirement = 20.0f;
		const float m_cellsPerRadius = 23.0f; // Have a reference for this but need to find it.
		const float m_betaCateninConsumptionPerTimestep = 0.5f;
		const float m_anoikisProbabilityPerTimestep = 0.002f;
		const float m_membraneSeparationToTriggerAnoikis = 100.0f;
		const float m_offMembraneRestorationFactor = 0.001f;
		const float m_stromalRestorationFactor = 0.3f;
		Vector2d m_colonBoundary = new Vector2d(m_fieldHalfSize, m_fieldHalfSize);
		const float m_colonBoundaryRepulsionFactor = 0.3f;
		const float m_cellStiffness = 0.3f;
		Colour[] m_baseColours;
		int[] m_colourCounts;
		UniformIndexGrid m_grid;

		const float m_compressionFactor = 0.75f; // Account for the fact that the cells compress so we have more of them than we get from simple legth/radius calculation

		static float CellSize { get { return (float)(m_cryptRadius * 2.0f * Math.PI / m_cellsPerRadius / 2.0f / m_compressionFactor); } } // == crypt circumference (2 * Pi * R) / cell diameter (2 * r) / compression overshoot
		static float CryptHeight { get { return CellSize * m_cellsPerColumn; } }

		const float m_averageNumberOfCellsInCycle = 100 * m_compressionFactor;

		float m_basicG0ProliferationBoundary = CryptHeight * -0.5f;
		float m_basicG0StemBoundary = CryptHeight * -0.95f;

		static float BasicG0ProliferationBetaCateninRequirement { get { return 0.0f; } }// (m_cellsPerRadius * m_cellsPerColumn * m_averageGrowthTimesteps / m_averageNumberOfCellsInCycle) - m_averageGrowthTimesteps; } }
		static float BasicG0StemBetaCateninRequirement { get { return m_averageGrowthTimesteps * 9.0f; } }

		public CryptCC(IRenderer renderer, string filename)
		{
			outfile = new System.IO.StreamWriter(@"C:\Users\Tim\Desktop\" + filename, false);
			m_renderer = renderer;
			m_scene = m_renderer.GetNewScene();

			m_random = new Random(DateTime.Now.Millisecond);

			m_cells = new CellArrayCC();

			m_renderArraysAnoikis = new RenderArrays3d();
			m_renderArraysAnoikis.Positions = new List<Vector3d>();
			m_renderArraysAnoikis.Colours = new List<Colour>();
			m_renderArraysAnoikis.Visible = new List<bool>();

			m_renderArraysCells = new RenderArrays3d();
			m_renderArraysCells.Positions = m_cells.Positions;
			m_renderArraysCells.Colours = m_cells.Colours;
			m_renderArraysCells.Visible = m_cells.Active;

			m_scene.CreateCamera();
			m_scene.SetCurrentCamera(0);

			m_scene.RenderArrays3d.Add(m_renderArraysCells);

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

			m_crypts = new CryptArrayCC();

			float centeringOffset = m_fieldHalfSize - 2000f;

			for (int y = 0; y < m_numCryptsPerSide; y++)
			{
				for (int x = 0; x < m_numCryptsPerSide; x++)
				{
					int colourIndex = (x + y * m_numCryptsPerSide) % m_baseColours.Count();

					PopulateCrypt(x, y, colourIndex, centeringOffset);

					m_crypts.Add(new Vector3d(x * m_initialCryptSeparation - centeringOffset, 0.0f, y * m_initialCryptSeparation - centeringOffset));
				}
			}

			m_grid = new UniformIndexGrid(m_numCryptsPerSide * 2, 30, m_numCryptsPerSide * 2, new Vector3d(2.0f * (m_colonBoundary.X + 100.0f), -1.0f * (CryptHeight + 500.0f), 2.0f * (m_colonBoundary.Y + 100.0f)), new Vector3d(-1.0f * (m_colonBoundary.X + 10.0f), 0.0f, -1.0f * (m_colonBoundary.Y + 10.0f)));
		}

		public void PopulateCrypt(int cryptX, int cryptY, int cryptColourIndex, float centeringOffset)
		{
			for(int hIndex = 1; hIndex < m_cellsPerColumn / 2 - 2; hIndex++)
			{
				float height = ( CryptHeight * hIndex * 2 / m_cellsPerColumn);

				for(int rIndex = 0; rIndex < m_cellsPerRadius; rIndex++)
				{
					float theta = (float)(2.0 * Math.PI * rIndex / m_cellsPerRadius);

					float x = (float)(Math.Cos(theta) * m_cryptRadius);
					float z = (float)(Math.Sin(theta) * m_cryptRadius);

					Vector3d pos = new Vector3d(cryptX * m_initialCryptSeparation - centeringOffset, -1.0f * CryptHeight, cryptY * m_initialCryptSeparation - centeringOffset);
					pos.X += x;
					pos.Y += height;
					pos.Z += z;

					m_cells.AddCell(pos,
							   (float)(-1000.0 * m_random.NextDouble()), // Randomise time of first division as we are jumping in part way through the simulation.
							   m_averageGrowthTimesteps,
							   m_baseColours[cryptColourIndex],
							   cryptColourIndex,
							   (UInt32)(cryptX + cryptY * m_numCryptsPerSide),
							   CellSize,
							   CellCycleStage.G0);
				}
			}
		}

		public void SwapDisplayMode()
		{
			if (m_scene.RenderArrays3d.Contains(m_renderArraysCells))
			{
				m_scene.RenderArrays3d.Remove(m_renderArraysCells);
				m_scene.RenderArrays3d.Add(m_renderArraysAnoikis);
			}
			else
			{
				m_scene.RenderArrays3d.Remove(m_renderArraysAnoikis);
				m_scene.RenderArrays3d.Add(m_renderArraysCells);
			}
		}



		private void OutputProliferationAndCellCount()
		{
			int count = 0;
			int ki67Count = 0;

			for (int i = 0; i < m_cells.Active.Count; i++)
			{
				bool active = m_cells.Active[i];
				if (active)
				{
					count++;
					if (m_cells.CycleStages[i] == CellCycleStage.G)
					{
						ki67Count++;
					}
				}
			}

			outfile.WriteLine(count.ToString() + ',' + ki67Count.ToString());
			outfile.Flush();
		}

		void CountCells()
		{
			int count = 0;

			foreach (bool active in m_cells.Active)
			{
				if (active)
				{
					count++;
				}
			}

			outfile.WriteLine(count.ToString());
			outfile.Flush();
		}

		void OutputAnoikisData()
		{
			int[] data = new int[100];

			foreach (var pos in m_renderArraysAnoikis.Positions)
			{
				int index = (int)((pos.Y - 250) / (CryptHeight + 250) * 100.0f);
				index *= -1;

				data[index]++;
			}

			foreach (int dataPoint in data)
			{
				outfile.WriteLine(dataPoint);
			}
			outfile.Flush();
		}

		public bool Tick()
		{
			for (int i = 0; i < 2; i++)
			{
				if (framecount % 200 == 0)
				{
					CountCells();
					// OutputAnoikisData();
					//OutputProliferationAndCellCount();
				}

				if (framecount == m_finalFrame)
				{
					outfile.Close();
					return true;
				}

				framecount++;

				if (framecount > 10000)
				{
					int a = 0;
					a++;
				}

				m_crypts.PreTick();
				//UpdateWnt(); // More biologically based G0 model that needs ephrin modelling to be realistic.
				DoBasicG0Phase(); // Basic phenomenological G0 model.
				DoGrowthPhase();
				AssignCellsToGrid();
				EnforceCryptWalls();
				DoCollisionAndMovement();
				DoAnoikis();
				EnforceColonBoundary();
			}
			return false;
		}

		void AssignCellsToGrid()
		{
			for (int i = 0; i < m_cells.Positions.Count; i++)
			{
				if (m_cells.Active[i])
				{
					int box = m_grid.CalcBox(m_cells.Positions[i]);
					if (m_cells.GridIndices[i] == -1)
					{
						m_grid.m_indices[box].Add(i);
						m_cells.GridIndices[i] = box;
					}
					if (m_cells.GridIndices[i] != box)
					{
						m_grid.m_indices[m_cells.GridIndices[i]].Remove(i);
						m_grid.m_indices[box].Add(i);
						m_cells.GridIndices[i] = box;
					}
				}
			}
		}

		void DoAnoikis()
		{
			for (int i = 0; i < m_cells.Positions.Count; i++)
			{
				if (m_cells.Active[i])
				{
					if (m_cells.OffMembraneDistance[i] > m_membraneSeparationToTriggerAnoikis)
					{
						m_renderArraysAnoikis.Positions.Add(m_cells.Positions[i]);
						m_renderArraysAnoikis.Colours.Add(m_baseColours[0]);
						m_renderArraysAnoikis.Visible.Add(true);

                        if (m_cells.CycleStages[i] == CellCycleStage.G || m_cells.CycleStages[i] == CellCycleStage.Child)
                        {
                            m_cells.Remove(m_cells.ChildPointIndices[i]);
                        }
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
			int capacity = m_grid.CalcNumCollisionBoxesInCentre(2.0f * CellSize);
			List<int> surroundingBoxes = new List<int>(capacity);
			for (int i = 0; i < m_cells.Positions.Count; i++)
			{
				if (m_cells.Active[i])
				{
					m_grid.GetCollisionBoxes(m_cells.Positions[i], 2.0f * CellSize, surroundingBoxes);
					{
						for (int box = 0; box < surroundingBoxes.Count; box++)
						{
							List<int> cellsInBox = m_grid.m_indices[surroundingBoxes[box]];
							for (int cell = 0; cell < cellsInBox.Count; cell++)
							{
								int j = cellsInBox[cell];
								if (i < j) // Avoid double checks.
								{
									if (m_cells.Active[j])
									{

										int cryptId1 = (int)m_cells.CryptIds[i];
										int cryptId2 = (int)m_cells.CryptIds[j];

										Vector3d cryptPos1 = m_crypts.m_cryptPositions[cryptId1];
										Vector3d cryptPos2 = m_crypts.m_cryptPositions[cryptId2];

										Vector3d outerPos = m_cells.OnMembranePositions[i];
										Vector3d innerPos = m_cells.OnMembranePositions[j];
										//Vector3d dummy;

										//GetClosestPointOnMembrane(m_cells.Positions[i] - cryptPos1, out outerPos, out dummy);
										//GetClosestPointOnMembrane(m_cells.Positions[j] - cryptPos2, out innerPos, out dummy);

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
											restitution *= m_cellStiffness;
											if (separation < 0.1f)
											{
												separation = 0.1f;
												delta.X = (float)m_random.NextDouble() - 0.5f;
												delta.Y = (float)m_random.NextDouble() - 0.5f;
												delta.Z = (float)m_random.NextDouble() - 0.5f;
											}

											Vector3d force = delta * restitution / separation;
											Vector3d cryptForce = force;
											cryptForce.Y = 0.0f;

											m_crypts.m_cellularity[cryptId1]++;
											m_crypts.m_cellularity[cryptId2]++;

											if ((cryptPos1 - outerPos).Length() < m_cryptRadius + m_flutingRadius)
											{
												//m_crypts.m_forces[cryptId1] += cryptForce;
											}

											if ((cryptPos2 - innerPos).Length() < m_cryptRadius + m_flutingRadius)
											{
												//m_crypts.m_forces[cryptId2] -= cryptForce;
											}

											m_cells.Positions[i] += force;
											m_cells.Positions[j] -= force;
										}
									}
								}
							}
						}
					}
				}
			}
		}

		void EnforceColonBoundary()
		{
			for (int i = 0; i < m_cells.Positions.Count; i++)
			{
				if (m_cells.Active[i])
				{
					int crypt = (int)m_cells.CryptIds[i];
					var pos = m_cells.Positions[i] - m_crypts.m_cryptPositions[crypt];

					if (pos.X > m_colonBoundary.X)
					{
						float delta = pos.X - m_colonBoundary.X;
						delta *= m_colonBoundaryRepulsionFactor;
						pos.X -= delta;
					}
					if (pos.X < (-1.0f * m_colonBoundary.X))
					{
						float delta = pos.X - (-1.0f * m_colonBoundary.X);
						delta *= m_colonBoundaryRepulsionFactor;
						pos.X -= delta;
					}
					if (pos.Z > m_colonBoundary.Y)
					{
						float delta = pos.Z - m_colonBoundary.Y;
						delta *= m_colonBoundaryRepulsionFactor;
						pos.Z -= delta;
					}
					if (pos.Z < (-1.0f * m_colonBoundary.Y))
					{
						float delta = pos.Z - (-1.0f * m_colonBoundary.Y);
						delta *= m_colonBoundaryRepulsionFactor;
						pos.Z -= delta;
					}

					m_cells.Positions[i] = pos + m_crypts.m_cryptPositions[crypt];
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

					// guard against 0 length vector division
					if (pos.X == 0.0f && pos.Z == 0.0f)
					{
						pos.X = 0.1f;
					}

					// Don't fall through the bottom of the crypt
					if (pos.Y < -1.0f * CryptHeight)
					{
						pos.Y = -1.0f * CryptHeight + 10.0f;
					}

					Vector2d pos2d = new Vector2d(pos.X, pos.Z);

					Vector3d normal;
					Vector3d membranePos;
					GetClosestPointOnMembrane(pos, out membranePos, out normal);

					membranePos += m_crypts.m_cryptPositions[cryptId];
					Vector3d delta = m_cells.Positions[i] - membranePos;

					m_cells.OffMembraneDistance[i] = delta.Length();


					bool isAboveBasementMembrane = Vector3d.DotProduct(delta, normal) > 0.0f;

					if (isAboveBasementMembrane == false)
					{
						m_cells.OffMembraneDistance[i] *= -1.0f;
						delta *= m_stromalRestorationFactor;
					}
					else
					{
						delta *= m_offMembraneRestorationFactor;
					}

					m_cells.Positions[i] -= delta;
					m_cells.OnMembranePositions[i] = membranePos;
				}
			}
		}

		void GetClosestPointOnMembrane(Vector3d inputPoint, out Vector3d outPoint, out Vector3d normal)
		{
			Vector2d pos2d = new Vector2d(inputPoint.X, inputPoint.Z);

			if (pos2d.Length() > m_cryptRadius + m_flutingRadius)
			{
				outPoint = inputPoint;
				outPoint.Y = 0.0f;
				normal = new Vector3d(0.0f, 1.0f, 0.0f);
			}
			else if (inputPoint.Y > m_flutingRadius * -1.0f)
			{
				Vector2d virtualSphereDirection = pos2d;
				virtualSphereDirection /= virtualSphereDirection.Length();
				virtualSphereDirection *= m_cryptRadius + m_flutingRadius;

				Vector3d virtualSpherePosition = new Vector3d(virtualSphereDirection.X, m_flutingRadius * -1.0f, virtualSphereDirection.Y);

				Vector3d sphereRelativeCellPosition = inputPoint - virtualSpherePosition;

				normal = sphereRelativeCellPosition / sphereRelativeCellPosition.Length();

				sphereRelativeCellPosition /= sphereRelativeCellPosition.Length();
				sphereRelativeCellPosition *= m_flutingRadius;

				outPoint = virtualSpherePosition + sphereRelativeCellPosition;
			}
			else if (inputPoint.Y > (CryptHeight - m_cryptRadius) * -1.0f)
			{
				Vector2d final;
				Vector2d normalised = pos2d / pos2d.Length();
				final = normalised * m_cryptRadius;

				outPoint.X = final.X;
				outPoint.Y = inputPoint.Y;
				outPoint.Z = final.Y;

				normal.X = pos2d.X * -1.0f;
				normal.Y = 0.0f;
				normal.Z = pos2d.Y * -1.0f;
				normal /= normal.Length();
			}
			else
			{
				Vector3d nicheCentre = new Vector3d(0.0f, (CryptHeight - m_cryptRadius) * -1.0f, 0.0f);
				Vector3d normalisedPositionRelativeToNicheCentre = inputPoint - nicheCentre;
				normalisedPositionRelativeToNicheCentre = normalisedPositionRelativeToNicheCentre / normalisedPositionRelativeToNicheCentre.Length();
				outPoint = normalisedPositionRelativeToNicheCentre * m_cryptRadius + nicheCentre;
				normal = normalisedPositionRelativeToNicheCentre * -1.0f;
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
                            m_cells.ChildPointIndices[childIndex] = -1;
						}
					}
				}
			}
		}

		void DoBasicG0Phase()
		{
			for (int i = 0; i < m_cells.Positions.Count; i++)
			{
				if (m_cells.Active[i] && m_cells.CycleStages[i] == CellCycleStage.G0)
				{
					m_cells.BetaCatenin[i]++;

					if (m_cells.Positions[i].Y < m_basicG0StemBoundary)
					{
						if (m_cells.BetaCatenin[i] > BasicG0StemBetaCateninRequirement)
						{
							EnterG1(i);
						}
					}
					else if (m_cells.Positions[i].Y < m_basicG0ProliferationBoundary)
					{
						if (m_cells.BetaCatenin[i] > BasicG0ProliferationBetaCateninRequirement)
						{
							EnterG1(i);
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
					float height = m_cells.Positions[i].Y + CryptHeight;
					float wntAmount = 1.0f - height / CryptHeight;

					m_cells.BetaCatenin[i] += wntAmount - m_betaCateninConsumptionPerTimestep;

					if (m_cells.BetaCatenin[i] < 0.0f)
					{
						m_cells.BetaCatenin[i] = 0.0f;
					}

					if (m_cells.BetaCatenin[i] > m_betaCateninRequirement)
					{
						EnterG1(i);
					}
				}
			}
		}

		void EnterG1(int cellId)
		{
			m_cells.CycleStages[cellId] = CellCycleStage.G;
			m_cells.BetaCatenin[cellId] = 0;
			m_cells.Colours[cellId] = new Colour() { A = 1.0f, R = 1.0f, G = 0.0f, B = 1.0f };

			Vector3d newPos = m_cells.Positions[cellId];
			newPos.X += 5.0f - ((float)m_random.NextDouble() * 10.0f);
			newPos.Y += 5.0f - ((float)m_random.NextDouble() * 10.0f);
			newPos.Z += 5.0f - ((float)m_random.NextDouble() * 10.0f);

			if (newPos.Y < -1.0f * CryptHeight)
			{
				newPos.Y = -1.0f * CryptHeight + 0.1f;
			}

			int childId = m_cells.AddCell(newPos,
				0.0f,
				(float)(0.5f + m_random.NextDouble()) * m_averageGrowthTimesteps,
				m_cells.Colours[cellId],
				m_cells.ColourIndices[cellId],
				m_cells.CryptIds[cellId],
				CellSize,
				CellCycleStage.Child);

			m_cells.GrowthStageRequiredTimes[cellId] = (float)(0.5f + m_random.NextDouble()) * m_averageGrowthTimesteps;

			m_cells.CycleStages[childId] = CellCycleStage.Child;
			m_colourCounts[m_cells.ColourIndices[cellId]]++;

			m_cells.ChildPointIndices[cellId] = childId;
            m_cells.ChildPointIndices[childId] = cellId;
		}
	}
}
