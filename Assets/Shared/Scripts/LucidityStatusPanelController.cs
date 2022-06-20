using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.UI;
using UnityEngine.UI;
using CommonCore;
using CommonCore.State;

namespace Lucidity
{

    /// <summary>
    /// specialized controller for the Status panel in Lucidity
    /// </summary>
    public class LucidityStatusPanelController : PanelController
    {
        public RawImage CharacterImage;

        public override void SignalPaint()
        {
            base.SignalPaint();

            string rid = GameState.Instance.CampaignState.HasFlag("BriellaIsAMagicalGirl") ? "portrait_magic" : "portrait_normal";
            CharacterImage.texture = CoreUtils.LoadResource<Texture2D>("UI/Portraits/" + rid);
        }
    }
}