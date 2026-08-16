using System.Collections;
using System.Collections.Generic;
using System.Security.Permissions;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;
public class AdCaller : MonoBehaviour
{


    public static AdCaller _inst;
    public static int _countAds = 0;
    public float time,takeTime;
    Coroutine co;
    private void Awake()
    {

        _inst = this;

        
    }
    public void Update()
    {
        if (isadshow == false)
        {
            time += Time.deltaTime;
            if (time > FirebaseHandler.adTimer)
            {
                isadshow = true;
            }
        }
    }
    public bool isadshow;
    bool flag = true;
   
    

    //fornextlevel
    public void callads()
    {
        if (isadshow == true)
        {
            callads2();
        }
    }
    public void ShowAds(bool val, Ping p)
    {
        if (val)
        {
            try
            {
                callads2();
            }
            catch
            {
                //catch
             
            }
        }
    }
   
    public void resetTime()
    {
        time = 0;
        isadshow = false;
    }
    public GameObject adloadingPanel;
    public Text loadingtxt;
    public IEnumerator _loadingAd()
    {
        Debug.Log("Loading" + isadshow);
        if (isadshow == true)
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                if (AdmobIntilization._instance.HasAdmobInterstialAvaible() || AdsManager.instance.isMaxReady())
                {
                    loadingtxt.text = "03";
                    adloadingPanel.SetActive(true);
                    yield return new WaitForSeconds(1f);
                    loadingtxt.text = "02";
                    yield return new WaitForSeconds(1f);
                    loadingtxt.text = "01";
                    yield return new WaitForSeconds(1);
                    adloadingPanel.SetActive(false);
                    callads2();
                }
            }
        }
    }
    public static void callads2()
    {
        if (PlayerPrefs.GetInt("NoAds") == 1) return;

        if (PlayerPrefs.GetInt("RemoveAds") == 0)
        {
            if (Application.internetReachability != NetworkReachability.NotReachable)
            {
                AdsManager.instance.ShowInterstitial();
            }
        }
    }

}
