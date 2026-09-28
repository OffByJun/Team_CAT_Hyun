using System;
using _001_Scripts.Manager.Base;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _001_Scripts.Manager
{
    public class InputManager : SinManagerBase<InputManager>
    {
        public KeyCode Jump = KeyCode.Space;
        public KeyCode Dash = KeyCode.LeftShift;
        public KeyCode Left = KeyCode.A;
        public KeyCode Right = KeyCode.D;
        public KeyCode ESC = KeyCode.Escape;

        public event Action<Vector2> Movement;
        public event Action Jumping;

        public void OnMove(InputAction.CallbackContext ctx)
        {
            Vector2 value = ctx.ReadValue<Vector2>();
            Movement?.Invoke(value);
        }

        public void OnJump(InputAction.CallbackContext ctx)
        {
            if (ctx.started)
            {
                Jumping.Invoke();
            }
        }
    }
}