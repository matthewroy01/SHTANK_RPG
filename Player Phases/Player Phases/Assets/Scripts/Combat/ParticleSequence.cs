using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSequence : MonoBehaviour
{
    public List<ParticleSystem> systems = new List<ParticleSystem>();
    private Coroutine playCoroutine;

    public void Play()
    {
        StopAll();

        if (playCoroutine != null)
        {
            StopCoroutine(playCoroutine);
        }

        playCoroutine = StartCoroutine(PlayCoroutine());
    }

    private void StopAll()
    {
        for (int i = 0; i < systems.Count; ++i)
        {
            systems[i].Stop();
        }
    }

    private IEnumerator PlayCoroutine()
    {
        for (int i = 0; i < systems.Count; ++i)
        {
            systems[i].Play();

            yield return new WaitForSecondsRealtime(systems[i].main.startLifetime.constant);
        }
    }
}