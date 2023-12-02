﻿using System;
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
    [SerializeField] private GameObject PlayerObject;
    [SerializeField] private GameObject CellIndicatorsParent;
    
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
    string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    string[,] walls1 = { { "DL", "UD", "R", "LR", "UDL", "UR" },
                         { "UDL", "UR", "RL", "RL", "URL", "L" } ,
                         { "UD", "R", "RDL", "DL", "RD", "RDL" } ,
                         { "UDL", "RD", "UDL", "UD", "UD", "UD" } ,
                         { "U", "URD", "URL", "URL", "URDL", "URL" } ,
                         { "RDL", "UL", "RD", "RL", "UDL", "D" } };
    string[,] walls2 = { { "RDL", "URDL", "DL", "URD", "URL", "LDR" },
                         { "URL", "URL", "UDL", "UR", "DL", "UR" } ,
                         { "D", "RD", "URDL", "RL", "URDL", "LD" } ,
                         { "URDL", "URL", "URDL", "RDL", "URL", "URDL" } ,
                         { "UR", "RDL", "UL", "UR", "RDL", "URL" } ,
                         { "RDL", "URL", "RDL", "L", "URD", "DL" } };



    int markings1Row, markings1Column;
    int markings2Row, markings2Column;
    int[,] genMarkings1 = new int[3, 3];
    int[,] genMarkings2 = new int[3, 3];
    int[] serialNumValues = new int[6];
    List<int> basePonRows = new List<int>();
    List<int> basePonColumns = new List<int>();
    List<int> ponLocations = new List<int>();
    int baseGoalRow, baseGoalColumn, goalLocation;

    bool deathWish;
    int currentSide = 0;
    float waitTime;
    bool muted;
    int playerRow, playerCol;
    string playerColLetter;

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

        GenerateMarkings();
        GeneratePonsAndGoal();
        playerRow = Rnd.Range(1, 7);
        playerCol = Rnd.Range(1, 7);
        Log($"Starting position is ({playerRow}, {playerCol + 1}).");
        SetPlayerPos();

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

        switch (dirPressed)
        {
            case "U":
                if ((currentSide == 0 ? walls1 : walls2)[playerRow - 1, playerCol - 1].Contains("U") || playerRow == 1)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                }
                else
                {
                    playerRow -= 1;
                    SetPlayerPos();
                }
                break;
            case "R":
                if ((currentSide == 0 ? walls1 : walls2)[playerRow - 1, playerCol - 1].Contains("R") || playerCol == 6)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                }
                else
                {
                    playerCol += 1;
                    SetPlayerPos();
                }
                break;
            case "D":
                if ((currentSide == 0 ? walls1 : walls2)[playerRow - 1, playerCol - 1].Contains("D") || playerRow == 6)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                }
                else
                {
                    playerRow += 1;
                    SetPlayerPos();
                }
                break;
            case "L":
                if ((currentSide == 0 ? walls1 : walls2)[playerRow - 1, playerCol - 1].Contains("L") || playerCol == 1)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                }
                else
                {
                    playerCol -= 1;
                    SetPlayerPos();
                }
                break;
            default:
                break;
        }
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

    string[,] ShiftGridHoriz(string[,] grid, int offset, bool right)
    {
        string[,] newGrid = new string[6, 6];
        int shiftAmount = right ? 6 - offset : offset;
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                newGrid[i, j] = grid[i, (j + shiftAmount) % 6];
            }
        }
        return newGrid;
    }

    string[,] ShiftGridUp(string[,] grid, int offset)
    {
        string[,] newGrid = new string[6, 6];
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                newGrid[i, j] = grid[(i - offset) % 6, j];
            }
        }
        return newGrid;
    }

    void GenerateMarkings()
    {
        markings1Row = Rnd.Range(0, 4);
        markings1Column = Rnd.Range(0, 4) + (deathWish ? 2 : 0);
        markings2Row = Rnd.Range(0, 4);
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

    void GeneratePonsAndGoal()
    {
        for (int i = 0; i < 6; i++)
        {
            string character = Bomb.GetSerialNumber()[i].ToString();
            if (character == "0")
            {
                serialNumValues[i] = 1;
            }
            else if (alphabet.Contains(character))
            {
                serialNumValues[i] = alphabet.IndexOf(character) % 6 + 1;
            }
            else
            {
                serialNumValues[i] = (int.Parse(character) - 1) % 6 + 1;
            }
        }

        if (!deathWish)
        {
            for (int i = 0; i < 5; i++)
            {
                basePonRows.Add(serialNumValues[i]);
                basePonColumns.Add(serialNumValues[i + 1]);
            }
        }
        else
        {
            for (int i = 0; i < 3; i++)
            {
                basePonRows.Add(serialNumValues[2 * i]);
                basePonColumns.Add(serialNumValues[2 * i + 1]);
            }
        }

        string ponLog = "";
        for (int i = 0; i < basePonRows.Count; i++)
        {
            int addLocation = AvoidDuplicates(basePonRows[i] * 10 + basePonColumns[i], ponLocations); ;
            ponLocations.Add(addLocation);
            ponLog += $"({ponLocations[i].ToString()[0]}, {ponLocations[i].ToString()[1]})" + (i == basePonRows.Count - 1 ? "." : ", ");
        }
        Log($"The pons are at coordinates: {ponLog}.");

        baseGoalRow = serialNumValues[0];
        baseGoalColumn = serialNumValues[5];
        goalLocation = AvoidDuplicates(baseGoalRow * 10 + baseGoalColumn, ponLocations);
        Log($"The goal is at ({goalLocation.ToString()[0]}, {goalLocation.ToString()[1]}).");
    }

    int AvoidDuplicates(int location, List<int> locationsList)
    {
        while (locationsList.Contains(location))
        {
            if (location % 10 == 6)
            {
                location = ((int)(location / 10) + 1) * 10;
                if (location == 70)
                {
                    location = 11;
                }
            }
            else
            {
                location += 1;
            }
        }
        return location;
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

    void SetPlayerPos()
    {
        playerColLetter = "ABCDEF"[playerCol - 1].ToString();
        PlayerObject.transform.position = CellIndicatorsParent.transform.Find(playerColLetter + playerRow.ToString()).transform.position;
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
