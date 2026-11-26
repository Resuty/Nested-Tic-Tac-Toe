using UnityEngine;

namespace WellWithinWell.Presentation
{
    public sealed class AudioController : MonoBehaviour
    {
        private const string VolumeKey = "WellWithinWell.Volume";
        private const string MuteKey = "WellWithinWell.Mute";

        [SerializeField] private AudioSource source;
        [SerializeField] private AudioClip button;
        [SerializeField] private AudioClip stone;
        [SerializeField] private AudioClip claim;
        [SerializeField] private AudioClip end;

        public float Volume { get; private set; }
        public bool Muted { get; private set; }

        private void Awake()
        {
            Volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 0.72f));
            Muted = PlayerPrefs.GetInt(MuteKey, 0) != 0;
            Apply();
        }

        public void SetVolume(float value)
        {
            Volume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(VolumeKey, Volume);
            Apply();
        }

        public void SetMuted(bool value)
        {
            Muted = value;
            PlayerPrefs.SetInt(MuteKey, Muted ? 1 : 0);
            Apply();
        }

        public void Save() => PlayerPrefs.Save();
        private void Apply()
        {
            source.volume = Volume;
            source.mute = Muted;
        }

        public void Button() => source.PlayOneShot(button, 0.53f);
        public void Stone() => source.PlayOneShot(stone, 0.9f);
        public void Claim() => source.PlayOneShot(claim, 0.72f);
        public void End() => source.PlayOneShot(end, 0.78f);

        public void Wire(AudioSource output, AudioClip navigation, AudioClip place, AudioClip regional, AudioClip final)
        {
            source = output;
            button = navigation;
            stone = place;
            claim = regional;
            end = final;
        }
    }
}
