/// <summary>
/// Project : Mind Code Interactive
/// Class : AudioClipPlayer.cs
/// Namespace : MindCodeInteractive.Common.Framework.Code.Runtime.Systems.AudioSystem
/// Copyright : © 2015 - 2026 Mind Code Interactive
/// </summary>

using System;
using System.Collections.Generic;
using System.Reflection;

using UnityEngine;
using UnityEngine.Audio;
using Random = UnityEngine.Random;

namespace MindCodeInteractive.Common.Framework.Code.Runtime.Systems.AudioSystem
{
    [Serializable]
    public sealed class AudioClipPlayer
    {
        public enum PlayMode { TwoD, ThreeD }
        public enum SelectMode { Single, RandomNoImmediateRepeat, Sequential, ShuffleNoImmediateRepeat }

        [SerializeField] private AudioClip m_clip;
        [SerializeField] private AudioClip[] m_clips;
        [SerializeField] private SelectMode m_select = SelectMode.Single;
        [SerializeField] private AudioMixerGroup m_mixer;
        [SerializeField] private PlayMode m_mode = PlayMode.TwoD;
        [SerializeField, Range(0f, 1f)] private float m_volume = 1f;
        [SerializeField, Range(0f, 1f)] private float m_volumeJitter;
        [SerializeField, Range(0.1f, 3f)] private float m_pitch = 1f;
        [SerializeField, Range(0f, 1f)] private float m_pitchJitter;
        [SerializeField, Range(0f, 1f)] private float m_spatialBlend = 1f;
        [SerializeField] private AudioRolloffMode m_rolloff = AudioRolloffMode.Logarithmic;
        [SerializeField] private float m_minDistance = 1f;
        [SerializeField] private float m_maxDistance = 500f;

        [NonSerialized] private int m_lastIndex = -1;
        [NonSerialized] private int m_sequentialIndex;
        [NonSerialized] private Queue<int> m_shuffleQueue;

#if UNITY_EDITOR
        private static readonly Type s_audioUtilType = Type.GetType("UnityEditor.AudioUtil,UnityEditor");
        private static readonly MethodInfo s_playClip = s_audioUtilType != null ? s_audioUtilType.GetMethod("PlayClip", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(AudioClip) }, null) : null;
        private static readonly MethodInfo s_stopAll = s_audioUtilType != null ? s_audioUtilType.GetMethod("StopAllClips", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : null;

        private void PlayInEditor(AudioClip clip)
        {
            if (!clip)
                return;

            try
            {
                s_stopAll?.Invoke(null, null);
                s_playClip?.Invoke(null, new object[] { clip });
            }
            catch
            {
            }
        }
#endif

        public void Play()
        {
            AudioClip audioClip = GetNextClip();
            if (!audioClip)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                PlayInEditor(audioClip);
                return;
            }
#endif
            AudioSource source = CreateSource(audioClip, null, Vector3.zero, false);
            Configure3D(source, false);
            source.Play();
            AutoDestroy(source);
        }

        public void PlayAtPosition(Vector3 position)
        {
            AudioClip audioClip = GetNextClip();
            if (!audioClip)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                PlayInEditor(audioClip);
                return;
            }
#endif
            AudioSource source = CreateSource(audioClip, null, position, true);
            Configure3D(source, m_mode == PlayMode.ThreeD);
            source.Play();
            AutoDestroy(source);
        }

        public void PlayOn(Transform parent)
        {
            AudioClip audioClip = GetNextClip();
            if (!audioClip || !parent)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                PlayInEditor(audioClip);
                return;
            }
