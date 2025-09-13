using Sirenix.OdinInspector;
using UnityEngine;
using Zone8.Audio;

public class AudioTest : MonoBehaviour
{
    [SerializeField] private SFXClip sfxClip;
    [SerializeField] private int playtime = 5;


    private SFXManager sfxManager;


    private void Awake()
    {
        sfxManager = FindAnyObjectByType<SFXManager>();
    }

    [Button]
    private void Play()
    {
        for (int i = 0; i < playtime; i++)
        {
            sfxClip.Play();
        }
    }

    [Button]
    public void StopAllAndPlay()
    {
        sfxManager.StopAll();
        sfxClip.Play();
    }
}
