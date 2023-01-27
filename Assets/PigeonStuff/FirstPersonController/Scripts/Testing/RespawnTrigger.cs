using UnityEngine;

namespace Pigeon.Movement.Testing
{
    public class RespawnTrigger : MonoBehaviour
    {
        Vector3 startPos;

        void Awake()
        {
            var player = FindObjectOfType<PlayerMovement>();
            startPos = player ? player.transform.localPosition : Vector3.zero;
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerMovement player))
            {
                player.MovePosition(startPos);
            }
        }
    }
}