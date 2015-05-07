using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core;

namespace GameWorld
{
    public enum CellCycleStage
    {
        G0,
        G
    }

    public class CellArrayCC
    {
        const float m_growthRate = 10.0f;

        public List<Vector3d> Positions;
        public List<float> Radii;
        public List<Colour> Colours;
        public List<float> BetaCatenin;
        public List<CellCycleStage> CycleStages;
        public List<float> GrowthStageCurrentTimes;
        public List<float> GrowthStageRequiredTimes;
        public List<int> ColourIndices;
        public List<UInt32> CryptIds;
        public List<int> ChildPointIndices;

        public CellArrayCC()
        {
            Positions = new List<Vector3d>();
            Radii = new List<float>();
            Colours = new List<Colour>();
            BetaCatenin = new List<float>();
            CycleStages = new List<CellCycleStage>();
            GrowthStageCurrentTimes = new List<float>();
            GrowthStageRequiredTimes = new List<float>();
            ColourIndices = new List<int>();
            CryptIds = new List<UInt32>();
            ChildPointIndices = new List<int>();
        }

        public void Tick()
        {
            for (int i = 0; i < Radii.Count; i++)
            {
                if (Radii[i] < CryptCC.m_separation)
                {
                    Radii[i] += m_growthRate;
                }
            }
        }

        public void Remove(int index)
        {
            Positions.RemoveAt(index);
            Radii.RemoveAt(index);
            Colours.RemoveAt(index);
            BetaCatenin.RemoveAt(index);
            CycleStages.RemoveAt(index);
            GrowthStageCurrentTimes.RemoveAt(index);
            ColourIndices.RemoveAt(index);
            GrowthStageRequiredTimes.RemoveAt(index);
            CryptIds.RemoveAt(index);
            ChildPointIndices.RemoveAt(index);
        }

        public void AddCell(Vector3d position, float wnt, float growthStageRequiredTime, Colour colour, int colourIndex, UInt32 cryptIndex, float radius)
        {
            Positions.Add(position);
            Radii.Add(radius);
            Colours.Add(colour);
            BetaCatenin.Add(wnt);
            GrowthStageRequiredTimes.Add(growthStageRequiredTime);
            GrowthStageCurrentTimes.Add(0.0f);
            CycleStages.Add(CellCycleStage.G0);
            ColourIndices.Add(colourIndex);
            CryptIds.Add(cryptIndex);
            ChildPointIndices.Add(-1);
        }
    }
}
