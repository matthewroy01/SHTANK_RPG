﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// this code taken from Battle Beetles
// written by Matthew Roy

public class UtilityAudioManager : MonoBehaviour
{
    public int numberOfAudioSources;

	private List<AudioSource> audioSources = new List<AudioSource>();
    private List<ManagedAudio> queuedUpAudio = new List<ManagedAudio>();

	void Start()
	{
		for (int i = 0; i < numberOfAudioSources; i++)
		{
			audioSources.Add(gameObject.AddComponent<AudioSource>());
		}
	}

    private void LateUpdate()
    {
        PlayQueuedSounds();
    }

    private void PlayQueuedSounds()
    {
        // loop through queued up audio and play it
        for (int i = 0; i < queuedUpAudio.Count; ++i)
        {
            ManagedAudio tmp = queuedUpAudio[i];

            // find an available audio source and use it
            for (int j = 0; j < audioSources.Count; j++)
            {
                if (audioSources[i].isPlaying == false)
                {
                    audioSources[i].clip = tmp.clip;
                    audioSources[i].pitch = tmp.pitch;
                    audioSources[i].volume = tmp.volume;
                    audioSources[i].Play();
                }
            }
        }

        queuedUpAudio.Clear();
    }

    public void QueueSound(ManagedAudio newAudio)
    {
        // if the same sound is going to be queued up more than once per frame, don't queue it up
        for (int i = 0; i < queuedUpAudio.Count; ++i)
        {
            if (queuedUpAudio[i].clip == newAudio.clip)
            {
                return;
            }
        }

        newAudio.pitch = Random.Range(Mathf.Clamp(newAudio.randomPitchMinMax.x, 0.0f, 1.0f), Mathf.Clamp(newAudio.randomPitchMinMax.y, 0.0f, 1.0f));
        queuedUpAudio.Add(newAudio);
    }
}

[System.Serializable]
public class ManagedAudio
{
    public AudioClip clip;
    [Range(0.0f, 1.0f)]
    public float volume;
    [HideInInspector]
    public float pitch;
    public Vector2 randomPitchMinMax;

    public ManagedAudio(AudioClip c, float v, Vector2 rp)
    {
        clip = c;
        volume = v;
        randomPitchMinMax = rp;
    }
}