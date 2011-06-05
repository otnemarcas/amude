using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;
using Microsoft.Xna.Framework.Audio;

namespace Amude.Core
{
    internal class AudioManager
    {
        private static AudioManager instance;

        private float effectsVolume;
        private float musicVolume;
        private AudioEngine audioEngine;
        private WaveBank waveBank;
        private SoundBank soundBank;
        private AudioCategory effectsCategory;
        private AudioCategory musicCategory;
        private List<Cue> musics;
        private bool isCyclic;
        
        // SingleTon
        private AudioManager() { }

        public static float EffectsVolume 
        {
            get
            {
                return instance.effectsVolume;
            }
            set
            {
                instance.effectsVolume = value;
                instance.SetEffectsVolume();
            }
        }

        public static float MusicVolume
        {
            get
            {
                return instance.musicVolume;
            }
            set
            {
                instance.musicVolume = value;
                instance.SetMusicVolume();
            }
        }

        public static void Initialize()
        {
            if (instance == null)
            {
                instance = new AudioManager();
                instance.musics = new List<Cue>();
                instance.audioEngine = new AudioEngine("Content/Sound/xact.xgs");
                instance.waveBank = new WaveBank(instance.audioEngine, "Content/Sound/waveBank.xwb");
                instance.soundBank = new SoundBank(instance.audioEngine, "Content/Sound/soundBank.xsb");
                instance.effectsCategory = instance.audioEngine.GetCategory("Effects");
                instance.musicCategory = instance.audioEngine.GetCategory("Music");
            }

            if (ConfigurationManager.AppSettings["effectsVolume"] == null)
                EffectsVolume = 1f;
            else
                EffectsVolume = float.Parse(ConfigurationManager.AppSettings["effectsVolume"]);

            if (ConfigurationManager.AppSettings["musicVolume"] == null)
                MusicVolume = 1f;
            else
            {
               MusicVolume = float.Parse(ConfigurationManager.AppSettings["musicVolume"]);
            }
        }

        public static void UpdateConfigFile()
        {
            IO.WriteConfig("musicVolume", instance.musicVolume.ToString("##0.00"));
            IO.WriteConfig("effectsVolume", instance.effectsVolume.ToString("##0.00"));
        }

        public static Cue PlaySound(string soundName)
        {
            Cue sound = instance.soundBank.GetCue(soundName);
            sound.Play();
            return sound;
        }

        public static void PlayMusic(string musicName, bool isCyclic)
        {
            instance.isCyclic = isCyclic;
            foreach (Cue music in instance.musics)
            {
                music.Stop(AudioStopOptions.Immediate);
            }

            instance.musics.Clear();
            Cue cue = instance.soundBank.GetCue(musicName);
            cue.Play();
            instance.musics.Add(cue);
        }

        public static void PlayMusicSequence(List<string> soundNames, bool isCyclic)
        {
            instance.isCyclic = isCyclic;
            foreach (Cue music in instance.musics)
            {
                music.Stop(AudioStopOptions.Immediate);
            }
            instance.musics.Clear();

            foreach (string soundName in soundNames)
            {
                instance.musics.Add(instance.soundBank.GetCue(soundName));
            }
            instance.musics[0].Play();
        }

        public static void Update()
        {
            if (instance.musics.Count > 0)
            {
                Cue music = instance.musics[0];
                if (music.IsStopped)
                {
                    instance.musics.RemoveAt(0);
                    if (instance.isCyclic)
                    {
                        instance.musics.Add(instance.soundBank.GetCue(music.Name));
                    }
                    if (instance.musics.Count > 0)
                    {
                        instance.musics[0].Play();
                    }
                }
            }
        }

        private void SetEffectsVolume()
        {
            effectsCategory.SetVolume(effectsVolume);
        }

        private void SetMusicVolume()
        {
            musicCategory.SetVolume(musicVolume);
        }
    }
}
