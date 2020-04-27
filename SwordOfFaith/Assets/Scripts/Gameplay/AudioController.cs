using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioController : MonoBehaviour
{
    FMOD.Studio.EventInstance SFXVolumeTestEvent;
    FMOD.Studio.EventInstance MenuMusic;
    FMOD.Studio.EventInstance LevelMusic;

    FMOD.Studio.Bus Music;
    FMOD.Studio.Bus SFX;
    float MusicVolume = 0.5f;
    float SFXVolume = 0.5f;

    

    private void Awake()
    {
        Music = FMODUnity.RuntimeManager.GetBus("bus:/Music");
        SFX = FMODUnity.RuntimeManager.GetBus("bus:/Sfx");
        SFXVolumeTestEvent = FMODUnity.RuntimeManager.CreateInstance("event:/Effects/SFXTest");
        MenuMusic = FMODUnity.RuntimeManager.CreateInstance("event:/Menu Theme");
        LevelMusic = FMODUnity.RuntimeManager.CreateInstance("event:/Level1 Theme");
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
        if(pbState != FMOD.Studio.PLAYBACK_STATE.PLAYING)
        {
            SFXVolumeTestEvent.start();
        }
    }

    private void OnLevelWasLoaded(int level)
    {
        // Stop all music
        MenuMusic.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        LevelMusic.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

        // Start the next level's music
        switch (level)
        {
            case 0:
                MenuMusic.start();
                break;
            case 1:
                LevelMusic.start();
                break;
        }
    }
}
