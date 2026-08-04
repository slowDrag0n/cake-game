#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Inspector for MediationAdsManager.
/// Shows only the fields relevant to the selected scenario.
/// Greyed-out / irrelevant fields are hidden entirely — no clutter.
/// </summary>
[CustomEditor(typeof(MediationAdsManager))]
public class MediationAdsManagerEditor : Editor
{
    // ── Serialized properties ─────────────────────────────────────────────
    SerializedProperty _adProvider;

    // UI
    SerializedProperty _loadingADPanel;
    SerializedProperty _bannerBgImg;

    // Timer
    SerializedProperty _useAdTimer;

    // AdMob
    SerializedProperty _admobInterstitialId;
    SerializedProperty _admobStaticInterstitialId;
    SerializedProperty _admobRewardedId;
    SerializedProperty _admobBannerId;
    SerializedProperty _admobMrecId;
    SerializedProperty _admobAppOpenId;

    // AppLovin
    SerializedProperty _maxSdkKey;
    SerializedProperty _maxInterstitialId;
    SerializedProperty _maxStaticInterstitialId;
    SerializedProperty _maxRewardedId;
    SerializedProperty _maxBannerId;
    SerializedProperty _maxMrecId;
    SerializedProperty _maxAppOpenId;

    // ── Styles (lazy-init) ────────────────────────────────────────────────
    GUIStyle _headerStyle;
    GUIStyle _boxStyle;
    GUIStyle _labelStyle;
    GUIStyle _noteStyle;

    static readonly Color ColAdMob    = new Color(0.18f, 0.62f, 0.98f, 1f);   // blue
    static readonly Color ColMax      = new Color(0.98f, 0.42f, 0.18f, 1f);   // applovin orange
    static readonly Color ColShared   = new Color(0.42f, 0.78f, 0.42f, 1f);   // green
    static readonly Color ColScenario = new Color(0.58f, 0.35f, 0.95f, 1f);   // purple

    void OnEnable()
    {
        _adProvider = serializedObject.FindProperty("adProvider");

        _loadingADPanel = serializedObject.FindProperty("loadingADPanel");
        _bannerBgImg    = serializedObject.FindProperty("bannerBgImg");
        _useAdTimer     = serializedObject.FindProperty("useAdTimer");

        _admobInterstitialId       = serializedObject.FindProperty("admobInterstitialId");
        _admobStaticInterstitialId = serializedObject.FindProperty("admobStaticInterstitialId");
        _admobRewardedId           = serializedObject.FindProperty("admobRewardedId");
        _admobBannerId             = serializedObject.FindProperty("admobBannerId");
        _admobMrecId               = serializedObject.FindProperty("admobMrecId");
        _admobAppOpenId            = serializedObject.FindProperty("admobAppOpenId");

        _maxSdkKey               = serializedObject.FindProperty("maxSdkKey");
        _maxInterstitialId       = serializedObject.FindProperty("maxInterstitialId");
        _maxStaticInterstitialId = serializedObject.FindProperty("maxStaticInterstitialId");
        _maxRewardedId           = serializedObject.FindProperty("maxRewardedId");
        _maxBannerId             = serializedObject.FindProperty("maxBannerId");
        _maxMrecId               = serializedObject.FindProperty("maxMrecId");
        _maxAppOpenId            = serializedObject.FindProperty("maxAppOpenId");
    }

    void InitStyles()
    {
        if (_headerStyle != null) return;

        _headerStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 11,
            fontStyle = FontStyle.Bold,
            normal    = { textColor = Color.white }
        };

