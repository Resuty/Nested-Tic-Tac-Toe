using UnityEngine;
using UnityEngine.UI;

namespace WellWithinWell.Presentation
{
    public sealed class AcceptButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private Text caption;

        public Button Button => button;
        public string Caption { set => caption.text = value; }

        public void Wire(Button control, Text title)
        {
            button = control;
            caption = title;
        }
    }
}
