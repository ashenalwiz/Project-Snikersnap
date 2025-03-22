using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TracingScripts
{
    public class AudioHandler : MonoBehaviour
    {
        [SerializeField] private bool isBGM;

        private void LateUpdate()
        {
            if (isBGM)
            {
                this.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("BackgroundMusicVolume");
            }
            else
            {
                this.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("FXMusicVolume");
            }
        }
    }
}
