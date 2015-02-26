using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
    public interface IRenderableItem
    {
        void SetSize(float width, float height);

        void SetColour(float red, float green, float blue, float alpha);

        void SetPos(float m_x, float m_y);

        bool IsVisible { get; set; }
    }
}
