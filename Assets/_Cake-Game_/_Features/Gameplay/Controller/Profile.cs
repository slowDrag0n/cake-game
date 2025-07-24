using System;
using UnityEngine;

public static partial class Profile
{
    const string FIRST_TIME_FLAG = "FIRST_TIME_FLAG_KEY";
    const string BUY_ITEM_ID = "BUY_ITEM_ID";
    const string CASH_KEY = "CASH_KEY";
    const string LEVEL_KEY = "LEVEL_KEY";
    const string MAX_LEVEL_UNLOCKED = "MAX_LEVEL_UNLOCKED";
    const string WIN_STREAK_KEY = "WIN_STREAK_KEY";
    const string UNLOCKED_ITEM_LEVEL_KEY = "UNLOCKED_ITEM_LEVEL_KEY";
    const string SLOT_ITEM_KEY = "SLOT_ITEM_";

    const string SOUND_ENABLED = "SOUND_ENABLED";
    const string MUSIC_ENABLED = "MUSIC_ENABLED";
    const string HAPTICS_ENABLED = "HAPTICS_ENABLED";

    public static bool FirstTimeFlag
    {
        get
        {
            return PlayerPrefs.GetInt(FIRST_TIME_FLAG, 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(FIRST_TIME_FLAG, value ? 0 : 1);
        }
    }

    public static int BuyItemId
    {
        get
        {
            return PlayerPrefs.GetInt(BUY_ITEM_ID, 0);
        }
        set
        {
            PlayerPrefs.SetInt(BUY_ITEM_ID, value);
        }
    }

    //    public static int Cash
    //    {
    //        get
    //        {
    //#if UNITY_EDITOR
    //            return PlayerPrefs.GetInt(CASH_KEY, 999999999);
    //#else
    //            return PlayerPrefs.GetInt(CASH_KEY, 469);
    //#endif
    //        }
    //        set
    //        {
    //            PlayerPrefs.SetInt(CASH_KEY, value);
    //        }
    //    }

    public static uint Cash
    {
        get
        {
            if(PlayerPrefs.HasKey(CASH_KEY) == false)
                return 50000;

            // Retrieve the string from PlayerPrefs
            string stringValue = PlayerPrefs.GetString(CASH_KEY, string.Empty);

            if(string.IsNullOrEmpty(stringValue))
            {
                // Return a default value or handle the absence of the key as needed
                return 0;
            }

            // Convert the string back to bytes
            byte[] bytes = Convert.FromBase64String(stringValue);

            // Use BitConverter to convert the bytes back to a uint
            return BitConverter.ToUInt32(bytes, 0);
        }
        set
        {
            // Use BitConverter to convert the uint to bytes
            byte[] bytes = BitConverter.GetBytes(value);

            // Save the bytes to PlayerPrefs as a string
            PlayerPrefs.SetString(CASH_KEY, Convert.ToBase64String(bytes));
        }
    }

    //public static void Cash_Set(uint value)
    //{
    //    // Use BitConverter to convert the uint to bytes
    //    byte[] bytes = BitConverter.GetBytes(value);

    //    // Save the bytes to PlayerPrefs as a string
    //    PlayerPrefs.SetString(CASH_KEY, Convert.ToBase64String(bytes));
    //}

    //public static uint Cash_Get()
    //{
    //    if(PlayerPrefs.HasKey(CASH_KEY) == false)
    //        return 50000;

    //    // Retrieve the string from PlayerPrefs
    //    string stringValue = PlayerPrefs.GetString(CASH_KEY, string.Empty);

    //    if(string.IsNullOrEmpty(stringValue))
    //    {
    //        // Return a default value or handle the absence of the key as needed
    //        return 0;
    //    }

    //    // Convert the string back to bytes
    //    byte[] bytes = Convert.FromBase64String(stringValue);

    //    // Use BitConverter to convert the bytes back to a uint
    //    return BitConverter.ToUInt32(bytes, 0);
    //}

    public static int LevelsFinishedCounter;

    public static int Level
    {
        get
        {
            return PlayerPrefs.GetInt(LEVEL_KEY, 0);
        }
        set
        {
            int lv = value;
            if(lv > MaxLevelUnlocked) MaxLevelUnlocked = lv;
            PlayerPrefs.SetInt(LEVEL_KEY, lv);
        }
    }

    public static int MaxLevelUnlocked
    {
        get
        {
            return PlayerPrefs.GetInt(MAX_LEVEL_UNLOCKED, 0);
        }
        set
        {
            PlayerPrefs.SetInt(MAX_LEVEL_UNLOCKED, value);
        }
    }

    public static int WinStreak
    {
        get
        {
            return PlayerPrefs.GetInt(WIN_STREAK_KEY, 0);
        }
        set
        {
            PlayerPrefs.SetInt(WIN_STREAK_KEY, value);
        }
    }

    public static int UnlockedItemLevel
    {
        get
        {
            return PlayerPrefs.GetInt(UNLOCKED_ITEM_LEVEL_KEY, 0);
        }
        set
        {
            PlayerPrefs.SetInt(UNLOCKED_ITEM_LEVEL_KEY, value);
        }
    }

    public static void LevelAttemptCount_Set(int levelIndex, int attemptCount)
    {
        PlayerPrefs.SetInt("LEVEL_" + levelIndex + "_ATTEMPT_KEY", attemptCount);
    }

    public static int LevelAttemptCount_Get(int levelIndex)
    {
        return PlayerPrefs.GetInt("LEVEL_" + levelIndex + "_ATTEMPT_KEY", 0);
    }

    public static void SlotItem_Set(int slot, int value)
    {
        PlayerPrefs.SetInt(SLOT_ITEM_KEY + slot.ToString("0"), value);
    }

    public static int SlotItem_Get(int slot)
    {
        return PlayerPrefs.GetInt(SLOT_ITEM_KEY + slot.ToString("0"), -1);
    }

    #region SETTINGS


    public static bool SoundEnabled
    {
        get
        {
            return PlayerPrefs.GetInt(SOUND_ENABLED, 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(SOUND_ENABLED, value ? 1 : 0);
        }
    }
    public static bool MusicEnabled
    {
        get
        {
            return PlayerPrefs.GetInt(MUSIC_ENABLED, 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(MUSIC_ENABLED, value ? 1 : 0);
        }
    }
    public static bool HapticsEnabled
    {
        get
        {
            return PlayerPrefs.GetInt(HAPTICS_ENABLED, 1) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(HAPTICS_ENABLED, value ? 1 : 0);
        }
    }


    #endregion
}
