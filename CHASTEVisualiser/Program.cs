using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CHASTEVisualiser
{
    class Program
    {

        public static SlimDX11Renderer.InputHandler Input;
        public static GameWorld.CellVisualiser m_visualiser;
        static SlimDX11Renderer.Camera m_camera;

        static void Main(string[] args)
        {

            SlimDX11Renderer.Renderer3d renderer = new SlimDX11Renderer.Renderer3d("Cell-Centre Crypt Simulation");
            m_visualiser = new GameWorld.CellVisualiser(renderer);

            m_camera = new SlimDX11Renderer.Camera((float)(Math.PI / 3.0), 1280.0f / 768.0f, 0.1f, 10000.0f);

            Input = new SlimDX11Renderer.InputHandler();

            var funcs = new Core.UpdateFunctions();
            funcs.AddUpdateFunction(UpdateInput, new TimeSpan(0, 0, 0, 0, 20));
            funcs.AddUpdateFunction(m_visualiser.Tick, new TimeSpan(0, 0, 0, 0, 20));

            int camIndex = renderer.AddCamera(m_camera);
            renderer.SetCurrentCamera(camIndex);
            m_camera.Locator.Move(0.0f, 0.0f, 1000.0f);

            renderer.Run(funcs);

            renderer.Dispose();
        }

        static bool UpdateInput()
        {
            if (Input.Query(Keys.A))
            {
                m_camera.Locator.MoveLocal(-20.0f, 0.0f, 0.0f);
            }
            if (Input.Query(Keys.D))
            {
                m_camera.Locator.MoveLocal(20.0f, 0.0f, 0.0f);
            }
            if (Input.Query(Keys.W))
            {
                m_camera.Locator.MoveLocal(0.0f, 0.0f, 20.0f);
            }
            if (Input.Query(Keys.S))
            {
                m_camera.Locator.MoveLocal(0.0f, 0.0f, -20.0f);
            }
            if (Input.Query(Keys.Up))
            {
                m_camera.Locator.Pitch(0.05f);
            }
            if (Input.Query(Keys.Down))
            {
                m_camera.Locator.Pitch(-0.05f);
            }
            if (Input.Query(Keys.Left))
            {
                m_camera.Locator.Heading(-0.05f);
            }
            if (Input.Query(Keys.Right))
            {
                m_camera.Locator.Heading(0.05f);
            }
			if(Input.Query(Keys.Escape))
			{
				return true;
			}

			return false;
        }
    }
}
