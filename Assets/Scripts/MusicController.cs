﻿using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MusicController : MonoBehaviour
{
    //public AudioSource agitato;
    //public AudioSource smooth;
    //public AudioSource leggiero;
    //public AudioSource calm;
    public GameObject majorMelodyGO;
    public GameObject majorCountermelodyGO;
    public GameObject majorAccompanimentGO;
    public GameObject minorMelodyGO;
    public GameObject minorCountermelodyGO;
    public GameObject minorAccompanimentGO;
    private AudioSource[] majorMelodies;
    private AudioSource[] majorCountermelodies;
    private AudioSource[] majorAccompaniment;
    private AudioSource[] minorMelodies;
    private AudioSource[] minorCountermelodies;
    private AudioSource[] minorAccompaniment;
    public Image overlayImage;
    public float transitionTime;
    public float fadeOutTime;
    public float fadeInTime;
    private Arrangement currentArrangement;
    private Arrangement oldArrangement;
    [System.Serializable]
    public struct Arrangement
    {
        public AudioSource[] melody;
        public AudioSource[] countermelody;
        public AudioSource[] accompaniment;
    }
    public Arrangement[] stage1Arrangements;
    public Arrangement[] stage2Arrangements;
    public Arrangement[] stage3Arrangements;
    public Arrangement stage1Selection;
    public Arrangement stage2Selection;
    public Arrangement stage3Selection;
    // Use this for initialization
    void Start()
    {
        StartCoroutine(IntroFade());

        majorMelodies = majorMelodyGO.GetComponentsInChildren<AudioSource>();
        majorCountermelodies = majorCountermelodyGO.GetComponentsInChildren<AudioSource>();
        majorAccompaniment = majorAccompanimentGO.GetComponentsInChildren<AudioSource>();
        minorMelodies = minorMelodyGO.GetComponentsInChildren<AudioSource>();
        minorCountermelodies = minorCountermelodyGO.GetComponentsInChildren<AudioSource>();
        minorAccompaniment = minorAccompanimentGO.GetComponentsInChildren<AudioSource>();
        //stage1Selection = stage1Arrangements[UnityEngine.Random.Range(0, stage1Arrangements.Length)];
        //stage2Selection = stage2Arrangements[UnityEngine.Random.Range(0, stage2Arrangements.Length)];
        //stage3Selection = stage3Arrangements[UnityEngine.Random.Range(0, stage3Arrangements.Length)];
    }

    // Update is called once per frame
    void Update()
    {
        if (!oldArrangement.Equals(currentArrangement))
        {
            StopArrangement(stage3Selection, 3);
            Debug.Log("Arrangement Changed");
        }
        oldArrangement = currentArrangement;
        int a;
        if (Mathf.Abs(ControllerInputHandler.instance.speed - 0.5f) < 0.167f)
        {
            currentArrangement = stage1Selection;
            a = 1;
        }
        else if (Mathf.Abs(ControllerInputHandler.instance.speed - 0.5f) < 0.333f)
        {
            currentArrangement = stage2Selection;
            a = 2;
        }
        else
        {
            currentArrangement = stage3Selection;
            a = 3;
        }
        PlayArrangement(currentArrangement, a);
    }

    IEnumerator FadeOut(AudioSource audioSource, float time)
    {
        float timer = 0;
        float initialVolume = audioSource.volume;
        while (timer < time)
        {
            timer += Time.deltaTime;
            audioSource.volume = 1 - timer / time;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        audioSource.volume = 0f;
    }
    IEnumerator FadeIn(AudioSource audioSource, float time)
    {
        float timer = 0;
        audioSource.volume = 0;
        while (timer < time)
        {
            timer += Time.deltaTime;
            audioSource.volume = timer / time;
            yield return new WaitForSeconds(Time.deltaTime);
        }
        audioSource.volume = 1f;
    }

    public void PlayArrangement(Arrangement arrangement, int a)
    {
        UpdateArray(arrangement.melody, a);
        UpdateArray(arrangement.countermelody, a);
        UpdateArray(arrangement.accompaniment, a);
    }
    public void StopArrangement(Arrangement arrangement, int a)
    {
        UpdateArray(arrangement.melody, a, true);
        UpdateArray(arrangement.countermelody, a, true);
        UpdateArray(arrangement.accompaniment, a, true);
    }

    void UpdateArray(AudioSource[] audioSources, int arrangement, bool stop = false)
    {
        if (audioSources != null)
        {
            foreach (AudioSource audioSource in audioSources)
            {
                if (audioSource)
                {
                    if (stop && audioSource.volume == 1 && BinaryTracks(audioSource, arrangement) == 0 && NotInCurrentArrangement(audioSource))
                    {
                        Debug.Log("Lower Volume: " + audioSource.name);
                        StartCoroutine(FadeOut(audioSource, fadeOutTime));
                    }
                    if (stop && audioSource.volume == 1 && BinaryTracks(audioSource, arrangement) == 0 && InAllArrangements(audioSource)
                        && (ControllerInputHandler.instance.speed > 0.5f && audioSource.name.Contains("major") ||
                        (ControllerInputHandler.instance.speed <= 0.5f && audioSource.name.Contains("minor"))))
                    {
                        Debug.Log("Stop Volume: " + audioSource.name);
                        audioSource.volume = 0;
                        //StartCoroutine(FadeOut(audioSource, fadeOutTime));
                    }
                    else if (!stop && audioSource.volume == 0 && BinaryTracks(audioSource, arrangement) == 1)
                    {
                        Debug.Log("Raise Volume: " + audioSource.name);
                        StartCoroutine(FadeIn(audioSource, fadeInTime));
                    }
                    audioSource.priority = (int)Mathf.Clamp(18 / audioSource.volume - 21, 1, 256);
                }

            }
        }

    }

    private bool NotInCurrentArrangement(AudioSource audioSource)
    {
        return (Array.Exists(oldArrangement.melody.Except(currentArrangement.melody).ToArray(), element => element == audioSource) ||
            Array.Exists(oldArrangement.countermelody.Except(currentArrangement.countermelody).ToArray(), element => element == audioSource) ||
            Array.Exists(oldArrangement.accompaniment.Except(currentArrangement.accompaniment).ToArray(), element => element == audioSource));
    }
    private bool InAllArrangements(AudioSource audioSource)
    {
        return (Array.Exists(stage3Selection.melody, element => element == audioSource) ||
            Array.Exists(stage3Selection.countermelody.ToArray(), element => element == audioSource) ||
            Array.Exists(stage3Selection.accompaniment, element => element == audioSource));
    }

    float UpdateVolume(float mean)
    {
        return (2 / Mathf.Sqrt(2 * Mathf.PI)) * Mathf.Pow((float)Math.E, -3 * (Mathf.Pow(ControllerInputHandler.instance.speed - mean, 2)));
    }

    int BinaryTracks(AudioSource audioSource, int arrangement)
    {
        if (audioSource.name.Contains("major"))
        {
            if(arrangement == 3)
            {
                return ControllerInputHandler.instance.speed <= 0.167f ? 1 : 0;
            } else if (arrangement == 2)
            {
                return ControllerInputHandler.instance.speed <= 0.333 ? 1 : 0;
            } else
            {
                return ControllerInputHandler.instance.speed <= 0.5 ? 1 : 0;
            }
            
        }
        else
        {
            if (arrangement == 3)
            {
                return ControllerInputHandler.instance.speed >= 0.833f ? 1 : 0;
            }
            else if (arrangement == 2)
            {
                return ControllerInputHandler.instance.speed >= 0.667 ? 1 : 0;
            }
            else
            {
                return ControllerInputHandler.instance.speed >= 0.5 ? 1 : 0;
            }
        }
    }

    IEnumerator IntroFade()
    {
        float timer = 0;
        Color c = overlayImage.color;
        overlayImage.enabled = true;
        while (timer < transitionTime)
        {
            timer += Time.deltaTime;
            overlayImage.color = new Color(c.r, c.b, c.g, 1f - timer / transitionTime);
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }
}
