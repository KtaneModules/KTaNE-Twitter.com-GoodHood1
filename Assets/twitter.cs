using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;
using Rnd = UnityEngine.Random;

public class twitter : MonoBehaviour
{
    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;

    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool ModuleSolved;

    public KMSelectable[] buttons;
    public TextMesh[] messageTexts;

    private int correctButton;

    public MeshRenderer[] iconRenderers;
    public Material[] peopleImgs;

    private string[] peopleNames = { "Eltrick", "GhostSalt", "eXish", "Quinn Wuest", "Kilo Bites", "GoodHood", "Blananas2", "Deaf", "Kuro" };

    private string personChosenName;
    private int personChosenIndex;

    private int cloutValue;



    public GameObject theModule;
    public GameObject twitterLogoGO;
    public KMSelectable twitterLogo;
    
    private string[,] messages = {
    { "-KTaNE is overrated.", "-Taylor Swift was never good.", "-Memes are just terrible today." },
    { "-Jackbox games are worthless.", "-The Last of Us was just okay.", "-Art is too confusing." },
    { "-YouTubers aren't funny.", "-Drake is only slightly talented.", "-Soccer is not entertaining." },
    { "-KTaNE is dying.", "-Sparkling water is good.", "-Minecraft is for kids." },
    { "-The MCU has lost its appeal.", "-Top Gear annoys me.", "-Art is a joke." },
    { "-Waffles are better than pancakes.", "-Pokemon hasn't been good in years.", "-Football has gotten boring." },
    { "-KTaNE is a waste of time.", "-Art is not difficult.", "-Memes make no sense." },
    { "-McDonalds is not that bad.", "-Harry Potter is subpar.", "-Spongebob always sucked." },
    { "-YouTubers are too loud.", "-Daft Punk was stupid.", "-Memes are irritating." },
    { "-YouTubers are annoying.", "-Tron is underrated.", "-Baseball is failing." } };

    private string[] timeConditions = {"when the seconds digits form a multiple of 7",
                    "when the seconds digits match each other",
                    "when the seconds digits are both prime numbers",
                    "when the seconds digits add to 10 or greater",
                    "when the seconds digits can divide into 100",
                    "when the seconds digits are both odd or both even",
                    "when the seconds digits have a 6 in the last position",
                    "when the seconds digits have a difference of less than 4",
                    "when the seconds digits display ##:21",
                    "at any time"};

    private Dictionary<string, int> vars = new Dictionary<string, int>()
    {
        {"M", 0}, {"E", 0}, {"V", 0}, {"I", 0},
        {"L", 0}, {"U", 0}, {"B", 0}, {"D", 0},
        {"S", 0}, {"A", 0}, {"P", 0}, {"N", 0},
        {"W", 0}, {"X", 0}, {"Y", 0}, {"Z", 0}

    };

    private Dictionary<string, int> daysOfTheWeek = new Dictionary<string, int>()
    {
        {"Sunday", 1}, {"Monday", 2}, {"Tuesday", 3}, {"Wednesday", 4}, {"Thursday", 5}, {"Friday", 6}, {"Saturday", 7}
    };

    void Awake()
    {
        ModuleId = ModuleIdCounter++;
        foreach (KMSelectable button in buttons) {
            button.OnInteract += delegate () { buttonPress(button); return false; };
        }

        twitterLogo.OnInteract += delegate () { twitterPress(); return false; };

    }


    void Start()
    {
        setup();
        determineVariables();
        personChosenIndex = UnityEngine.Random.Range(0, 9);
        personChosenName = peopleNames[personChosenIndex];

        for (int i = 0; i < iconRenderers.Length; i++)
        {
            iconRenderers[i].material = peopleImgs[personChosenIndex];
        }

        determineCloutValue();
        Debug.LogFormat("[Twitter.com #{0}] You are tweeting from {1}'s account and your clout value is {2}.", ModuleId, personChosenName, cloutValue);

        List<string> invalidMessages = new List<string>();
        invalidMessages.Add(messages[cloutValue, 0]);
        invalidMessages.Add(messages[cloutValue, 1]);
        invalidMessages.Add(messages[cloutValue, 2]);

        correctButton = UnityEngine.Random.Range(0, 5);

        for (int i = 0; i < buttons.Length; i++)
        {
            if (i == correctButton)
            {
                messageTexts[i].text = messages[cloutValue, UnityEngine.Random.Range(0, 3)];
                continue;
            }
            string messageContents = messages[UnityEngine.Random.Range(0, 10), UnityEngine.Random.Range(0, 3)];
            while (invalidMessages.Contains(messageContents))
            {
                messageContents = messages[UnityEngine.Random.Range(0, 10), UnityEngine.Random.Range(0, 3)];
            }
            messageTexts[i].text = messageContents;
            invalidMessages.Add(messageContents);

        }

        Debug.LogFormat("[Twitter.com #{0}] You should press button {1} {2}. This button says \"{3}\"", ModuleId, correctButton + 1, timeConditions[cloutValue], messageTexts[correctButton].text);

    }

