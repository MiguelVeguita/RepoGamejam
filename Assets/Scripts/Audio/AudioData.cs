using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Audio", menuName = "Volume Data", order = 2)]
public class AudioData : ScriptableObject
{
    public float _master = 1f;
    public float _SFX = 1f;
    public float _music = 1f;
    public List<AudioClip> musicClip = new List<AudioClip>();
    public List<AudioClip> sfxClip = new List<AudioClip>();

}
