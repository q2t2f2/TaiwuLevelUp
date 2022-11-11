using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityGameUI;

namespace SXDZD
{
    public class ProgressBar : MonoBehaviour
    {
        private Image bar;
        private Image bg;

        private float progress;

        public float Progress
        {
            get => progress;
            set
            {
                progress = Mathf.Clamp(value, 0, 1);
                Debug.Log($"设置进度为：{progress}");
                //bar.transform.localScale = new Vector3(progress, 1, 1);
                bar.fillAmount = progress;
            }
        }

        public Color BarColor { get => bar.color; set => bar.color = value; }

        public Color BarBGColor { get => bg.color; set => bg.color = value; }
        public ProgressBar Init(string width, string height)
        {
            bg = UIControls.createUIPanel(gameObject, width, height).GetComponent<Image>();
            bg.gameObject.name = "bg";
            bg.GetComponent<Image>().color = Color.white;
            bar = UIControls.createUIPanel(gameObject, width, height).GetComponent<Image>();
            bar.gameObject.name = "bar";
            bar.GetComponent<Image>().color = Color.blue;
            bar.rectTransform.anchorMin = Vector2.zero;

            bar.sprite = Sprite.Create(new Texture2D(25, 1), new Rect(Vector2.zero, new Vector2(25, 1)), new Vector2(0.5f, 0.5f));

            bar.fillMethod = Image.FillMethod.Horizontal;
            bar.type = Image.Type.Filled;
            //bar.fillClockwise = true;

            Progress = 0;
            return this;
        }


    }
}