    void Update()
    {
    }

    void setup()
    {
        theModule.SetActive(false);
        twitterLogoGO.SetActive(true);
    }

    void twitterPress()
    {
        twitterLogoGO.SetActive(false);
        theModule.SetActive(true);
    }

    void buttonPress(KMSelectable button)
    {
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, button.transform);
        button.AddInteractionPunch();
        if (ModuleSolved)
        {
            return;
        }
        if (button == buttons[correctButton] && determinePressTiming() == true)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, button.transform);
            ModuleSolved = true;
            Module.HandlePass();
        }
        else
        {
            Module.HandleStrike();
            Debug.LogFormat("[Twitter.com #{0}] You have pressed button {1} at {2} which is wrong. Strike!", ModuleId, Array.IndexOf(buttons, button) + 1, Bomb.GetFormattedTime());
            Start();
        }
    }

    private bool determinePressTiming()
    {
        string fT = Bomb.GetFormattedTime();
        if (Bomb.GetTime() < 60)
        {
            fT = "00:" + Math.Floor(Bomb.GetTime()) % 60;
        }
        string seconds1 = fT.Substring(fT.Length - 2, 1);
        string seconds2 = fT.Substring(fT.Length - 1);
        int s1Int = Int32.Parse(seconds1);
        int s2Int = Int32.Parse(seconds2);
        switch (cloutValue)
        {
            case 0:
                if (Int32.Parse(fT.Substring(fT.Length - 2)) % 7 == 0)
                    return true;
                break;
            case 1:
                if (seconds1 == seconds2)
                    return true;
                break;
            case 2:
                if (new[] { "2", "3", "5", "7" }.Contains(seconds1) && new[] { "2", "3", "5", "7" }.Contains(seconds2))
                    return true;
                break;
            case 3:
                if (s1Int + s2Int >= 10)
                    return true;
                break;
            case 4:
                if (new[] { "1", "2", "4", "5", "10", "20", "25", "50", "100" }.Contains(seconds1 + seconds2))
                    return true;
                break;
            case 5:
                if ((s1Int % 2 == 0 && s2Int % 2 == 0) || (s1Int % 2 == 1 && s2Int % 2 == 1))
                    return true;
                break;
            case 6:
                if (s2Int == 6)
                    return true;
                break;
            case 7:
                if (Math.Abs(s2Int - s1Int) < 4)
                    return true;
                break;
            case 8:
                if (seconds1 + seconds2 == "21")
                    return true;
                break;
            case 9:
                return true;
            default:
                return false;
        }
        return false;
    }

    void determineVariables()
    {
        vars["M"] = Bomb.GetModuleNames().Count();
        vars["E"] = Bomb.GetSolvedModuleNames().Count();
        vars["V"] = vars["M"] - vars["E"];
        vars["I"] = Bomb.GetIndicators().Count();
        vars["L"] = Bomb.GetOnIndicators().Count();
        vars["U"] = Bomb.GetOffIndicators().Count();
        vars["B"] = Bomb.GetBatteryCount();
        vars["D"] = Bomb.GetBatteryCount(Battery.AA);
        vars["S"] = vars["B"] - vars["D"];
        vars["A"] = Bomb.GetPortPlateCount();
        vars["P"] = Bomb.GetPortCount();
        vars["N"] = (Bomb.GetSerialNumberNumbers().Sum() % 5) + 1;
        vars["W"] = daysOfTheWeek[DateTime.Now.DayOfWeek.ToString()];
        int[] algebraValues = determineAlgebraVars();
        vars["X"] = algebraValues[0] % 5;
        vars["Y"] = algebraValues[1];
        vars["Z"] = algebraValues[2];
    }

    private int[] determineAlgebraVars()
    {
        int valueX = 0;
        int valueY = 0;
        int valueZ = 0;

        int[] values = new int[3];

        int allModuleCount = Bomb.GetModuleNames().Count();
        int indicatorCount = Bomb.GetIndicators().Count();

        int baseX = Bomb.GetSerialNumberNumbers().Sum();
        int baseY = indicatorCount - Bomb.GetPortCount();
        int baseZ = allModuleCount + ((Bomb.GetBatteryCount(Battery.D)) * (Bomb.GetBatteryCount(Battery.AA)));

        //Find the actual values of x, y & z
        if (Bomb.GetBatteryHolderCount() > 2)
        {
            valueX += 2;
        }
        if (Bomb.GetPortCount(Port.RJ45) >= 1)
        {
            valueX -= 1;
        }
        if (Bomb.IsIndicatorOn("BOB"))
        {
            valueX += 4;
        }
        if (Bomb.GetSerialNumberLetters().Any(x => x == 'A' || x == 'E' || x == 'I' || x == 'O' || x == 'U'))
        {
            valueX -= 3;
        }
        valueX = valueX + baseX;


        if (Bomb.GetBatteryHolderCount() < 3)
        {
            valueY -= 2;
        }
        if (Bomb.GetPortCount(Port.Serial) >= 1)
        {
            valueY += 3;
        }
        if (Bomb.IsIndicatorOff("FRQ"))
        {
            valueY -= 5;
        }
        if (new[] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31 }.Contains(Bomb.GetSerialNumberNumbers().Sum()))
        {
            valueY += 4;
        }
        valueY = valueY + baseY;


        if (Bomb.GetBatteryHolderCount() < 1)
        {
            valueZ += 3;
        }
        if (Bomb.GetPortCount(Port.Parallel) >= 1)
        {
            valueZ -= 6;
        }
        if (Bomb.IsIndicatorOn("MSA"))
        {
            valueZ += 2;
        }
        if (Bomb.GetSerialNumberNumbers().Sum() % 3 == 0)

        {
            valueZ += 1;
        }
        valueZ = valueZ + baseZ;

        values[0] = valueX;
        values[1] = valueY;
        values[2] = valueZ;
        return values;
    }

    void determineCloutValue()
    {
        double cV;
        switch (personChosenIndex)
        {
            case 0:
                cV = vars["Z"];
                break;
            case 1:
                cV = vars["P"] - vars["D"];
                break;
            case 2:
                cV = (vars["E"] / (Math.Pow(vars["V"], vars["X"])));
                break;
            case 3:
                cV = (Math.Pow(vars["N"], vars["N"]));
                break;
            case 4:
                cV = vars["Y"] * (vars["L"] + vars["P"]);
                break;
            case 5:
                cV = vars["M"] / vars["W"];
                break;
            case 6:
                cV = vars["U"] * vars["S"] * vars["A"];
                break;
            case 7:
                cV = Math.Sqrt(vars["D"]);
                break;
            case 8:
                cV = vars["I"] + vars["B"];
                break;
            default:
                cV = 0;
                break;

        }
        double cVAbs = Math.Abs(cV);
        cloutValue = (int)Math.Truncate(cVAbs) % 10;
    }

#pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} <1-5> <##> [Presses button 1-5 from top to bottom when seconds digits are <##>.]";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string Command)
    {
        Command = Command.Trim().ToUpper();
        yield return null;
        string[] commands = Command.Split(' ');
        if (commands.Length != 2)
            yield break;
        if (!"12345".Contains(commands[0]) || commands[0].Length != 1 || commands[1].Length != 2 || Int32.Parse(commands[1]) > 60)
        {
            print("WE HAVE BROKEN");
            yield break;
        }
        while (Math.Floor(Bomb.GetTime() % 60) != Int32.Parse(commands[1]))
        {
            yield return "trycancel";
        }
        buttons[Int32.Parse(commands[0]) - 1].OnInteract();
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (!determinePressTiming())
        {
            yield return true;
        }
        buttons[correctButton].OnInteract();
        yield return new WaitForSeconds(0.1f);
    }
}
