using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using System.Collections;
using System.Collections.Generic;

public class TextToASL : MonoBehaviour
{
    public Animator animator;

    private PlayableGraph graph;
    private AnimationClipPlayable clipPlayable;
    private AnimationPlayableOutput output;

    // Optional: preload all clips for speed
    private Dictionary<string, AnimationClip> clipCache = new Dictionary<string, AnimationClip>();

    void Awake()
    {
        // Create persistent PlayableGraph
        graph = PlayableGraph.Create("ASLGraph");
        output = AnimationPlayableOutput.Create(graph, "Animation", animator);
        graph.Play();

        // Optional: preload all clips from Resources for faster playback
        AnimationClip[] clips = Resources.LoadAll<AnimationClip>("FBX_Animations");
        foreach (var clip in clips)
        {
            clipCache[clip.name] = clip;
        }
    }

    /// <summary>
    /// Play a single clip immediately
    /// </summary>
    public void PlayClip(string clipName)
    {
        AnimationClip clip = null;

        // Try to get from cache first
        if (clipCache.ContainsKey(clipName))
            clip = clipCache[clipName];
        else
            clip = Resources.Load<AnimationClip>("FBX_Animations/" + clipName);

        if (clip == null)
        {
            Debug.LogWarning("Clip not found: " + clipName);
            return;
        }

        // Destroy previous playable if exists
        if (clipPlayable.IsValid())
            clipPlayable.Destroy();

        // Create new playable
        clipPlayable = AnimationClipPlayable.Create(graph, clip);
        output.SetSourcePlayable(clipPlayable);
    }

    /// <summary>
    /// Play a sequence of clips in order, like spelling a phrase
    /// </summary>
    public void PlaySequence(string[] clipNames)
    {
        StartCoroutine(PlaySequenceCoroutine(clipNames));
    }

    private IEnumerator PlaySequenceCoroutine(string[] clipNames)
    {
        foreach (var clipName in clipNames)
        {
            PlayClip(clipName);

            // Wait for clip to finish before playing next
            AnimationClip clip = clipCache.ContainsKey(clipName) ? clipCache[clipName] : Resources.Load<AnimationClip>("FBX_Animations/" + clipName);
            if (clip != null)
                yield return new WaitForSeconds(clip.length);
            else
                yield return null;
        }
    }

    void OnDestroy()
    {
        if (graph.IsValid())
            graph.Destroy();
    }

    void Start()
    {
        PlaySequence(new string[] { "Alcohol", "Petting_Animal","Typing" });
    }

}