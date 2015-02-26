using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snowball
{
    class Program
    {
        public static SlimDX11Renderer.InputHandler Input;
        public static GameWorld.World m_gameWorld;

        static void Main(string[] args)
        {

            SlimDX11Renderer.Renderer renderer = new SlimDX11Renderer.Renderer("Mechanical Crypt Simulation");
            m_gameWorld = new GameWorld.World(renderer);

            Input = new SlimDX11Renderer.InputHandler();

            var funcs = new Core.UpdateFunctions();
            funcs.AddUpdateFunction(UpdateInput, new TimeSpan(0, 0, 0, 0, 20));
            funcs.AddUpdateFunction(m_gameWorld.Tick, new TimeSpan(0, 0, 0, 0, 20));

            renderer.Run(funcs);

            renderer.Dispose();
        }

        static void UpdateInput()
        {
           if(Input.Query(Keys.Space))
           {
               // Do something
           }
        }
    }
}
