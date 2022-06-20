using CommonCore.Messaging;
using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucidity.DanceCastleScene
{

    public class BossDoorScript : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"Boss door entered by {other.name}");

            if(WorldUtils.IsPlayer(other.gameObject))
            {
                QdmsMessageBus.Instance.PushBroadcast(new QdmsFlagMessage("CastleExitLevel"));
            }
        }
    }
}