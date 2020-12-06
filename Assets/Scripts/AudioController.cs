using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    AudioSource audioSource;
    public AudioClip lossClip;
    public AudioClip winClip;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void ChangeMusic(int musicCheck)
    {
        if (musicCheck == 1)
        {
            //audioSource.Stop();
            audioSource.clip = winClip;
            audioSource.Play();
        }
        else if (musicCheck == 2)
        {
            //audioSource.Stop();
            audioSource.clip = lossClip;
            audioSource.Play();
        }
    }
}
