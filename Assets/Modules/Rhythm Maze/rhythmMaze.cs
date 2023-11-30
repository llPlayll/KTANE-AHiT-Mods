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
    [SerializeField] private GameObject MazeParent;
    [SerializeField] private List<KMSelectable> ArrowButtons;
    [SerializeField] private List<GameObject> TopLeftMarkings;
    [SerializeField] private List<GameObject> TopRightMarkings;

    int[,] markings1 = { {0, 0, 0, 0, 0, 0},
                         {0, 0, 0, 0, 0, 0},
                         {0, 0, 0, 1, 0, 0},
                         {0, 1, 1, 0, 0, 0},
                         {0, 0, 0, 0, 0, 0},
                         {1, 0, 0, 1, 1, 0} };

    int[,] markings2 = { {0, 0, 0, 0, 0, 0},
                         {0, 0, 0, 1, 0, 0},
                         {1, 0, 1, 0, 1, 0},
                         {0, 0, 0, 0, 0, 0},
                         {0, 0, 1, 0, 0, 0},
                         {0, 0, 0, 1, 1, 1} };

    int markings1Row, markings1Column;
    int markings2Row, markings2Column;
    int[,] genMarkings1 = new int[3, 3];
    int[,] genMarkings2 = new int[3, 3];

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
        foreach (KMSelectable arrowButton in ArrowButtons) {
            arrowButton.OnInteract += delegate () { ArrowPressed(arrowButton); return false; };
        }
        MazeParent.SetActive(false);
    }

    void StartButtonPressed()
    {
        StartButton.gameObject.SetActive(false);
        if (deathWish)
        {
            currentSide = Rnd.Range(0, 2);
            ModuleRenderer.material = SidesMaterials[2];
            AudioSrc.clip = Songs[1];
            waitTime = 2f / 3f;

            markings1 = FlipIntGrid(markings1);
            markings2 = FlipIntGrid(markings2);
        }
        else
        {
            ModuleRenderer.material = SidesMaterials[currentSide];
            AudioSrc.clip = Songs[0];
            waitTime = 1.5f;
        }

        foreach (GameObject marking in TopLeftMarkings)
        {
            marking.SetActive(false);
        }
        foreach (GameObject marking in TopRightMarkings)
        {
            marking.SetActive(false);
        }
        MazeParent.SetActive(true);

        GetComponentInParent<KMSelectable>().UpdateChildrenProperly();
        Generate();
        AudioSrc.Play();
        StartCoroutine("SidesCycle");
        SetMarkings();
    }

    void MuteSound()
    {
        muted = !muted;
        AudioSrc.mute = muted;
    }

    void ArrowPressed(KMSelectable arrow)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, arrow.transform);
        string dirPressed = "";
        for (int i = 0; i < ArrowButtons.Count; i++)
        {
            if (ArrowButtons[i] == arrow)
            {
                dirPressed = "URDL"[i].ToString();
            }
        }
        Log(dirPressed);
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

    int[,] FlipIntGrid(int[,] grid)
    {
        int[,] newGrid = new int[6, 6];
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                newGrid[i, 6 - j - 1] = grid[i, j];
            }
        }
        return newGrid;
    }
        

    void Generate()
    {
        markings1Row = Rnd.Range(0, 4) + (deathWish ? 2 : 0);
        markings1Column = Rnd.Range(0, 4) + (deathWish ? 2 : 0);
        markings2Row = Rnd.Range(0, 4) + (deathWish ? 2 : 0);
        markings2Column = Rnd.Range(0, 4) + (deathWish ? 2 : 0);

        List<string> sideLog;
        string position = !deathWish ? "top-left" : "top-right"; 
        if (!deathWish)
        {
            sideLog = new List<string> { "Pink Side", "Blue Side" };
        }
        else
        {
            sideLog = new List<string> { "Side 1", "Side 2" };
        }
        Log($"Marking positions of the {position} corners are ({markings1Row + 1}, {markings1Column + 1}) for {sideLog[0]}, and ({markings2Row + 1}, {markings2Column + 1}) for {sideLog[1]}.");

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                genMarkings1[i, !deathWish ? j : 3 - j - 1] = markings1[markings1Row + i, markings1Column + (deathWish ? -j : j)];
                genMarkings2[i, !deathWish ? j : 3 - j - 1] = markings2[markings2Row + i, markings2Column + (deathWish ? -j : j)];
            }
        }
    }

    void SetMarkings()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (currentSide == 0)
                {
                    (!deathWish ? TopLeftMarkings : TopRightMarkings)[i * 3 + j].SetActive(new List<bool> { false, true }[genMarkings1[i, j]]);
                }
                else
                {
                    (!deathWish ? TopLeftMarkings : TopRightMarkings)[i * 3 + j].SetActive(new List<bool> { false, true }[genMarkings2[i, j]]);
                }
            }
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
            SetMarkings();
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
