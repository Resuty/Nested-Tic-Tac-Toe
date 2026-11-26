using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace WellWithinWell.Presentation
{
    public sealed class LineView : MonoBehaviour
    {
        [SerializeField] private Image ink;
        [SerializeField] private Image shadow;
        private Coroutine reveal;

        public void Show(Vector2 from, Vector2 to, Color color, float width, bool animate)
        {
            RectTransform line = (RectTransform)transform;
            Vector2 path = to - from;
            line.anchoredPosition = from;
            line.localEulerAngles = new Vector3(0, 0, Mathf.Atan2(path.y, path.x) * Mathf.Rad2Deg);
            line.sizeDelta = new Vector2(path.magnitude, width);
            ink.color = color;
            ink.enabled = true;
            shadow.enabled = true;
            if (reveal != null)
                StopCoroutine(reveal);
            line.localScale = Vector3.one;
            reveal = animate && isActiveAndEnabled ? StartCoroutine(Reveal(line)) : null;
        }

        public void Hide()
        {
            if (reveal != null)
                StopCoroutine(reveal);
            reveal = null;
            ink.enabled = false;
            shadow.enabled = false;
        }

        private void OnDisable() => reveal = null;

        private IEnumerator Reveal(RectTransform line)
        {
            const float duration = 0.29f;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                float progress = 1 - Mathf.Pow(1 - t / duration, 3);
                line.localScale = new Vector3(progress, 1, 1);
                yield return null;
            }
            line.localScale = Vector3.one;
            reveal = null;
        }

        public void Wire(Image lineInk, Image lineShadow)
        {
            ink = lineInk;
            shadow = lineShadow;
        }
    }
}
