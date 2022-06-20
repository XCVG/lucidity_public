using CommonCore;
using CommonCore.Audio;
using CommonCore.RpgGame.Dialogue;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.World;
using CommonCore.State;
using CommonCore.World;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucidity.UnleashedVillageScene
{
    public class UnleashedVillageSequenceScript : MonoBehaviour
    {
        [SerializeField]
        private float AfterKillTargetTime = 30f; //hold for this long after all bandits are dead

        private List<BaseController> Bandits = new List<BaseController>();
        private bool SequenceEnding = false;
        private float TimeAfterKills = 0;


        private void Awake()
        {
            //IMMEDIATELY inject flags

            //GameState.Instance.PlayerFlags.Add(PlayerFlags.Invulnerable);
            GameState.Instance.PlayerFlags.Add(PlayerFlags.NoWeapons); //technically, we're unarmed
            //GameState.Instance.PlayerFlags.Add(PlayerFlags.HideHud);
            GameState.Instance.PlayerFlags.Remove(PlayerFlags.HideHud); //because it'll be fuxxored
            GameState.Instance.PlayerFlags.Remove(PlayerFlags.Invulnerable);
            GameState.Instance.PlayerFlags.Add(PlayerFlags.NoFallDamage);
            GameState.Instance.CampaignState.AddFlag("BriellaIsAMagicalGirl"); //yes this is actually what the flag is called

        }

        private void Start()
        {
            //start music
            AudioPlayer.Instance.SetMusic("action2", MusicSlot.Ambient, 0.75f, true, false);
            AudioPlayer.Instance.StartMusic(MusicSlot.Ambient);

            //DO NOT SPAWN EXTRA BANDITS DURING GAMEPLAY
            var bandits = WorldUtils.FindEntitiesWithTag("Bandit");
            Bandits.AddRange(bandits);
            Debug.Log($"Found {bandits?.Count.ToString() ?? "null"} bandits");

        }

        private void Update()
        {
            if (SequenceEnding)
                return;

            //fuck efficiency, get a faster CPU
            if(AreAllBanditsDead)
            {
                TimeAfterKills += Time.deltaTime;
                if(TimeAfterKills >= AfterKillTargetTime)
                {
                    StartSequenceEnd();
                }
            }
        }

        private void StartSequenceEnd()
        {
            Debug.Log("Starting end of sequence");

            SequenceEnding = true;

            //start the ending sequence
            StartCoroutine(CoSequenceEnd());
        }

        private IEnumerator CoSequenceEnd()
        {
            yield return null;

            ScreenFader.FadeTo(Color.black, 5.0f, false, false, false);
            yield return new WaitForSeconds(5.0f);

            //TODO stop music, start new music
            AudioPlayer.Instance.StopMusic(MusicSlot.Ambient);
            AudioPlayer.Instance.SetMusic("sadness3", MusicSlot.Event, 0.5f, true, false);
            AudioPlayer.Instance.StartMusic(MusicSlot.Event);

            //start dialogue
            bool doContinue = false;
            DialogueInitiator.InitiateDialogue("UnleashedReflect", true, () => {
                doContinue = true;
            });
            //SetBackgroundImage(null);
            while (!doContinue)
            {
                yield return null;
            }

            GameState.Instance.CampaignState.SetQuestStage("MainQuest", 260);
            GameState.Instance.PlayerFlags.Add(PlayerFlags.HideHud); //fix flag on exit
            SharedUtils.ChangeScene("UnleashedGarrickScene");
        }


        private bool AreAllBanditsDead { get {

                int actualBandits = 0;
                int deadBandits = 0;
                foreach(var bandit in Bandits)
                {
                    var ac = bandit as ActorController;
                    if(ac != null)
                    {
                        actualBandits++;
                        if (ac.CurrentAiState == ActorAiState.Dead)
                            deadBandits++;
                    }
                }

                return deadBandits >= actualBandits;

            } }
    }
}