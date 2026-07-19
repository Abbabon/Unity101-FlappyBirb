using System.Collections;
using UnityEngine;

/// Plays the five classic one-shot sounds through a single AudioSource.
public class AudioManager : MonoBehaviour
{
    public AudioSource source;
    public AudioSource musicSource;
    public AudioClip wing;
    public AudioClip point;
    public AudioClip hit;
    public AudioClip die;
    public AudioClip swoosh;
    public AudioClip music;

    void Start()
    {
        if (music != null && musicSource != null)
        {
            musicSource.clip = music;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlayWing() => Play(wing);
    public void PlayPoint() => Play(point);
    public void PlayHit() => Play(hit);
    public void PlaySwoosh() => Play(swoosh);
    public void PlayDie(float delay) => StartCoroutine(Delayed(die, delay));

    void Play(AudioClip clip)
    {
        if (clip != null && source != null)
            source.PlayOneShot(clip);
    }

    IEnumerator Delayed(AudioClip clip, float delay)
    {
        yield return new WaitForSeconds(delay);
        Play(clip);
    }
}
