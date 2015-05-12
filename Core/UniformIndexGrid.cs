using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public class UniformIndexGrid
    {
        public List<int>[] m_indices;
        Vector3d m_boxSize;
        Vector3d m_zeroCorner;
        Vector3d m_size;

        int m_xBoxes;
        int m_yBoxes;
        int m_zBoxes;

        public int XBoxes { get { return m_xBoxes; } }
        public int YBoxes { get { return m_yBoxes; } }
        public int ZBoxes { get { return m_zBoxes; } }

        public UniformIndexGrid(int xBoxes, int yBoxes, int zBoxes, Vector3d size, Vector3d zeroCorner)
        {
            m_xBoxes = xBoxes;
            m_yBoxes = yBoxes;
            m_zBoxes = zBoxes;

            m_size = size;
            m_boxSize = size;
            m_boxSize.X /= (float)(xBoxes);
            m_boxSize.Y /= (float)(yBoxes);
            m_boxSize.Z /= (float)(zBoxes);

            m_zeroCorner = zeroCorner;

            m_indices = new List<int>[m_xBoxes * m_yBoxes * m_zBoxes];
            for (int i = 0; i < m_xBoxes * m_yBoxes * m_zBoxes; i++)
            {
                m_indices[i] = new List<int>();
            }
        }

        public int CalcNumCollisionBoxesInCentre(float radius)
        {
            float diameter = radius * 2;

            Vector3d boxSizeAbs = m_boxSize;
            boxSizeAbs.X = Math.Abs(boxSizeAbs.X);
            boxSizeAbs.Y = Math.Abs(boxSizeAbs.Y);
            boxSizeAbs.Z = Math.Abs(boxSizeAbs.Z);

            int localXBoxes = (int)(diameter / boxSizeAbs.X) + 2;
            int localYBoxes = (int)(diameter / boxSizeAbs.Y) + 2;
            int localZBoxes = (int)(diameter / boxSizeAbs.Z) + 2;

            return localXBoxes * localYBoxes * localZBoxes;
        }

        public int CalcBox(Vector3d position)
        {
            position -= m_zeroCorner;

            int xBox = GetBoxId(position.X, m_boxSize.X, XBoxes - 1);
            int yBox = GetBoxId(position.Y, m_boxSize.Y, YBoxes - 1);
            int zBox = GetBoxId(position.Z, m_boxSize.Z, ZBoxes - 1);

            int box = xBox + yBox * m_xBoxes + zBox * m_xBoxes * m_yBoxes;
            return box;
        }

        int GetBoxId(float positionOnAxis, float boxSizeOnAxis, int numBoxesOnAxis)
        {
            return Clamp((int)(positionOnAxis / boxSizeOnAxis), 0, numBoxesOnAxis);
        }

        public void GetCollisionBoxes(Vector3d position, float radius, List<int> collisionBoxes)
        {
            position -= m_zeroCorner;
            collisionBoxes.Clear();

            Vector3d boxSizeAbs = m_boxSize;
            boxSizeAbs.X = Math.Abs(boxSizeAbs.X);
            boxSizeAbs.Y = Math.Abs(boxSizeAbs.Y);
            boxSizeAbs.Z = Math.Abs(boxSizeAbs.Z);

            // Need to use abs box size to gauruntee the loop runs the
            // right way for the exit condition.
            int localXBoxes = (int)(radius / boxSizeAbs.X) + 1;
            int localYBoxes = (int)(radius / boxSizeAbs.Y) + 1;
            int localZBoxes = (int)(radius / boxSizeAbs.Z) + 1;

            int xBox = GetBoxId(position.X, m_boxSize.X, XBoxes - 1);
            int yBox = GetBoxId(position.Y, m_boxSize.Y, YBoxes - 1);
            int zBox = GetBoxId(position.Z, m_boxSize.Z, ZBoxes - 1);

            for (int dx = -localXBoxes; dx <= localXBoxes; dx++)
            {
                for (int dy = -localYBoxes; dy <= localYBoxes; dy++)
                {
                    for (int dz = -localZBoxes; dz <= localZBoxes; dz++)
                    {
                        int x = xBox + dx;
                        int y = yBox + dy;
                        int z = zBox + dz;

                        if (x >= 0 && x < m_xBoxes &&
                            y >= 0 && y < m_yBoxes &&
                            z >= 0 && z < m_zBoxes)
                        {
                            collisionBoxes.Add(x + y * m_xBoxes + z * m_xBoxes * m_yBoxes);
                        }
                    }
                }
            }
        }

        // No need to die if the user passes in a bad
        // value, just store it in the nearest edge box.
        int Clamp(int inVal, int min, int max)
        {
            if (inVal < min)
            {
                return min;
            }
            if (inVal > max)
            {
                return max;
            }
            return inVal;
        }
    }
}
