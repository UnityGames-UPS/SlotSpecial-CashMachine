using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioListener audio_listener;
    [SerializeField] private AudioSource bg_adudio;
    [SerializeField] internal AudioSource audioPlayer_wl;
    [SerializeField] internal AudioSource audioPlayer_button;
    [SerializeField] internal AudioSource audioSpin_button;
    [SerializeField] private AudioClip[] clips;

    private List<AudioSource> allSources;
    private readonly Dictionary<AudioSource, bool> preFocusMuteState = new Dictionary<AudioSource, bool>();
    private bool isForceMuted = false;

    private void Awake()
    {
        allSources = new List<AudioSource> { bg_adudio, audioPlayer_wl, audioPlayer_button, audioSpin_button };
    }

    private void Start()
    {
        if (bg_adudio) bg_adudio.Play();
        audioPlayer_button.clip = clips[clips.Length-1];
        audioSpin_button.clip = clips[clips.Length-2];
    }

    internal void PlayWLAudio(string type)
    {
        audioPlayer_wl.loop = false;
        int index = 0;
        switch (type)
        {
            case "bigwin":
                index = 0;
                break;
            case "win":
                index = 1;
                break;
            case "dot":
                index = 2;
                break;
            case "respin":
                index = 3;
                break;
            case "spin":
                index = 4;
                audioPlayer_wl.loop = true;
                break;
        }
        StopWLAaudio();
        audioPlayer_wl.clip = clips[index];
        audioPlayer_wl.Play();
    }

    internal void PlayButtonAudio()
    {
        audioPlayer_button.Play();
    }

    internal void PlaySpinButtonAudio()
    {
        audioSpin_button.Play();
    }

    internal void StopWLAaudio()
    {
        audioPlayer_wl.Stop();
        audioPlayer_wl.loop = false;
    }

    internal void StopBgAudio()
    {
        bg_adudio.Stop();
    }

    // User-toggle-driven — the sound/mute button. An explicit user interaction proves the game
    // currently has real focus, so it clears any stale forced-mute and always wins immediately.
    internal void ToggleMute(bool toggle, string type="all")
    {
        isForceMuted = false;
        switch (type)
        {
            case "bg":
                ApplyUserMute(bg_adudio, toggle);
                break;
            case "button":
                ApplyUserMute(audioPlayer_button, toggle);
                ApplyUserMute(audioSpin_button, toggle);
                break;
            case "wl":
                ApplyUserMute(audioPlayer_wl, toggle);
                break;
            case "all":
                ApplyUserMute(audioPlayer_wl, toggle);
                ApplyUserMute(bg_adudio, toggle);
                ApplyUserMute(audioPlayer_button, toggle);
                ApplyUserMute(audioSpin_button, toggle);
                break;
        }
    }

    private void ApplyUserMute(AudioSource source, bool mute)
    {
        if (source == null) return;
        source.mute = mute;
        preFocusMuteState[source] = mute;
    }

    // Focus-driven — called from BOTH OnFocusChanged (JS bridge) and OnApplicationFocus below.
    // Never force-unmutes: on regaining focus each source returns to the user's last chosen state.
    internal void SetMuteAll(bool forceMute)
    {
        if (forceMute == isForceMuted || allSources == null) return;
        isForceMuted = forceMute;

        foreach (var source in allSources)
        {
            if (source == null) continue;
            if (forceMute)
            {
                preFocusMuteState[source] = source.mute;
                source.mute = true;
            }
            else
            {
                source.mute = preFocusMuteState.TryGetValue(source, out bool prevMuted) ? prevMuted : source.mute;
            }
        }
    }

    // Native/editor focus path — calls the SAME method the WebGL OnFocusChanged path calls.
    private void OnApplicationFocus(bool focus)
    {
        SetMuteAll(!focus);
    }
}
