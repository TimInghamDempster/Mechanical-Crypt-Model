using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;

namespace GameWorld
{
    public class CellVisualiser
    {
        IRenderer m_renderer;
        IRenderableScene m_scene;

        RenderArrays3d m_renderArrays;

        Random m_random;

        Colour[] m_baseColours;

        public CellVisualiser(IRenderer renderer)
        {
            m_renderer = renderer;
            m_scene = m_renderer.GetNewScene();

            m_random = new Random();

            m_renderArrays = new RenderArrays3d();
            m_renderArrays.Positions = new List<Vector3d>();
            m_renderArrays.Colours = new List<Colour>();

            m_scene.CreateCamera();
            m_scene.SetCurrentCamera(0);

            m_scene.RenderArrays3d.Add(m_renderArrays);

            m_baseColours = new Colour[11];

            m_baseColours[00] = new Colour() { R = 1.0f, G = 0.0f, B = 0.0f, A = 0.0f };
            m_baseColours[01] = new Colour() { R = 0.0f, G = 1.0f, B = 0.0f, A = 0.0f };
            m_baseColours[02] = new Colour() { R = 0.0f, G = 0.0f, B = 1.0f, A = 0.0f };
            m_baseColours[03] = new Colour() { R = 1.0f, G = 1.0f, B = 0.0f, A = 0.0f };
            m_baseColours[04] = new Colour() { R = 0.0f, G = 1.0f, B = 1.0f, A = 0.0f };
            m_baseColours[05] = new Colour() { R = 1.0f, G = 0.5f, B = 0.5f, A = 0.0f };
            m_baseColours[06] = new Colour() { R = 0.5f, G = 1.0f, B = 0.5f, A = 0.0f };
            m_baseColours[07] = new Colour() { R = 0.5f, G = 0.5f, B = 1.0f, A = 0.0f };
            m_baseColours[08] = new Colour() { R = 1.0f, G = 1.0f, B = 0.5f, A = 0.0f };
            m_baseColours[09] = new Colour() { R = 0.5f, G = 1.0f, B = 1.0f, A = 0.0f };
            m_baseColours[10] = new Colour() { R = 1.0f, G = 0.5f, B = 1.0f, A = 0.0f };

        }

        public void Tick()
        {
        }
    }
}
