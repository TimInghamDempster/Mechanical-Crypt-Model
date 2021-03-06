﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core;

namespace GameWorld
{
    public enum CellCycleStage
    {
        Child, // This cell is not yet independant and forms part of a growing cell before it divides.
        G0,
        G1,
        M
    }

    public class CellArrayCC
    {
        const float m_growthRate = 10.0f;

        public List<Vector3d> Positions;
		public List<Vector3d> OnMembranePositions;
        public List<float> Radii;
        public List<Colour> Colours;
        public List<float> BetaCatenin;
        public List<CellCycleStage> CycleStages;
        public List<float> GrowthStageCurrentTimesteps;
        public List<float> GrowthStageRequiredTimesteps;
        public List<float> MStageCurrentTimesteps;
        public List<int> ColourIndices;
        public List<UInt32> CryptIds;
        public List<int> ChildPointIndices;
        public List<bool> Active;
        public List<float> OffMembraneDistance;
        public List<int> GridIndices;

        List<int> m_freeIndices;

        public CellArrayCC()
        {
            Positions = new List<Vector3d>();
			OnMembranePositions = new List<Vector3d>();
            Radii = new List<float>();
            Colours = new List<Colour>();
            BetaCatenin = new List<float>();
            CycleStages = new List<CellCycleStage>();
            GrowthStageCurrentTimesteps = new List<float>();
            GrowthStageRequiredTimesteps = new List<float>();
            MStageCurrentTimesteps = new List<float>();
            ColourIndices = new List<int>();
            CryptIds = new List<UInt32>();
            ChildPointIndices = new List<int>();
            Active = new List<bool>();
            OffMembraneDistance = new List<float>();
            GridIndices = new List<int>();

            m_freeIndices = new List<int>();
        }

        public void Remove(int index)
        {
            //m_freeIndices.Add(index);
            Colours[index] = new Colour() { A = 0.0f, R = 0.0f, G = 0.0f, B = 0.0f };
            Active[index] = false;
        }

        public int AddCell(Vector3d position, float wnt, float growthStageRequiredTime, Colour colour, int colourIndex, UInt32 cryptIndex, float radius, CellCycleStage cycleStage)
        {
            if (m_freeIndices.Count > 0)
            {
                int index = m_freeIndices[m_freeIndices.Count - 1];
                m_freeIndices.RemoveAt(m_freeIndices.Count - 1);

                Positions[index] = position;
				OnMembranePositions[index] = position;
                Radii[index] = radius;
                Colours[index] = colour;
                BetaCatenin[index] = wnt;
                GrowthStageRequiredTimesteps[index] = growthStageRequiredTime;
                GrowthStageCurrentTimesteps[index] = 0.0f;
                MStageCurrentTimesteps[index] = 0.0f;
                CycleStages[index] = cycleStage;
                ColourIndices[index] = colourIndex;
                CryptIds[index] = cryptIndex;
                ChildPointIndices[index] = -1;
                Active[index] = true;
                OffMembraneDistance[index] = 0.0f;
                GridIndices[index] = -1;
                return index;
            }
            else
            {
                Positions.Add(position);
				OnMembranePositions.Add(position);
                Radii.Add(radius);
                Colours.Add(colour);
                BetaCatenin.Add(wnt);
                GrowthStageRequiredTimesteps.Add(growthStageRequiredTime);
                GrowthStageCurrentTimesteps.Add(0.0f);
                MStageCurrentTimesteps.Add(0.0f);
                CycleStages.Add(cycleStage);
                ColourIndices.Add(colourIndex);
                CryptIds.Add(cryptIndex);
                ChildPointIndices.Add(-1);
                Active.Add(true);
                OffMembraneDistance.Add(0.0f);
                GridIndices.Add(-1);
                return Active.Count - 1;
            }
        }
    }
}
