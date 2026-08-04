using System;
using UnityEngine.Events;

namespace SexyDevs.Ads
{
    /// <summary>
    /// Contract every ad network backend must implement.
    /// MediationAdsManager talks only to this interface — SDK underneath is irrelevant.
    /// </summary>
    public interface IAdProvider
    {
        void Initialize(Action onReady);

        void LoadInterstitial();
        void ShowInterstitial(UnityAction<AdState> callback);

        void LoadStaticInterstitial();
        void ShowStaticInterstitial(UnityAction<AdState> callback);

        void LoadRewarded();
        void ShowRewarded(UnityAction<AdState> callback);

        void LoadBanner();
        void ShowBanner();
        void HideBanner();

        void LoadMrec();
        void ShowMrec();
        void HideMrec();
    }
}
