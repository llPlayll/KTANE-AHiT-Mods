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
    [SerializeField] private KMColorblindMode Colorblind;
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
    [SerializeField] private List<AudioClip> PonCollectionClips;
    [SerializeField] private AudioClip HmmmClip;
    [SerializeField] private KMSelectable DefeatButton;
    [SerializeField] private TextMesh ColorblindText;
    [SerializeField] private GameObject DeathWishTimerParent;
    [SerializeField] private TextMesh DeathWishTimerSeconds;
    [SerializeField] private TextMesh DeathWishTimerMilliseconds;

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
    string[,] walls1 = { { "RD", "URDL", "UDL", "UD", "RD", "DL" },
                         { "UDL", "UD", "UR", "UDL", "U", "URD" },
                         { "U", "URD", "RDL", "URL", "RDL", "UL" },
                         { "DL", "URD", "URL", "RDL", "UDL", "RD" },
                         { "URD", "UDL", "D", "UD", "URD", "UL" },
                         { "UR", "UDL", "UD", "UD", "UR", "L" } };
    string[,] walls2 = { { "UL", "RD", "UL", "R", "UDL", "UR" },
                         { "RDL", "URL", "RDL", "RDL", "URL", "RL" },
                         { "URD", "DL", "URD", "UL", "RD", "DL" },
                         { "UL", "URD", "URL", "RL", "URL", "URL" },
                         { "DL", "URD", "RL", "RDL", "RL", "RL" },
                         { "UDL", "U", "RD", "UL", "RD", "RDL" } };



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
    int ponsCollected;
    List<int> uncollectedPons;

    int deathWishSeconds = 600;
    int deathWishMilliseconds = 0;

    bool TPStarted;

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
        DefeatButton.OnInteract += delegate () { DisableCollapse(); return false; };
        foreach (KMSelectable arrowButton in ArrowButtons) {
            arrowButton.OnInteract += delegate () { ArrowPressed(arrowButton); return false; };
        }
        MazeParent.SetActive(false);
    }

    void DisableCollapse()
    {
        deathWish = false;
        GetComponent<KMBombModule>().HandleStrike();
        Log("I don't have to prove myself to anyone.");
        Log("You have stabilized the module, at a cost of a strike.");
        SetupButtons();
    }

    void StartButtonPressed()
    {
        TPStarted = true;
        StartButton.gameObject.SetActive(false);
        DefeatButton.gameObject.SetActive(false);
        if (deathWish)
        {
            currentSide = Rnd.Range(0, 2);
            ModuleRenderer.material = SidesMaterials[2];
            AudioSrc.clip = Songs[1];
            waitTime = 2f / 3f;

            markings1 = FlipIntGrid(markings1);
            markings2 = FlipIntGrid(markings2);
            walls1 = FlipMaze(walls1);
            walls2 = FlipMaze(walls2);

            DeathWishTimerParent.SetActive(true);
            StartCoroutine("DeathWishTimerRoutine");
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
        SetColorblindText();
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

        bool struck = false;
        switch (dirPressed)
        {
            case "U":
                if ((currentSide == 0 ? walls1 : walls2)[playerRow - 1, playerCol - 1].Contains("U"))
                {
                    Log($"Trying to go up from ({playerRow}, {playerCol}) is a wall. Strike!");
                    GetComponent<KMBombModule>().HandleStrike();
                    struck = true;
                }
                else
                {
                    playerRow -= 1;
                    if (playerRow == 0)
                    {
                        playerRow = 6;
                    }
                    SetPlayerPos();
                }
                break;
            case "R":
                if ((currentSide == 0 ? walls1 : walls2)[playerRow - 1, playerCol - 1].Contains("R"))
                {
                    Log($"Trying to go right from ({playerRow}, {playerCol}) is a wall. Strike!");
                    GetComponent<KMBombModule>().HandleStrike();
                    struck = true;
                }
                else
                {
                    playerCol += 1;
                    if (playerCol == 7)
                    {
                        playerCol = 1;
                    }
                    SetPlayerPos();
                }
                break;
            case "D":
                if ((currentSide == 0 ? walls1 : walls2)[playerRow - 1, playerCol - 1].Contains("D"))
                {
                    Log($"Trying to go down from ({playerRow}, {playerCol}) is a wall. Strike!");
                    GetComponent<KMBombModule>().HandleStrike();
                    struck = true;
                }
                else
                {
                    playerRow += 1;
                    if (playerRow == 7)
                    {
                        playerRow = 1;
                    }
                    SetPlayerPos();
                }
                break;
            case "L":
                if ((currentSide == 0 ? walls1 : walls2)[playerRow - 1, playerCol - 1].Contains("L"))
                {
                    Log($"Trying to go left from ({playerRow}, {playerCol}) is a wall. Strike!");
                    GetComponent<KMBombModule>().HandleStrike();
                    struck = true;
                }
                else
                {
                    playerCol -= 1;
                    if (playerCol == 0)
                    {
                        playerCol = 6;
                    }
                    SetPlayerPos();
                }
                break;
            default:
                break;
        }

        if (!struck)
        {
            CheckPonsAndGoal();
        }
    }

    void CheckPonsAndGoal()
    {
        int playerLocation = playerRow * 10 + playerCol;
        if (uncollectedPons.Contains(playerLocation))
        {
            uncollectedPons.Remove(playerLocation);
            ponsCollected++;
            Log($"Collected a Pon at ({playerLocation.ToString()[0]}, {playerLocation.ToString()[1]}). That's {ponsCollected} Pons collected.");
            if (!deathWish)
            {
                Audio.PlaySoundAtTransform(PonCollectionClips[ponsCollected - 1].name, transform);
            }
        }
        if (goalLocation == playerLocation)
        {
            if (ponsCollected >= 3)
            {
                Log($"Went to the goal at ({playerLocation.ToString()[0]}, {playerLocation.ToString()[1]}) with enough pons. Module solved.");
                GetComponent<KMBombModule>().HandlePass();

                ModuleSolved = true;
                MazeParent.SetActive(false);
                ColorblindText.gameObject.SetActive(false);
                AudioSrc.Stop();
            }
            else
            {
                Log($"Went to the goal at ({playerLocation.ToString()[0]}, {playerLocation.ToString()[1]}) with less than 3 pons. Hmmm...");
                if (!deathWish)
                {
                    Audio.PlaySoundAtTransform(HmmmClip.name, transform);
                }
            }
        }
    }

    void Start()
    {
        if (Bomb.GetModuleIDs().Contains("snatchersMap"))
        {
            Log("This bomb has a Snatcher's Map, the Time Rift has become unstable!");
            deathWish = true;
        }
        else
        {
            deathWish = false;
        }
        SetupButtons();
    }

    void SetupButtons()
    {
        if (deathWish)
        {
            StartButtonRenderer.material = StartButtonMaterials[1];
            DefeatButton.gameObject.SetActive(true);
        }
        else
        {
            StartButtonRenderer.material = StartButtonMaterials[0];
            DefeatButton.gameObject.SetActive(false);
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

    string[,] FlipMaze(string[,] grid)
    {
        string[,] newGrid = new string[6, 6];
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                newGrid[i, 6 - j - 1] = grid[i, j].Replace('L', '*').Replace('R', 'L').Replace('*', 'R');
            }
        }
        return newGrid;
    }

    string[,] ShiftGridHoriz(string[,] grid, int offset, bool right)
    {
        string[,] newGrid = new string[6, 6];
        int shiftAmount = right ? offset + 1 : offset;
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
                newGrid[i, j] = grid[(i + offset) % 6, j];
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

        walls1 = ShiftGridHoriz(walls1, markings1Column, deathWish);
        walls1 = ShiftGridUp(walls1, markings1Row);
        walls2 = ShiftGridHoriz(walls2, markings2Column, deathWish);
        walls2 = ShiftGridUp(walls2, markings2Row);

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
            ponLog += $"({ponLocations[i].ToString()[0]}, {ponLocations[i].ToString()[1]})" + (i == basePonRows.Count - 1 ? "" : ", ");
        }
        uncollectedPons = ponLocations.ConvertAll(position => position);
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
                location = ((int)(location / 10) + 1) * 10 + 1;
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

    void SetColorblindText()
    {
        if (!deathWish)
        {
            if (Colorblind.ColorblindModeActive & !TwitchPlaysActive)
            {
                ColorblindText.text = "PB"[currentSide].ToString();
            }
        }
        if (TwitchPlaysActive)
        {
            ColorblindText.text = (currentSide + 1).ToString();
        }
    }

    IEnumerator SidesCycle()
    {
        while (!ModuleSolved)
        {
            yield return new WaitForSeconds(waitTime);
            currentSide++;
            currentSide %= 2;
            SetColorblindText();
            if (!deathWish)
            {
                ModuleRenderer.material = SidesMaterials[currentSide];
            }
            SetMarkings();
        }
    }

    IEnumerator DeathWishTimerRoutine()
    {
        while (deathWishSeconds > 0 || deathWishMilliseconds > 0)
        {
            deathWishMilliseconds -= 1;
            if (deathWishMilliseconds == -1)
            {
                deathWishSeconds -= 1;
                deathWishMilliseconds = 99;
            }
            DeathWishTimerSeconds.text = deathWishSeconds.ToString();
            DeathWishTimerMilliseconds.text = deathWishMilliseconds.ToString().Length == 2 ? deathWishMilliseconds.ToString() : "0" + deathWishMilliseconds.ToString();
            yield return new WaitForSeconds(0.01f);
        }
        GetComponent<KMBombModule>().HandleStrike();
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"Use <!{0} start> to start the module. <!{0} mute> To press the status light. <!{0} move 1ur2dl> to move up then right on side 1, and then move down then left on side 2. The number in the top-left corner of the module is the side number.";
    private bool TwitchPlaysActive = false;
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        switch (Command)
        {
            case "start":
                if (!TPStarted)
                {
                    yield return null;
                    StartButton.OnInteract();
                }
                else
                {
                    yield return "sendtochatmessage Module already started!";
                }
                break;
            case "mute":
                yield return null;
                MuteButton.OnInteract();
                break;
            case "move":
                if (!TPStarted)
                {
                    yield return "sendtochatmessage Module not started yet!";
                }
                else
                {
                    string[] commandMoves = Command.ToLowerInvariant().Split(new[] { ' ' }, 2);
                    if (commandMoves.Length != 2)
                    {
                        yield return "sendtochatmessage Invalid moves!";
                        break;
                    }
                    int currentCheckSide = -1;
                    for (int i = 0; i < commandMoves[1].Length; i++)
                    {
                        if (!"urdl12".Contains(commandMoves[1][i])) { yield return "sendtochatmessage Invalid moves!"; break; }
                        else if ("urdl".Contains(commandMoves[1][i]) & currentCheckSide == -1 { yield return "sendtochatmessage Invalid moves!"; break; }
                        else if ("12".Contains(commandMoves[1][i]) { currentCheckSide = (int)(commandMoves[1][i]); }
                    }
                }
                break;
            default:
                break;
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        yield return null;
    }
}
