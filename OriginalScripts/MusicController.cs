using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    [System.NonSerialized]
    public int bpm;
    [SerializeField]
    private AudioSource audioSource;
    // Start is called before the first frame update
    void Start()
    {
        bpm = UniBpmAnalyzer.AnalyzeBpm(audioSource.clip);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
