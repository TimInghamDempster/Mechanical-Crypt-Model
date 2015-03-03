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
        public List<Vector3d> Positions;
        public List<Colour> Colours;
        public List<float> Wnt;
        public List<CellCycleStage> CycleStages;
        public List<float> GrowthStageCurrentTimes;
        public List<float> GrowthStageRequiredTimes;

        public CellArrayCC()
        {
            Positions = new List<Vector3d>();
            Colours = new List<Colour>();
            Wnt = new List<float>();
            CycleStages = new List<CellCycleStage>();
            GrowthStageCurrentTimes = new List<float>();
            GrowthStageRequiredTimes = new List<float>();
        }

        public void Remove(int index)
        {
            Positions.RemoveAt(index);
            Colours.RemoveAt(index);
            Wnt.RemoveAt(index);
            CycleStages.RemoveAt(index);
            GrowthStageCurrentTimes.RemoveAt(index);
            GrowthStageRequiredTimes.RemoveAt(index);
        }

        public void AddCell(Vector3d position, float wnt, float growthStageRequiredTime)
        {
            Positions.Add(position);
            Colours.Add(new Colour());
            Wnt.Add(wnt);
            GrowthStageRequiredTimes.Add(growthStageRequiredTime);
            GrowthStageCurrentTimes.Add(0.0f);
            CycleStages.Add(CellCycleStage.G0);
        }
    }
}
