using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Lucidity
{

    /// <summary>
    /// Hacky script that fades an image in, then out
    /// </summary>
    public class FadeInOutHack : MonoBehaviour
    {
        [SerializeField]
        private float FadeInTime = 0.5f;
        [SerializeField]
        private float FadeOutTime = 0.5f;
        [SerializeField]
        private float TargetAlpha = 0.5f;
        [SerializeField]
        private Image Image;

        private float OriginalAlpha;
        private bool ReachedTarget = false;
        private float Elapsed = 0;

        private void Start()
        {
            if (Image == null)
                Image = GetComponentInChildren<Image>();

            OriginalAlpha = Image.color.a;
        }

        private void Update()
        {
            if(!ReachedTarget)
            {
                //lerp toward
                float ratio = Elapsed / FadeInTime;
                float a = Mathf.Lerp(OriginalAlpha, TargetAlpha, ratio);

                Image.color = new Color(Image.color.r, Image.color.g, Image.color.b, a);

                if(Mathf.Approximately(Image.color.a, TargetAlpha))
                {
                    ReachedTarget = true;
                    Elapsed = 0;
                }
                
            }
            else
            {
                //lerp away
                float ratio = Elapsed / FadeOutTime;
                float a = Mathf.Lerp(TargetAlpha, OriginalAlpha, ratio);

                Image.color = new Color(Image.color.r, Image.color.g, Image.color.b, a);

                if (Mathf.Approximately(Image.color.a, OriginalAlpha))
                {
                    Destroy(this);
                }
            }

            Elapsed += Time.deltaTime;
        }
    }
}