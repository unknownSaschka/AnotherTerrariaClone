using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Logic
{
    public class InputManager
    {
        KeyboardState lastKeyboardState;
        KeyboardState currentKeyboardState;
        MouseState lastMouseState;
        MouseState currentMouseState;

        public InputManager()
        {
            
        }

        public void Update(KeyboardState newKeyboardState, MouseState newMouseState)
        {
            lastKeyboardState = currentKeyboardState;
            currentKeyboardState = newKeyboardState;

            lastMouseState = currentMouseState;
            currentMouseState = newMouseState;
        }

        public bool GetKeyPressed(Key key)
        {
            if (currentKeyboardState.IsKeyDown(key) && !lastKeyboardState.IsKeyDown(key)) return true;
            else return false;
        }

        public int GetMouseWheelDifference()
        {
            return currentMouseState.ScrollWheelValue - lastMouseState.ScrollWheelValue;
        }
    }
}
