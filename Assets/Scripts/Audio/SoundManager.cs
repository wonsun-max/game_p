using UnityEngine;
using System.Collections;

namespace PrismPulse.Audio
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance { get; private set; }

        private AudioSource sfxSource;
        private AudioSource musicSource;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SetupSources();
                StartMusic();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void SetupSources()
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.25f;
        }

        public void PlayClick() { PlayTone(440, 0.05f, 0.2f); }
        public void PlayPop() { PlayTone(660, 0.1f, 0.3f); }
        public void PlayPlace() { PlayTone(220, 0.15f, 0.4f); }
        public void PlayLineClear() { StartCoroutine(LineClearSequence()); }
        public void PlayBoardClear() { StartCoroutine(VictorySequence()); }

        private void PlayTone(float frequency, float duration, float volume)
        {
            AudioClip clip = AudioClip.Create("Tone", (int)(44100 * duration), 1, 44100, false);
            float[] data = new float[(int)(44100 * duration)];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = Mathf.Sin(2 * Mathf.PI * frequency * i / 44100) * (1f - (float)i / data.Length);
            }
            clip.SetData(data, 0);
            sfxSource.PlayOneShot(clip, volume);
        }

        private IEnumerator LineClearSequence()
        {
            PlayTone(523.25f, 0.1f, 0.3f); yield return new WaitForSeconds(0.05f);
            PlayTone(659.25f, 0.1f, 0.3f); yield return new WaitForSeconds(0.05f);
            PlayTone(783.99f, 0.2f, 0.3f);
        }

        private IEnumerator VictorySequence()
        {
            for (int i = 0; i < 5; i++)
            {
                PlayTone(400 + (i * 100), 0.1f, 0.4f);
                yield return new WaitForSeconds(0.1f);
            }
        }

        public void StartMusic()
        {
            if (musicSource.isPlaying) return;
            int length = 44100 * 8;
            AudioClip musicClip = AudioClip.Create("Ambient_Pro", length, 1, 44100, false);
            float[] data = new float[length];
            for (int i = 0; i < data.Length; i++)
            {
                float t = (float)i / 44100;
                float wave1 = Mathf.Sin(2 * Mathf.PI * 110 * t);
                float wave2 = Mathf.Sin(2 * Mathf.PI * 110.5f * t);
                float lfo = 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * 0.2f * t);
                data[i] = (wave1 + wave2) * 0.05f * lfo;
                if (i < 44100) data[i] *= (float)i / 44100;
                if (i > length - 44100) data[i] *= (float)(length - i) / 44100;
            }
            musicClip.SetData(data, 0);
            musicSource.clip = musicClip;
            musicSource.Play();
        }
    }
}
