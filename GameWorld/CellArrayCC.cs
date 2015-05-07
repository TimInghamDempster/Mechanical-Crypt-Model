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
        Child, // This cell is not yet independant and forms part of a growing cell before it divides.
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
        public List<bool> Active;

        List<int> m_freeIndices;

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
            Active = new List<bool>();

            m_freeIndices = new List<int>();
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

        {
        }
    }
}
