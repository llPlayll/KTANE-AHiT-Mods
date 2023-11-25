using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class rhythmMaze : MonoBehaviour
{
    [SerializeField] private KMBombInfo Bomb;
    [SerializeField] private KMAudio Audio;
    [SerializeField] private AudioSource AudioSrc;

    [SerializeField] private MeshRenderer ModuleRenderer;
    [SerializeField] private KMSelectable StartButton;
    [SerializeField] private MeshRenderer StartButtonRenderer;
    [SerializeField] private List<Material> StartButtonMaterials;
    [SerializeField] private List<Material> SidesMaterials;
    [SerializeField] private List<AudioClip> Songs;
    [SerializeField] private KMSelectable MuteButton;

    bool deathWish;
    int currentSide = 0;
    float waitTime;

    bool muted;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    void Awake()
    {
        ModuleId = ModuleIdCounter++;
        /*
        foreach (KMSelectable object in keypad) {
            object.OnInteract += delegate () { keypadPress(object); return false; };
            }
        */

        //button.OnInteract += delegate () { buttonPress(); return false; };
        StartButton.OnInteract += delegate () { StartButtonPressed(); return false; };
        MuteButton.OnInteract += delegate () { MuteSound(); return false; };
    }

    void StartButtonPressed()
    {
        StartButton.gameObject.SetActive(false);
        if (deathWish)
        {
            ModuleRenderer.material = SidesMaterials[2];
            AudioSrc.clip = Songs[1];
            waitTime = 2f / 3f;
        }
        else
        {
            ModuleRenderer.material = SidesMaterials[currentSide];
            AudioSrc.clip = Songs[0];
            waitTime = 1.5f;
        }
        AudioSrc.Play();
        StartCoroutine("SidesCycle");
    }

    void MuteSound()
    {
        muted = !muted;
        AudioSrc.mute = muted;
    }

    void Start()
    {
        if (Bomb.GetModuleIDs().Contains("snatchersMap"))
        {
            StartButtonRenderer.material = StartButtonMaterials[1];
            deathWish = true;
            Log("Looks like someone has a Death Wish...");
            Log("This bomb has a Snatcher's Map, the Time Rift has become unstable!");
        }
        else
        {
            StartButtonRenderer.material = StartButtonMaterials[0];
        }
    }

    void Log(string arg)
    {
        Debug.Log($"[Rhythm Maze #{ModuleId}] {arg}");
    }

    IEnumerator SidesCycle()
    {
        while (!ModuleSolved)
        {
            yield return new WaitForSeconds(waitTime);
            currentSide++;
            currentSide %= 2;
            if (!deathWish)
            {
                ModuleRenderer.material = SidesMaterials[currentSide];
            }
        }
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use !{0} to do something.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        yield return null;
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
}
