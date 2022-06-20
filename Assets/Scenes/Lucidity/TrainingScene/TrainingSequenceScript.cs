using CommonCore;
using CommonCore.Audio;
using CommonCore.Messaging;
using CommonCore.RpgGame.Dialogue;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.World;
using CommonCore.State;
using CommonCore.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Lucidity.WarBattleScene
{

    //TODO music

    /// <summary>
    /// Script for the training scene sequence
    /// </summary>
    public class TrainingSequenceScript : MonoBehaviour
    {
        [SerializeField, Header("References")]
        private ActorController[] Dummies = null;

        [SerializeField, Header("Options")]
        private float MaxTime = 300f; //300 seconds = 5 minutes
        [SerializeField]
        private float AfterKillTargetTime = 30f; //30 seconds for now

        private bool SequenceStarted = false;
        private bool SequenceEnding = false;
        private float TimeInScene = 0;
        private float TimeAfterKills = 0;
        private Coroutine CurrentCoroutine = null;

        private void Awake()
        {
            //IMMEDIATELY inject flags

            //GameState.Instance.PlayerFlags.Add(PlayerFlags.Invulnerable);
            GameState.Instance.PlayerFlags.Add(PlayerFlags.NoWeapons); //technically, we're unarmed
            //GameState.Instance.PlayerFlags.Add(PlayerFlags.HideHud);
            GameState.Instance.PlayerFlags.Remove(PlayerFlags.HideHud); //because it'll be fuxxored
            GameState.Instance.PlayerFlags.Add(PlayerFlags.NoFallDamage);
            GameState.Instance.CampaignState.AddFlag("BriellaIsAMagicalGirl"); //yes this is actually what the flag is called

        }

        private void Start()
        {
            //fuck it
            CurrentCoroutine = StartCoroutine(CoSequenceStart());
        }

        private IEnumerator CoSequenceStart()
        {
            AudioPlayer.Instance.SetMusic("war1", MusicSlot.Event, 0.66f, true, false); //reuse tension1 for now
            AudioPlayer.Instance.StartMusic(MusicSlot.Event);

            //start dialogue
            bool doContinue = false;
            DialogueInitiator.InitiateDialogue("TrainingBefore", true, () => {
                doContinue = true;
            });
            //SetBackgroundImage(null);
            while (!doContinue)
            {
                yield return null;
            }

            SequenceStarted = true;
            QdmsMessageBus.Instance.PushBroadcast(new SubtitleMessage("Try out your new powers!", 10.0f));

            AudioPlayer.Instance.StopMusic(MusicSlot.Event);
            AudioPlayer.Instance.SetMusic("action1", MusicSlot.Ambient, 1.0f, true, false); //reuse tension1 for now
            AudioPlayer.Instance.StartMusic(MusicSlot.Ambient);
        }

        private void Update()
        {
            if (!SequenceStarted)
                return;

            if (!SequenceEnding)
            {
                //check ending conditions
                if (TimeInScene >= MaxTime)
                {
                    Debug.Log("Timer has run out!");
                    StartSequenceEnd();
                }
                else if(AreAllDummiesDead)
                {
                    //Debug.Log("All dummies dead!");
                    TimeAfterKills += Time.deltaTime;
                    if (TimeAfterKills >= AfterKillTargetTime)
                        StartSequenceEnd();
                }

                TimeInScene += Time.deltaTime;
            }
        }

        private void StartSequenceEnd()
        {
            SequenceEnding = true;

            //start the ending sequence
            CurrentCoroutine = StartCoroutine(CoSequenceEnd());
        }

        private IEnumerator CoSequenceEnd()
        {
            yield return null;

            AudioPlayer.Instance.StopMusic(MusicSlot.Ambient);
            AudioPlayer.Instance.SetMusic("war1", MusicSlot.Event, 0.66f, true, false); //reuse tension1 for now
            AudioPlayer.Instance.StartMusic(MusicSlot.Event);

            //start dialogue
            bool doContinue = false;
            DialogueInitiator.InitiateDialogue("TrainingAfter", true, () => {
                doContinue = true;
            });
            //SetBackgroundImage(null);
            while (!doContinue)
            {
                yield return null;
            }

            GameState.Instance.CampaignState.SetQuestStage("MainQuest", 210);
            SharedUtils.ChangeScene("UnleashedVillageScene");
        }

        private bool AreAllDummiesDead { get {

                int deadDummies = 0;
                foreach(var dummy in Dummies)
                {
                    if (dummy.CurrentAiState == ActorAiState.Dead)
                        deadDummies++;
                }

                return deadDummies >= Dummies.Length;

            } }

        //so, we want to give some hints, but probably won't get to it

        //we will end the sequence under a few conditions:
        //-all targets hit + a short (<1 min) delay
        //-all hidden locations reached  + a moderate (1-2 min) delay (?)
        //-player has accomplished nothing after 5 minutes or so
    }
}