#endif
            AudioSource source = CreateSource(audioClip, parent, Vector3.zero, true);
            Configure3D(source, m_mode == PlayMode.ThreeD);
            source.Play();
            AutoDestroy(source);
        }

        private AudioClip GetNextClip()
        {
            int clipCount = m_clips != null ? m_clips.Length : 0;
            if (clipCount <= 0)
                return m_clip;

            if (m_select == SelectMode.Single)
            {
                m_lastIndex = 0;
                return m_clips[0];
            }

            if (m_select == SelectMode.Sequential)
            {
                if (clipCount == 1)
                {
                    m_lastIndex = 0;
                    return m_clips[0];
                }

                int index = m_sequentialIndex % clipCount;
                m_sequentialIndex = (m_sequentialIndex + 1) % clipCount;
                m_lastIndex = index;
                return m_clips[index];
            }

            if (m_select == SelectMode.RandomNoImmediateRepeat)
            {
                if (clipCount == 1)
                {
                    m_lastIndex = 0;
                    return m_clips[0];
                }

                int index;
                do
                {
                    index = Random.Range(0, clipCount);
                } while (index == m_lastIndex);

                m_lastIndex = index;
                return m_clips[index];
            }

            if (m_select == SelectMode.ShuffleNoImmediateRepeat)
            {
                if (clipCount == 1)
                {
                    m_lastIndex = 0;
                    return m_clips[0];
                }

                if (m_shuffleQueue == null || m_shuffleQueue.Count == 0)
                {
                    List<int> shuffleList = new List<int>(clipCount);
                    for (int i = 0; i < clipCount; i++)
                        shuffleList.Add(i);

                    for (int i = 0; i < clipCount - 1; i++)
                    {
                        int randomIndex = Random.Range(i, clipCount);
                        (shuffleList[i], shuffleList[randomIndex]) = (shuffleList[randomIndex], shuffleList[i]);
                    }

                    if (m_lastIndex >= 0 && clipCount > 1 && shuffleList[0] == m_lastIndex)
                    {
                        int swapIndex = Random.Range(1, clipCount);
                        (shuffleList[0], shuffleList[swapIndex]) = (shuffleList[swapIndex], shuffleList[0]);
                    }

                    m_shuffleQueue = new Queue<int>(shuffleList);
                }

                int pickedIndex = m_shuffleQueue.Dequeue();
                m_lastIndex = pickedIndex;
                return m_clips[pickedIndex];
            }

            return m_clip;
        }

        private AudioSource CreateSource(AudioClip audioClip, Transform parent, Vector3 position, bool worldPosition)
        {
            GameObject audioGameObject = new GameObject("AudioClipPlayer_Audio") { hideFlags = HideFlags.HideAndDontSave };
            if (parent)
            {
                audioGameObject.transform.SetParent(parent, false);
                if (worldPosition)
                    audioGameObject.transform.position = parent.position;
            }
            else
            {
                audioGameObject.transform.position = position;
            }

            AudioSource audioSource = audioGameObject.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.outputAudioMixerGroup = m_mixer;

            float adjustedVolume = Mathf.Clamp01(m_volume + Random.Range(-m_volumeJitter, m_volumeJitter));
            float adjustedPitch = Mathf.Clamp(m_pitch + Random.Range(-m_pitchJitter, m_pitchJitter), 0.1f, 3f);

            audioSource.volume = adjustedVolume;
            audioSource.pitch = adjustedPitch;
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = Mathf.Clamp01(m_spatialBlend);
            return audioSource;
        }

        private void Configure3D(AudioSource audioSource, bool enable3D)
        {
            if (!enable3D)
            {
                audioSource.spatialBlend = 0f;
                return;
            }

            audioSource.spatialBlend = Mathf.Clamp01(m_spatialBlend);
            audioSource.minDistance = m_minDistance;
            audioSource.maxDistance = m_maxDistance;
            audioSource.rolloffMode = m_rolloff;
            audioSource.dopplerLevel = 0f;
        }

        private void AutoDestroy(AudioSource audioSource)
        {
            if (!audioSource || !audioSource.clip)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(audioSource.gameObject);
                return;
            }
#endif
            UnityEngine.Object.Destroy(audioSource.gameObject, audioSource.clip.length / Mathf.Max(0.01f, audioSource.pitch));
        }
    }
}