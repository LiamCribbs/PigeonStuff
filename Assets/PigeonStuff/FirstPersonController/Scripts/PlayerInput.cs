using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Movement
{
    public class PlayerInput : MonoBehaviour
    {
        public PlayerControls Controls { get; private set; }

        void Awake()
        {
            Controls = new PlayerControls();
            Controls.Enable();
        }

        void OnDestroy()
        {
            Controls.Disable();
            Controls.Dispose();
        }
    }
}