using System.Collections;
using UnityEngine;

public class RoomEntryTrigger : MonoBehaviour
{
    public NPC_Controller npc;
    private bool triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (other.CompareTag("Player"))
        {
            triggered = true;
            npc.OnPlayerEnterRoom();
        }
    }
}