using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucidity.DanceConfrontScene
{

    /// <summary>
    /// Teleports garrick back into the arena if you fus ro dah him out of the map
    /// </summary>
    public class DanceEmergencyTeleportScript : MonoBehaviour
    {
        [SerializeField]
        private Transform RespawnPoint = null;

        private void OnTriggerExit(Collider other)
        {
            Debug.Log($"{other.name} exited boundaries");

            var bc = other.GetComponent<BaseController>();
            if(bc != null)
            {
                other.transform.position = RespawnPoint.position;
                other.transform.rotation = RespawnPoint.rotation;
            }
        }
    }
}