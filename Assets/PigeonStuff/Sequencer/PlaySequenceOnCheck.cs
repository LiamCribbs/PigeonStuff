using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pigeon.Sequencer;

public class PlaySequenceOnCheck : MonoBehaviour
{
    [SerializeField] bool play;

    void Update()
    {
        if (play)
        {
            play = false;
            GetComponent<SequencePlayer>().Play();
        }
    }
}