using CommonCore.Audio;
using CommonCore.RpgGame.Dialogue;
using CommonCore.State;
using System.Collections;
using UnityEngine;

namespace Whistler
{

    /// <summary>
    /// Initiates dialogue, optionally starts music, optionally sets quest stage and moves to a new scene
    /// </summary>
    public class InitiateDialogueScript : MonoBehaviour
    {
        [SerializeField]
        private string Dialogue = null;

        [SerializeField]
        private int QuestStageOnStart = 0;

        [SerializeField]
        private int QuestStageOnExit = 0;

        [SerializeField]
        private string Music = null;

        [SerializeField]
        private string NextScene = null;


        void Start()
        {
            if (!string.IsNullOrEmpty(Music))
            {
                AudioPlayer.Instance.SetMusic(Music, MusicSlot.Cinematic, 1.0f, true, false);
                AudioPlayer.Instance.StartMusic(MusicSlot.Cinematic);
            }

            if (QuestStageOnStart > 0)
                GameState.Instance.CampaignState.SetQuestStage("MainQuest", QuestStageOnStart);

            StartCoroutine(CoInitiateDialogue());
        }

        IEnumerator CoInitiateDialogue()
        {
            yield return null; //wait a frame because why not

            DialogueInitiator.InitiateDialogue(Dialogue, false, DialogueFinished);
        }

        private void DialogueFinished()
        {
            if(QuestStageOnExit > 0)
                GameState.Instance.CampaignState.SetQuestStage("MainQuest", QuestStageOnExit);

            if (!string.IsNullOrEmpty(NextScene))
                CommonCore.SharedUtils.ChangeScene(NextScene);
        }


    }
}