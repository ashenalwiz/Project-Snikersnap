using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TracingScripts
{
    public class SoundManager : MonoBehaviour
    {
        public static SoundManager Instance;
        public GameObject WriteFX;
        public AudioSource ClickFX;
        private float valBG, valFX;

        private void Awake()
        {
            Instance = this;
            if (PlayerPrefs.GetString("OpenAppAlready") == "")
            {
                PlayerPrefs.SetFloat("BackgroundMusicVolume", 1f);
                PlayerPrefs.SetFloat("FXMusicVolume", 1f);
                PlayerPrefs.SetString("OpenAppAlready", "yes");
                PlayerPrefs.Save();
            }
        }

        public void SetVolume()
        {
            PlayerPrefs.SetFloat("BackgroundMusicVolume", MainMenuHandler.Instance.BGM_Slider.value);
            PlayerPrefs.SetFloat("FXMusicVolume", MainMenuHandler.Instance.SFX_Slider.value);
            PlayerPrefs.Save();
        }

        public void SetupSlider()
        {
            valBG = PlayerPrefs.GetFloat("BackgroundMusicVolume");
            valFX = PlayerPrefs.GetFloat("FXMusicVolume");

            MainMenuHandler.Instance.BGM_Slider.value = valBG;
            MainMenuHandler.Instance.SFX_Slider.value = valFX;
        }
    }
}