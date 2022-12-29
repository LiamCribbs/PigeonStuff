using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pigeon.Movement
{
    public class PlayerInput : MonoBehaviour
    {
        public static PlayerInput Instance { get; private set; }

        public PlayerControls Controls { get; private set; }

        void Awake()
        {
            Instance = this;

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