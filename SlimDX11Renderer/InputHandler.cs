using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SlimDX.RawInput;

namespace SlimDX11Renderer
{
    public class InputHandler
    {
        Dictionary<System.Windows.Forms.Keys, bool> keyboardState_;        
        public InputHandler()
        {
            keyboardState_ = new Dictionary<System.Windows.Forms.Keys, bool>();
            foreach (System.Windows.Forms.Keys k in Enum.GetValues(typeof(System.Windows.Forms.Keys)))
            {
                if (!keyboardState_.ContainsKey(k))
                {
                    keyboardState_.Add(k, false);
                }
            }
            Device.RegisterDevice(SlimDX.Multimedia.UsagePage.Generic, SlimDX.Multimedia.UsageId.Keyboard, DeviceFlags.None);

            Device.KeyboardInput += new EventHandler<KeyboardInputEventArgs>(OnKeyboardInput);
        }
        public bool Query(System.Windows.Forms.Keys k)
        {
            return keyboardState_[k];
        }
        void OnKeyboardInput(object sender, KeyboardInputEventArgs e) 
        { 
            switch (e.State) 
            { 
                case KeyState.Pressed:
                    keyboardState_[e.Key] = true;
                    break;
                case KeyState.Released: 
                    keyboardState_[e.Key] = false;
                    break;
            }
        }

    }
}
