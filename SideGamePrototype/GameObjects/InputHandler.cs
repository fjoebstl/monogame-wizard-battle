using Microsoft.Xna.Framework.Input;

namespace SideGamePrototype
{
    public interface IInputHandler
    {
        bool FirePressed { get; }
        bool JumpPressed { get; }
        bool LeftPressed { get; }
        bool RightPressed { get; }
        bool CrouchPressed { get; }
    }

    public class KeyboardLayout1InputHandler : IInputHandler
    {
        public bool LeftPressed => Keyboard.GetState().IsKeyDown(Keys.Left);
        public bool RightPressed => Keyboard.GetState().IsKeyDown(Keys.Right);
        public bool JumpPressed => Keyboard.GetState().IsKeyDown(Keys.Up);
        public bool FirePressed => Keyboard.GetState().IsKeyDown(Keys.Space);
        public bool CrouchPressed => Keyboard.GetState().IsKeyDown(Keys.Down);
    }
}