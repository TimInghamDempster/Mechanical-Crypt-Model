using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX;

namespace SlimDX11Renderer
{
    public class DirectionalLight
    {
         Locator direction_;

        public Core.ILocator Direction
        {
            get
            {
                return direction_;
            }
        }

        public DirectionalLight()
        {
            direction_ = new Locator(new Vector3(0),Vector3.UnitZ, Vector3.UnitY);
        }
    }
}
