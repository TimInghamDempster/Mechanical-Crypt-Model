using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CryptCC
{
    class Program
    {
        public static SlimDX11Renderer.InputHandler Input;
        public static GameWorld.World m_gameWorld;
        static SlimDX11Renderer.Camera m_camera; 

        static void Main(string[] args)
        {

            SlimDX11Renderer.Renderer renderer = new SlimDX11Renderer.Renderer("Cell-Centre Crypt Simulation");
            m_gameWorld = new GameWorld.World(renderer);

            m_camera = new SlimDX11Renderer.Camera((float)(Math.PI / 3.0), 1280.0f / 768.0f, 0.001f, 100.0f);

            Input = new SlimDX11Renderer.InputHandler();

            var funcs = new Core.UpdateFunctions();
            funcs.AddUpdateFunction(UpdateInput, new TimeSpan(0, 0, 0, 0, 20));
            funcs.AddUpdateFunction(m_gameWorld.Tick, new TimeSpan(0, 0, 0, 0, 20));

            int camIndex = renderer.AddCamera(m_camera);
            renderer.SetCurrentCamera(camIndex);
            m_camera.Locator.Move(0.0f, 0.0f, 2.0f);

            renderer.Run(funcs);

            renderer.Dispose();
        }

        static void UpdateInput()
        {
            if (Input.Query(Keys.A))
            {
                m_camera.Locator.MoveLocal(-0.02f, 0.0f, 0.0f);
            }
            if (Input.Query(Keys.D))
            {
                m_camera.Locator.MoveLocal(0.02f, 0.0f, 0.0f);
            }
            if (Input.Query(Keys.W))
            {
                m_camera.Locator.MoveLocal(0.0f, 0.0f, 0.02f);
            }
            if (Input.Query(Keys.S))
            {
                m_camera.Locator.MoveLocal(0.0f, 0.0f, -0.02f);
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
        }
    }
}
