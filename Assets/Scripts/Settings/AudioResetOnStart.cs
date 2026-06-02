using UnityEngine;
using UnityEngine.Audio;

namespace Project.Audio
{
    public class AudioResetOnStart : MonoBehaviour
    {
        [Header("[ НАСТРОЙКИ МИКШЕРА ]")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private string gameVolumeParam = "GameVolume";
        [SerializeField] private string musicVolumeParam = "MusicVolume";
        [SerializeField] private string sfxVolumeParam = "SFXVolume";

        private void Awake()
        {
            ResetAllVolumes();
        }

        private void Start()
        {
            ResetAllVolumes();
            RestartGameMusic();
        }

        private void ResetAllVolumes()
        {
            if (audioMixer != null)
            {
                audioMixer.SetFloat(gameVolumeParam, 0f);
                audioMixer.SetFloat(musicVolumeParam, 0f);
                audioMixer.SetFloat(sfxVolumeParam, 0f);
            }
        }

        private void RestartGameMusic()
        {
            GameObject audioManagerGO = GameObject.Find("AudioManager");
            
            if (audioManagerGO != null)
            {
                AudioSource[] sources = audioManagerGO.GetComponentsInChildren<AudioSource>();
                
                foreach (AudioSource source in sources)
                {
                    if (source.loop && !source.isPlaying)
                    {
                        source.Play();
                    }
                }
            }
        }
    }
}