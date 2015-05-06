using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Core;

namespace GameWorld
{
    class CryptArrayCC
    {
        public List<Vector3d> m_cryptPositions;
        public List<Vector3d> m_forces;
        public List<UInt32> m_cellularity;

        const float m_forceMultiplyer = 0.01f;

        public CryptArrayCC()
        {
            m_cryptPositions = new List<Vector3d>();
            m_forces = new List<Vector3d>();
            m_cellularity = new List<UInt32>();
        }

        public void PreTick()
        {
            for (int i = 0; i < m_cellularity.Count; i++)
            {
                m_cryptPositions[i] += m_forces[i] * m_forceMultiplyer;
                m_forces[i] = new Vector3d();
            }
        }

        public void Add(Vector3d position)
        {
            m_cryptPositions.Add(position);
            m_forces.Add(new Vector3d());
            m_cellularity.Add(0);
        }
    }
}
