using CommonCore.Audio;
using CommonCore.RpgGame.Rpg;
using CommonCore.State;
using CommonCore.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore;
using CommonCore.World;
using CommonCore.RpgGame.World;

namespace Lucidity.DanceCastleScene
{
    /// <summary>
    /// Controller for the castle sequence
    /// </summary>
    public class DanceCastleSequenceScript : MonoBehaviour
    {
        [SerializeField]
        private Vector3 FlingVector = new Vector3(0, 20f, 60f);
        [SerializeField]
        private float FlingGroundHeight = 0.2f;
        [SerializeField]
        private GameObject Blockers = null;
        [SerializeField]
        private ActorController LarActor = null;

        private QdmsMessageInterface MessageInterface;

        private bool SequenceEnding = false;

        private void Awake()
        {
            //we listen for messages
            MessageInterface = new QdmsMessageInterface(this.gameObject);
            MessageInterface.SubscribeReceiver(HandleMessage);

            //IMMEDIATELY inject flags

            //note: most of these aren't needed in the final game but are necessary for single-scene testing

            //GameState.Instance.PlayerFlags.Add(PlayerFlags.Invulnerable);
            GameState.Instance.PlayerFlags.Add(PlayerFlags.NoWeapons); //technically, we're unarmed
            //GameState.Instance.PlayerFlags.Add(PlayerFlags.HideHud);
            GameState.Instance.PlayerFlags.Remove(PlayerFlags.HideHud);
            GameState.Instance.PlayerFlags.Remove(PlayerFlags.Invulnerable);
            GameState.Instance.PlayerFlags.Add(PlayerFlags.NoFallDamage);
            GameState.Instance.CampaignState.AddFlag("BriellaIsAMagicalGirl"); //yes this is actually what the flag is called

        }

        private void Start()
        {
            //start the sequence start
            StartCoroutine(CoSequenceStart());
        }

        private IEnumerator CoSequenceStart()
        {
            //music
            AudioPlayer.Instance.SetMusic("action4", MusicSlot.Ambient, 0.75f, true, false);
            AudioPlayer.Instance.StartMusic(MusicSlot.Ambient);

            //WIP the fling
            GameState.Instance.PlayerFlags.Add(PlayerFlags.Frozen);
            Blockers.SetActive(false);
            var pmc = WorldUtils.GetPlayerObject().GetComponentInChildren<PlayerMovementComponent>();

            pmc.Velocity += pmc.transform.forward * FlingVector.z + pmc.transform.up * FlingVector.y;

            //wait for player to hit the ground
            while(pmc.transform.position.y > FlingGroundHeight)
            {
                yield return null;
            }

            //kill Lar, activate blockers, and unlock
            LarActor.TakeDamage(new ActorHitInfo(10000f, 10000f, 0, 0, 0, null));
            Blockers.SetActive(true);
            pmc.Velocity = Vector3.zero;
            AudioPlayer.Instance.PlaySound("GroundImpact", SoundType.Sound, false);
            AudioPlayer.Instance.PlaySound("LarImpact", SoundType.Sound, false);
            WorldUtils.SpawnEffect("GroundImpact", pmc.transform.position, Vector3.zero, CoreUtils.GetWorldRoot());
            yield return null;
            GameState.Instance.PlayerFlags.Remove(PlayerFlags.Frozen);
        }

        private void HandleMessage(QdmsMessage message)
        {
            if(message is QdmsFlagMessage flagMessage && flagMessage.Flag == "CastleExitLevel")
            {
                //exit level

                //make a nice sequence lol
                if(!SequenceEnding)
                    StartCoroutine(CoSequenceEnd());
                
            }
        }

        private IEnumerator CoSequenceEnd()
        {
            SequenceEnding = true;

            //fade out and exit

            GameState.Instance.PlayerFlags.Add(PlayerFlags.Frozen);
            GameState.Instance.PlayerFlags.Add(PlayerFlags.Invulnerable);
            GameState.Instance.PlayerFlags.Add(PlayerFlags.NoTarget);

            AudioPlayer.Instance.PlaySound("DoorWood", SoundType.Sound, true);
            ScreenFader.FadeTo(Color.black, 1.5f, true, true, false);
            yield return new WaitForSecondsRealtime(1.5f);

            GameState.Instance.PlayerFlags.Remove(PlayerFlags.Frozen);
            GameState.Instance.PlayerFlags.Remove(PlayerFlags.Invulnerable);
            GameState.Instance.PlayerFlags.Remove(PlayerFlags.NoTarget);

            GameState.Instance.CampaignState.SetQuestStage("MainQuest", 410);

            SharedUtils.ChangeScene("DanceConfrontScene");

        }
    }
}