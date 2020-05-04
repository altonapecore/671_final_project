using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    FMOD.Studio.EventInstance SFXVolumeTestEvent;
    FMOD.Studio.EventInstance hover;
    FMOD.Studio.EventInstance click;

    FMOD.Studio.Bus Music;
    FMOD.Studio.Bus SFX;
    float MusicVolume = 0.5f;
    float SFXVolume = 0.5f;



    private void Awake()
    {
        Music = FMODUnity.RuntimeManager.GetBus("bus:/Music");
        SFX = FMODUnity.RuntimeManager.GetBus("bus:/Sfx");
        SFXVolumeTestEvent = FMODUnity.RuntimeManager.CreateInstance("event:/Effects/SFXTest");
        hover = FMODUnity.RuntimeManager.CreateInstance("event:/Effects/hover");
        click = FMODUnity.RuntimeManager.CreateInstance("event:/Effects/select");
    }

    private void Update()
    {
        Music.setVolume(MusicVolume);
        SFX.setVolume(SFXVolume);
    }

    public void MusicVolumeLevel(float volume)
    {
        MusicVolume = volume;
    }

    public void SFXVolumeLevel(float volume)
    {
        SFXVolume = volume;

        FMOD.Studio.PLAYBACK_STATE pbState;
        SFXVolumeTestEvent.getPlaybackState(out pbState);
        if (pbState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            SFXVolumeTestEvent.start();
        }
    }


    public void Hover()
    {
        FMOD.Studio.PLAYBACK_STATE pbState;
        hover.getPlaybackState(out pbState);
        if (pbState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            hover.start();
        }
    }

    public void Click()
    {
        FMOD.Studio.PLAYBACK_STATE pbState;
        click.getPlaybackState(out pbState);
        if (pbState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            click.start();
        }

    }
}
