using System.Collections.Generic;
using UnityEngine;

public static class MonetizationData
{
    // ================== GENERAL ==================

    public static bool RemoveAds
    {
        get => PlayerPrefs.GetInt("RemoveAds", 0) == 1;
        set
        {
            PlayerPrefs.SetInt("RemoveAds", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool RemoveAdsPurchased
    {
        get => PlayerPrefs.GetInt("isRemoveAdsPurchased", 0) == 1;
        set
        {
            PlayerPrefs.SetInt("isRemoveAdsPurchased", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static bool SubscriptionPurchased
    {
        get => PlayerPrefs.GetInt("IsSubscriptionPurchased", 0) == 1;
        set
        {
            PlayerPrefs.SetInt("IsSubscriptionPurchased", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public static int CurrentLevel
    {
        get => PlayerPrefs.GetInt("CurrentLevel", 0);
        set => PlayerPrefs.SetInt("CurrentLevel", value);
    }

    public static int SessionCounter
    {
        get => PlayerPrefs.GetInt("SessionCounter", 0);
        set => PlayerPrefs.SetInt("SessionCounter", value);
    }

    public static int Currency
    {
        get => PlayerPrefs.GetInt("Currency", 0);
        set => PlayerPrefs.SetInt("Currency", value);
    }

    public static bool MegaBundle
    {
        get => PlayerPrefs.GetInt("MegaBundle", 0) == 1;
        set => PlayerPrefs.SetInt("MegaBundle", value ? 1 : 0);
    }
}