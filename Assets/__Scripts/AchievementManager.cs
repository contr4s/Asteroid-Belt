using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchievementManager : MonoBehaviour
{
    // This is a somewhat protected private singleton for PlayerShip
    static private AchievementManager _S;
    static public AchievementManager S
    {
        get
        {
            return _S;
        }
        private set
        {
            if (_S != null)
            {
                Debug.LogWarning("Second attempt to set PlayerShip singleton _S.");
            }
            _S = value;
        }
    }

    [System.Serializable]
    public class Achievement
    {
        public string name;
        public string description;
        public bool complete;
        public ShipPart.eShipPartType partType; // The type of part unlocked by this Achievement
        public int partNum;

        public void Reach()
        {
            this.complete = true;
        }
    }
    public List<Achievement> Achievements = new List<Achievement>();

    public GameObject AchievemntPopUpDisplay;
    public Text achievementNameDisplay;
    public Text achievementDescriptionDisplay;
    public float durationOfShowingAchievement = 2f;
    private Animator anim;

    public static int shotsToReachTriggerHappy = 100;
    public int amountOfFiredBullets = 0;

    public static int scoreToReachRookiePilot = 10000;

    public static int luckyShotsToReachEagleEye = 100;
    public int amountOfLuckyShots = 0;

    public static int levelToReachSkillfullDodger = 5;

    Queue<int> achievementsQueue = new Queue<int>();

    void Awake()
    {
        S = this;
    }

    void Start()
    {
        Bullet.BULLET_FIRED_DELEGATE += AmountOfFiredBullets;
        Bullet.BULLET_HIT_ASTEROID_DELEGATE += FirstDust;
        Bullet.LUCKY_SHOT_DELEGATE += LuckyShot;
        GameManager.HIGH_SCORE_DELEGATE += RookiePilot;
        GameManager.HIGH_LEVEL_DELEGATE += SkillfullDodger;
        anim = AchievemntPopUpDisplay.GetComponent<Animator>();
        if (anim == null) // if Animator is missing
            Debug.LogError("Animator component missing from this gameobject");
        StartCoroutine("AchievementUnlocking");
    }

    IEnumerator AchievementUnlocking()
    {
        while (true)
        {
            if (achievementsQueue.Count > 0)
            {
                UnlockAchievement(achievementsQueue.Dequeue());
                yield return new WaitForSeconds(durationOfShowingAchievement + 1);
            }
            yield return new WaitForEndOfFrame();
        }
    }

    void UnlockAchievement(int achievementNumberInList)
    {
        if (achievementNumberInList == -1)
        {
            achievementNameDisplay.text = "HIGH SCORE";
            achievementDescriptionDisplay.text = "You've achieved a new high score.";
        }
        else
        {
            achievementNameDisplay.text = Achievements[achievementNumberInList].name;
            achievementDescriptionDisplay.text = Achievements[achievementNumberInList].description;
            ShipCustomizationPanel.UnlockPart(Achievements[achievementNumberInList].partType, Achievements[achievementNumberInList].partNum);
            CustomAnalytics.SendAchievementUnlocked(Achievements[achievementNumberInList]);
        }     
        anim.SetBool("Achievement appear", true);
        Invoke("AchievementDisappear", durationOfShowingAchievement);
        SaveGameManager.Save();
    }

    void AchievementDisappear()
    {
        anim.SetBool("Achievement appear", false);
    }


    void FirstDust()
    {
        if (!Achievements[0].complete)
        {
            achievementsQueue.Enqueue(0);
            //UnlockAchievement(0);
            Achievements[0].Reach();
        }          
    }

    void LuckyShot()
    {
        if (!Achievements[1].complete)
        {
            achievementsQueue.Enqueue(1);
            //UnlockAchievement(1);
            Achievements[1].Reach();
        }
        amountOfLuckyShots++;
        if (amountOfLuckyShots >= luckyShotsToReachEagleEye)
        {
            EagleEye();
        }
    }

    void TriggerHappy()
    {
        if (!Achievements[2].complete)
        {
            achievementsQueue.Enqueue(2);
            //UnlockAchievement(2);
            Achievements[2].Reach();
        }
    }

    void RookiePilot()
    {
        if (GameManager.score >= scoreToReachRookiePilot)
        {
            achievementsQueue.Enqueue(3);
            //UnlockAchievement(3);
            Achievements[3].Reach();
        }
        else
            achievementsQueue.Enqueue(-1);
    }

    void EagleEye()
    {
        if (!Achievements[4].complete)
        {
            achievementsQueue.Enqueue(4);
            //UnlockAchievement(4);
            Achievements[4].Reach();
        }
    }

    void SkillfullDodger()
    {
        if (!Achievements[5].complete)
        {
            achievementsQueue.Enqueue(5);
            //UnlockAchievement(5);
            Achievements[5].Reach();
        }
    }

    void AmountOfFiredBullets()
    {
        amountOfFiredBullets++;
        if (amountOfFiredBullets >= shotsToReachTriggerHappy) 
        {
            TriggerHappy();
        }       
    }

    public void LoadDataFromSaveFile(SaveFile saveFile)
    {
        // Handle StepRecords
        amountOfFiredBullets = saveFile.firedBullets;
        amountOfLuckyShots = saveFile.luckyShots;

        // Handle Achievements
        foreach (Achievement achSF in saveFile.achievements)
        {
            foreach (Achievement achAM in S.Achievements)
            {
                if (achSF.name == achAM.name)
                {
                    // This is the same Achievement
                    achAM.complete = achSF.complete;
                }
            }
        }
        S.UnlockPartsAfterLoadingGame();
    }

    public void ClearStepsAndAchievements()
    {
        // Clear the StepRecord progress
        amountOfFiredBullets = 0;
        amountOfLuckyShots = 0;

        // Clear Achievement completion
        foreach (Achievement ach in S.Achievements)
        {
            ach.complete = false;
        }
        S.UnlockPartsAfterLoadingGame();
    }

    void UnlockPartsAfterLoadingGame()
    {

        foreach (Achievement ach in Achievements)
        {
            if (ach.complete)
            {
                ShipCustomizationPanel.UnlockPart(ach.partType, ach.partNum);
            }
            else
            {
                ShipCustomizationPanel.LockPart(ach.partType, ach.partNum);
            }
        }

    }
}
