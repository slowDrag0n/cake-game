namespace SexyDevs.Ads
{
    /// <summary>
    /// Contract for any App Open Ad backend.
    /// AppOpenAdManager talks only to this — SDK is irrelevant to the caller.
    /// </summary>
    public interface IAppOpenProvider
    {
        /// <summary>Pre-fetch the next App Open Ad.</summary>
        void Load();

        /// <summary>Show the ad if loaded and not expired. No-op otherwise.</summary>
        void ShowIfReady();

        /// <summary>True while an App Open Ad is visible.</summary>
        bool IsShowingAOA { get; }
    }
}