        _boxStyle = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(10, 10, 6, 6),
            margin  = new RectOffset(0, 0, 4, 4)
        };

        _labelStyle = new GUIStyle(EditorStyles.label)
        {
            fontStyle = FontStyle.Normal,
            normal    = { textColor = new Color(0.85f, 0.85f, 0.85f) }
        };

        _noteStyle = new GUIStyle(EditorStyles.miniLabel)
        {
            wordWrap  = true,
            fontStyle = FontStyle.Italic,
            normal    = { textColor = new Color(0.65f, 0.65f, 0.65f) }
        };
    }

    public override void OnInspectorGUI()
    {
        InitStyles();
        serializedObject.Update();

        var scenario = (AdProvider)_adProvider.enumValueIndex;

        // ── Scenario selector ─────────────────────────────────────────────
        DrawColoredHeader("  Scenario", ColScenario);
        using (new EditorGUILayout.VerticalScope(_boxStyle))
        {
            EditorGUILayout.PropertyField(_adProvider, new GUIContent("Ad Provider"));
            EditorGUILayout.Space(2);
            DrawScenarioDescription(scenario);
        }

        EditorGUILayout.Space(6);

        // ── UI references (always visible) ───────────────────────────────
        DrawColoredHeader("  UI References", ColShared);
        using (new EditorGUILayout.VerticalScope(_boxStyle))
        {
            EditorGUILayout.PropertyField(_loadingADPanel, new GUIContent("Loading Panel"));
            EditorGUILayout.PropertyField(_bannerBgImg,    new GUIContent("Banner BG Image"));
        }

        EditorGUILayout.Space(6);

        // ── Timer ────────────────────────────────────────────────────────────
        DrawColoredHeader("  Ad Timer", new Color(0.85f, 0.55f, 0.10f, 1f));
        using (new EditorGUILayout.VerticalScope(_boxStyle))
        {
            EditorGUILayout.PropertyField(_useAdTimer, new GUIContent("Use Ad Timer"));
            if (_useAdTimer.boolValue)
                EditorGUILayout.LabelField(
                    "FirstAdInterval and AdsInterval are read from Remote Config.\nUse TryShowInterstitial() to respect the gate.",
                    _noteStyle, GUILayout.MinHeight(28));
        }

        EditorGUILayout.Space(6);

        // ── Scenario-specific fields ──────────────────────────────────────
        switch (scenario)
        {
            case AdProvider.AdMobOnly:
                DrawAdMobSection(showAppOpen: true, showBannerMrec: true);
                break;

            case AdProvider.AppLovinOnly:
                DrawMaxSection(showAppOpen: true, showBannerMrec: true);
                break;

            case AdProvider.AppLovinWithAdMobAOA:
                DrawMaxSection(showAppOpen: false, showBannerMrec: true);
                DrawAdMobAOAOnlySection();
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }

    // ── Section drawers ───────────────────────────────────────────────────

    void DrawAdMobSection(bool showAppOpen, bool showBannerMrec)
    {
        DrawColoredHeader("  AdMob — Ad Unit IDs", ColAdMob);
        using (new EditorGUILayout.VerticalScope(_boxStyle))
        {
            DrawSubHeader("Full-screen");
            EditorGUILayout.PropertyField(_admobInterstitialId,       new GUIContent("Interstitial"));
            EditorGUILayout.PropertyField(_admobStaticInterstitialId, new GUIContent("Static Interstitial"));
            EditorGUILayout.PropertyField(_admobRewardedId,           new GUIContent("Rewarded"));

            if (showBannerMrec)
            {
                EditorGUILayout.Space(4);
                DrawSubHeader("Display");
                EditorGUILayout.PropertyField(_admobBannerId, new GUIContent("Banner"));
                EditorGUILayout.PropertyField(_admobMrecId,   new GUIContent("MREC"));
            }

            if (showAppOpen)
            {
                EditorGUILayout.Space(4);
                DrawSubHeader("App Open");
                EditorGUILayout.PropertyField(_admobAppOpenId, new GUIContent("App Open"));
            }
        }
    }

    void DrawAdMobAOAOnlySection()
    {
        EditorGUILayout.Space(6);
        DrawColoredHeader("  AdMob — App Open Only", ColAdMob);
        using (new EditorGUILayout.VerticalScope(_boxStyle))
        {
            EditorGUILayout.LabelField(
                "This scenario uses AdMob exclusively for App Open Ads.\nAll other formats are handled by AppLovin MAX above.",
                _noteStyle);
            EditorGUILayout.Space(4);
            EditorGUILayout.PropertyField(_admobAppOpenId, new GUIContent("App Open ID"));
        }
    }

    void DrawMaxSection(bool showAppOpen, bool showBannerMrec)
    {
        DrawColoredHeader("  AppLovin MAX — Ad Unit IDs", ColMax);
        using (new EditorGUILayout.VerticalScope(_boxStyle))
        {
            DrawSubHeader("SDK");
            EditorGUILayout.PropertyField(_maxSdkKey, new GUIContent("SDK Key"));

            EditorGUILayout.Space(4);
            DrawSubHeader("Full-screen");
            EditorGUILayout.PropertyField(_maxInterstitialId,       new GUIContent("Interstitial"));
            EditorGUILayout.PropertyField(_maxStaticInterstitialId, new GUIContent("Static Interstitial"));
            EditorGUILayout.PropertyField(_maxRewardedId,           new GUIContent("Rewarded"));

            if (showBannerMrec)
            {
                EditorGUILayout.Space(4);
                DrawSubHeader("Display");
                EditorGUILayout.PropertyField(_maxBannerId, new GUIContent("Banner"));
                EditorGUILayout.PropertyField(_maxMrecId,   new GUIContent("MREC"));
            }

            if (showAppOpen)
            {
                EditorGUILayout.Space(4);
                DrawSubHeader("App Open");
                EditorGUILayout.PropertyField(_maxAppOpenId, new GUIContent("App Open"));
            }
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    void DrawColoredHeader(string title, Color color)
    {
        var rect = EditorGUILayout.GetControlRect(false, 22);
        EditorGUI.DrawRect(rect, color * (EditorGUIUtility.isProSkin ? 0.75f : 0.85f));
        EditorGUI.LabelField(rect, title, _headerStyle);
    }

    void DrawSubHeader(string title)
    {
        EditorGUILayout.LabelField(title, EditorStyles.miniBoldLabel);
    }

    void DrawScenarioDescription(AdProvider scenario)
    {
        string desc = scenario switch
        {
            AdProvider.AdMobOnly =>
                "AdMob handles everything — Banner, MREC, Interstitial, Rewarded, and App Open.\nOnly AdMob IDs required.",

            AdProvider.AppLovinOnly =>
                "AppLovin MAX handles everything — Banner, MREC, Interstitial, Rewarded, and App Open.\nOnly MAX IDs required.",

            AdProvider.AppLovinWithAdMobAOA =>
                "AppLovin MAX handles Banner, MREC, Interstitial, and Rewarded.\nAdMob handles App Open Ads only.\nBoth SDK keys required.",

            _ => ""
        };

        EditorGUILayout.LabelField(desc, _noteStyle, GUILayout.MinHeight(32));
    }
}
#endif
