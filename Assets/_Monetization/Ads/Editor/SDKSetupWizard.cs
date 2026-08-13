#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// SDK For Ads — Full setup wizard.
/// Auto-opens on first import. Reopen via: SexyDevs → SDK Setup Wizard
/// </summary>
public class SDKSetupWizard : EditorWindow
{
    // ─────────────────────────────────────────────────────────────────────
    // Auto-open
    // ─────────────────────────────────────────────────────────────────────
    private const string PrefKey = "SexyDevs_SDK_WizardShown_v1";

    [InitializeOnLoadMethod]
    private static void AutoOpen()
    {
        if (EditorPrefs.GetBool(PrefKey, false)) return;
        EditorApplication.delayCall += () => { if (!EditorPrefs.GetBool(PrefKey, false)) Open(); };
    }

    [MenuItem("SexyDevs/SDK Setup Wizard", priority = 0)]
    public static void Open()
    {
        var w = GetWindow<SDKSetupWizard>(true, "SDK For Ads — Setup Wizard", true);
        w.minSize = new Vector2(540, 720);
        w.maxSize = new Vector2(540, 900);
        w.Show();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Steps:  0=Prerequisites  1=Scenario  2=IDs  3=Validate
    // ─────────────────────────────────────────────────────────────────────
    private int _step = 0;
    private const int TotalSteps = 4;

    // Scenario
    private AdProvider _scenario = AdProvider.AdMobOnly;

    // AdMob IDs
    private string _admobInter  = "";
    private string _admobStatic = "";
    private string _admobRew    = "";
    private string _admobBanner = "";
    private string _admobMrec   = "";
    private string _admobAOA    = "";

    // MAX IDs
    private string _maxKey      = "";
    private string _maxInter    = "";
    private string _maxStatic   = "";
    private string _maxRew      = "";
    private string _maxBanner   = "";
    private string _maxMrec     = "";
    private string _maxAOA      = "";

    // Validation
    private List<string> _errors   = new();
    private List<string> _warnings = new();
    private bool _applied = false;

    // Styles
    private GUIStyle _titleStyle, _subtitleStyle, _bodyStyle, _stepLabelStyle;
    private GUIStyle _errorStyle, _warnStyle, _successStyle, _sectionStyle, _fieldLabelStyle;
    private GUIStyle _linkStyle, _smallBodyStyle;

    private static readonly Color Purple = new Color(0.58f, 0.35f, 0.95f);
    private static readonly Color Green  = new Color(0.28f, 0.78f, 0.42f);
    private static readonly Color Red    = new Color(0.90f, 0.28f, 0.28f);
    private static readonly Color Amber  = new Color(0.92f, 0.65f, 0.12f);
    private static readonly Color Blue   = new Color(0.18f, 0.62f, 0.98f);
    private static readonly Color Orange = new Color(0.98f, 0.42f, 0.18f);
    private static readonly Color Gray   = new Color(0.38f, 0.38f, 0.38f);

    private Vector2 _scroll;

    // ─────────────────────────────────────────────────────────────────────
    // Dependency detection — all checks in one place
    // ─────────────────────────────────────────────────────────────────────
    private static bool HasAdMob()     => System.IO.Directory.Exists(Application.dataPath + "/GoogleMobileAds");
    private static bool HasFirebase()  => System.IO.Directory.Exists(Application.dataPath + "/Firebase");
    private static bool HasAdjust()    => System.IO.Directory.Exists(Application.dataPath + "/Adjust");
    private static bool HasMaxFolder() => System.IO.Directory.Exists(Application.dataPath + "/MaxSdk")
                                       || System.IO.Directory.Exists(Application.dataPath + "/AppLovin");
    private static bool HasAdMobDefine() => GetDefines().Contains("ADMOB_SDK");
    private static bool HasMaxDefine()   => GetDefines().Contains("MAX_SDK");
    private static bool HasIAP()       => UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.unity.purchasing") != null
                                       || System.IO.Directory.Exists(Application.dataPath.Replace("/Assets","/Library/PackageCache").Split(':')[0] + "/Library/PackageCache");

    private static bool HasGoogleServicesJson() =>
        System.IO.Directory.GetFiles(Application.dataPath, "google-services.json",
            System.IO.SearchOption.AllDirectories).Length > 0;

    private static bool HasGoogleServicesPlist() =>
        System.IO.Directory.GetFiles(Application.dataPath, "GoogleService-Info.plist",
            System.IO.SearchOption.AllDirectories).Length > 0;

    /// <summary>True if the active build target is Android (needs google-services.json).</summary>
    private static bool NeedsGoogleServicesJson() => EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android;

    /// <summary>True if the active build target is iOS (needs GoogleService-Info.plist).</summary>
    private static bool NeedsGoogleServicesPlist() => EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS;

    private static bool HasStaleDefines()
    {
        var d = GetDefines();
        return d.Contains("gameanalytics") || d.Contains("GAMEANALYTICS");
    }

    private static List<string> GetDefines()
    {
        var buildTarget = UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup);
        var raw = PlayerSettings.GetScriptingDefineSymbols(buildTarget);
        return raw.Split(';').Select(d => d.Trim()).Where(d => d.Length > 0).ToList();
    }

    private static void AddDefine(string define)
    {
        var list = GetDefines();
        if (list.Contains(define)) return;
        list.Add(define);
        PlayerSettings.SetScriptingDefineSymbols(
            UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup),
            string.Join(";", list));
    }

    private static void RemoveDefine(string define)
    {
        var list = GetDefines().Where(d => d != define).ToList();
        PlayerSettings.SetScriptingDefineSymbols(
            UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup),
            string.Join(";", list));
    }

    private static void RemoveStaleDefines()
    {
        var list = GetDefines()
            .Where(d => !d.ToLower().Contains("gameanalytics"))
            .ToList();
        PlayerSettings.SetScriptingDefineSymbols(
            UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(
                EditorUserBuildSettings.selectedBuildTargetGroup),
            string.Join(";", list));
        Debug.Log("[SDK] Stale defines removed.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // OnEnable — pre-populate from scene
    // ─────────────────────────────────────────────────────────────────────
    private void OnEnable()
    {
        var mgr = FindFirstObjectByType<MediationAdsManager>();
        if (mgr == null) return;
        var so = new SerializedObject(mgr);
        _scenario  = (AdProvider)so.FindProperty("adProvider").enumValueIndex;
        _admobInter  = so.FindProperty("admobInterstitialId").stringValue;
        _admobStatic = so.FindProperty("admobStaticInterstitialId").stringValue;
        _admobRew    = so.FindProperty("admobRewardedId").stringValue;
        _admobBanner = so.FindProperty("admobBannerId").stringValue;
        _admobMrec   = so.FindProperty("admobMrecId").stringValue;
        _admobAOA    = so.FindProperty("admobAppOpenId").stringValue;
        _maxKey      = so.FindProperty("maxSdkKey").stringValue;
        _maxInter    = so.FindProperty("maxInterstitialId").stringValue;
        _maxStatic   = so.FindProperty("maxStaticInterstitialId").stringValue;
        _maxRew      = so.FindProperty("maxRewardedId").stringValue;
        _maxBanner   = so.FindProperty("maxBannerId").stringValue;
        _maxMrec     = so.FindProperty("maxMrecId").stringValue;
        _maxAOA      = so.FindProperty("maxAppOpenId").stringValue;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Styles
    // ─────────────────────────────────────────────────────────────────────
    private void InitStyles()
    {
        if (_titleStyle != null) return;
        _titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 20, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
        _subtitleStyle = new GUIStyle(EditorStyles.label)  { fontSize = 11, alignment = TextAnchor.MiddleCenter, wordWrap = true, normal = { textColor = new Color(0.75f,0.75f,0.75f) } };
        _bodyStyle = new GUIStyle(EditorStyles.label)      { fontSize = 11, wordWrap = true, normal = { textColor = new Color(0.85f,0.85f,0.85f) } };
        _smallBodyStyle = new GUIStyle(EditorStyles.label) { fontSize = 10, wordWrap = true, normal = { textColor = new Color(0.65f,0.65f,0.65f) } };
        _stepLabelStyle = new GUIStyle(EditorStyles.miniLabel) { alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.55f,0.55f,0.55f) } };
        _errorStyle   = new GUIStyle(EditorStyles.label) { fontSize = 11, wordWrap = true, normal = { textColor = Red } };
        _warnStyle    = new GUIStyle(EditorStyles.label) { fontSize = 11, wordWrap = true, normal = { textColor = Amber } };
        _successStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 14, alignment = TextAnchor.MiddleCenter, normal = { textColor = Green } };
        _sectionStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 11, normal = { textColor = new Color(0.9f,0.9f,0.9f) } };
        _fieldLabelStyle = new GUIStyle(EditorStyles.label) { fontSize = 10, normal = { textColor = new Color(0.65f,0.65f,0.65f) } };
        _linkStyle = new GUIStyle(EditorStyles.label) { fontSize = 10, normal = { textColor = new Color(0.4f,0.7f,1f) } };
    }

    // ─────────────────────────────────────────────────────────────────────
    // OnGUI
    // ─────────────────────────────────────────────────────────────────────
    private void OnGUI()
    {
        InitStyles();
        DrawHeader();
        DrawStepBar();
        GUILayout.Space(8);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        switch (_step)
        {
            case 0: DrawStepPrerequisites(); break;
            case 1: DrawStepScenario();      break;
            case 2: DrawStepIDs();           break;
            case 3: DrawStepValidate();      break;
        }
        EditorGUILayout.EndScrollView();
        GUILayout.FlexibleSpace();
        DrawFooter();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Header
    // ─────────────────────────────────────────────────────────────────────
    private void DrawHeader()
    {
        var rect = EditorGUILayout.GetControlRect(false, 64);
        EditorGUI.DrawRect(rect, Purple * (EditorGUIUtility.isProSkin ? 0.55f : 0.75f));
        EditorGUI.LabelField(new Rect(rect.x, rect.y + 8, rect.width, 28), "SDK For Ads", _titleStyle);
        EditorGUI.LabelField(new Rect(rect.x, rect.y + 38, rect.width, 20), "Monetization Plugin — SexyDevs", _subtitleStyle);
    }

    // ─────────────────────────────────────────────────────────────────────
    // Step bar
    // ─────────────────────────────────────────────────────────────────────
    private void DrawStepBar()
    {
        GUILayout.Space(6);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        string[] labels = { "Prerequisites", "Scenario", "Ad IDs", "Validate" };
        for (int i = 0; i < TotalSteps; i++)
        {
            bool active = i == _step;
            bool done   = i < _step;
            Color col   = done ? Green : (active ? Purple : Gray);
            var r = EditorGUILayout.GetControlRect(false, 22, GUILayout.Width(104));
            EditorGUI.DrawRect(r, col * (EditorGUIUtility.isProSkin ? 0.7f : 0.85f));
            string lbl = done ? $"✓ {labels[i]}" : labels[i];
            var s = new GUIStyle(_stepLabelStyle) { normal = { textColor = (active || done) ? Color.white : new Color(0.5f,0.5f,0.5f) }, fontStyle = active ? FontStyle.Bold : FontStyle.Normal };
            EditorGUI.LabelField(r, lbl, s);
            if (i < TotalSteps - 1) { var sep = EditorGUILayout.GetControlRect(false, 22, GUILayout.Width(8)); EditorGUI.DrawRect(new Rect(sep.x+3, sep.y+10, 2, 2), new Color(0.38f,0.38f,0.38f)); }
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(4);
    }

    // ─────────────────────────────────────────────────────────────────────
    // STEP 0 — Prerequisites
    // ─────────────────────────────────────────────────────────────────────
    private void DrawStepPrerequisites()
    {
        GUILayout.Space(12);
        EditorGUILayout.LabelField("Prerequisites", new GUIStyle(_titleStyle) { fontSize = 16 });
        GUILayout.Space(4);
        EditorGUILayout.LabelField(
            "These are required regardless of which scenario you choose.",
            new GUIStyle(_bodyStyle) { alignment = TextAnchor.MiddleCenter });
        GUILayout.Space(8);

        const string demoScenePath = "Assets/_Monetization/Scenes/DemoScene.unity";
        bool demoSceneExists = System.IO.File.Exists(Application.dataPath.Replace("Assets", "") + demoScenePath);
        if (demoSceneExists)
        {
            DrawCard(() =>
            {
                DrawSectionLabel("Demo Scene", Blue);
                GUILayout.Space(2);
                EditorGUILayout.LabelField(
                    "DemoScene.unity is the reference implementation — a working MediationAdsManager wired to UI buttons for every ad format. Open it to see the SDK in action.",
                    _smallBodyStyle);
                GUILayout.Space(4);
                if (GUILayout.Button("Open Demo Scene", GUILayout.Height(24)))
                {
                    if (EditorApplication.isPlaying) { Debug.LogWarning("[SDK] Exit Play Mode before opening a scene."); }
                    else
                    {
                        UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(demoScenePath);
                    }
                }
            });
            GUILayout.Space(5);
        }

        DrawDependencyCard(
            "Firebase (Analytics + Remote Config)",
            "Required for Remote Config and analytics. Ads won't initialize without it.",
            "https://firebase.google.com/docs/unity/setup",
            HasFirebase(),
            "Installed — Assets/Firebase found",
            "Not found — import FirebaseAnalytics.unitypackage and FirebaseRemoteConfig.unitypackage from the Firebase Unity SDK.",
            null, new Color(0.98f, 0.75f, 0.18f)
        );

        GUILayout.Space(5);

        bool hasGS      = HasGoogleServicesJson();
        bool hasGP      = HasGoogleServicesPlist();
        bool needsGS    = NeedsGoogleServicesJson();
        bool needsGP    = NeedsGoogleServicesPlist();
        string platform = EditorUserBuildSettings.activeBuildTarget.ToString();

        // Only the file relevant to the active build target counts toward the status dot
        bool platformConfigOk = (!needsGS || hasGS) && (!needsGP || hasGP);

        DrawDependencyCard(
            $"Firebase Config File ({platform})",
            "Connects Firebase to your app. Checked against your active Build Target — switch platforms in File → Build Settings to check the other one.",
            "https://console.firebase.google.com/",
            platformConfigOk,
            needsGS ? "google-services.json found — Android ready" :
            needsGP ? "GoogleService-Info.plist found — iOS ready" :
            "No config file required for this build target",
            needsGS ? "google-services.json missing — required for Android. Download from Firebase Console → Project Settings → Your Apps." :
            needsGP ? "GoogleService-Info.plist missing — required for iOS. Download from Firebase Console → Project Settings → Your Apps." :
            "Switch your active platform to Android or iOS to check the right config file.",
            null, new Color(0.98f, 0.75f, 0.18f)
        );

        GUILayout.Space(5);

                DrawDependencyCard(
            "Adjust SDK",
            "Optional — used for ad revenue tracking (ROAS/LTV) if you want it. Not required for ads to function.",
            null,
            HasAdjust(),
            "Installed — Assets/Adjust found",
            "Not imported — click below to download. The SDK works fine without it; this only adds revenue tracking.",
            HasAdjust() ? null : (System.Action)(() => {
                if (GUILayout.Button("Download Adjust SDK", GUILayout.Height(24)))
                    Application.OpenURL("https://github.com/adjust/unity_sdk/releases");
            }), Green
        );;

        GUILayout.Space(5);

        bool hasIAP = IsPackageInstalled("com.unity.purchasing");
        DrawDependencyCard(
            "Unity In-App Purchasing (IAP)",
            "Required for InappManager. Install via Window → Package Manager → In App Purchasing.",
            "com.unity.purchasing",
            hasIAP,
            "Installed — com.unity.purchasing found in manifest",
            "Not found — open Package Manager and install 'In App Purchasing'.",
            hasIAP ? null : (System.Action)(() => {
                if (GUILayout.Button("Open Package Manager", GUILayout.Height(24)))
                    EditorApplication.ExecuteMenuItem("Window/Package Manager");
            }), Blue
        );

        GUILayout.Space(5);

        if (HasStaleDefines())
        {
            DrawCard(() =>
            {
                DrawSectionLabel("⚠  Stale scripting defines detected", Amber);
                GUILayout.Space(2);
                EditorGUILayout.LabelField(
                    "Old GameAnalytics defines are still present. These can cause compile warnings.", _bodyStyle);
                GUILayout.Space(4);
                if (GUILayout.Button("Remove Stale Defines", GUILayout.Height(24)))
                { RemoveStaleDefines(); Repaint(); }
            });
            GUILayout.Space(5);
        }

        GUILayout.Space(4);
        if (GUILayout.Button("↺  Refresh checks", GUILayout.Height(26))) Repaint();
    }

    // ─────────────────────────────────────────────────────────────────────
    // STEP 1 — Scenario
    // ─────────────────────────────────────────────────────────────────────
    private void DrawStepScenario()
    {
        GUILayout.Space(12);
        EditorGUILayout.LabelField("Choose Your Scenario", new GUIStyle(_titleStyle) { fontSize = 16 });
        GUILayout.Space(8);

        DrawScenarioOption(AdProvider.AdMobOnly,
            "AdMob Only",
            "AdMob handles everything — Banner, MREC, Interstitial, Rewarded, and App Open.",
            "Only AdMob IDs required.",
            Blue);

        GUILayout.Space(5);

        DrawScenarioOption(AdProvider.AppLovinOnly,
            "AppLovin MAX Only",
            "AppLovin MAX handles everything — Banner, MREC, Interstitial, Rewarded, and App Open.",
            "MAX SDK Key + Ad Unit IDs required.",
            Orange);

        GUILayout.Space(5);

        DrawScenarioOption(AdProvider.AppLovinWithAdMobAOA,
            "AppLovin MAX + AdMob App Open",
            "AppLovin MAX for Banner, MREC, Interstitial, and Rewarded.\nAdMob for App Open Ads only.",
            "Both MAX and AdMob IDs required.",
            Purple);

        GUILayout.Space(5);

        DrawScenarioOption(AdProvider.AppLovinWithAdMobAOAAndMREC,
            "AppLovin MAX + AdMob App Open, MREC",
            "AppLovin MAX for Banner, Interstitial, and Rewarded.\nAdMob for App Open Ads and MREC.",
            "Both MAX and AdMob IDs required.",
            new Color(0.72f, 0.28f, 0.72f));
    }

    private void DrawScenarioOption(AdProvider scenario, string title, string desc, string note, Color accent)
    {
        bool selected = _scenario == scenario;
        Color bg = selected
            ? accent * (EditorGUIUtility.isProSkin ? 0.25f : 0.32f)
            : new Color(0.2f, 0.2f, 0.2f, 1f);

        var rect = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(rect, bg);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3, rect.height), selected ? accent : Gray);
        GUILayout.Space(8);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(12);
        EditorGUILayout.BeginVertical();

        var titleCol = selected ? Color.white : new Color(0.72f, 0.72f, 0.72f);
        EditorGUILayout.LabelField(title, new GUIStyle(_sectionStyle) { normal = { textColor = titleCol } });
        EditorGUILayout.LabelField(desc, _bodyStyle);
        EditorGUILayout.LabelField(note, _fieldLabelStyle);


        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        var dot = EditorGUILayout.GetControlRect(false, 20, GUILayout.Width(26));
        EditorGUI.DrawRect(new Rect(dot.x+5, dot.y+4, 14, 14), new Color(0.14f,0.14f,0.14f));
        if (selected) EditorGUI.DrawRect(new Rect(dot.x+8, dot.y+7, 8, 8), accent);
        GUILayout.Space(8);
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(8);
        EditorGUILayout.EndVertical();

        if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition))
        {
            _scenario = scenario;
            Repaint();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // STEP 2 — IDs
    // ─────────────────────────────────────────────────────────────────────
    private void DrawStepIDs()
    {
        GUILayout.Space(12);
        EditorGUILayout.LabelField("Enter Ad Unit IDs", new GUIStyle(_titleStyle) { fontSize = 16 });
        GUILayout.Space(4);

        bool needsAdMob = NeedsAdMob(_scenario);
        bool needsMax   = NeedsMax(_scenario);
        bool admobAoaOnly = _scenario == AdProvider.AppLovinWithAdMobAOA;
        bool admobAoaAndMrec = _scenario == AdProvider.AppLovinWithAdMobAOAAndMREC;

        // ── SDK presence checks for this scenario ─────────────────────────
        if (needsAdMob && !HasAdMob())
        {
            DrawCard(() =>
            {
                DrawSectionLabel("✕  Google Mobile Ads SDK not found", Red);
                GUILayout.Space(2);
                EditorGUILayout.LabelField(
                    "Your selected scenario requires the AdMob SDK. Download the Google Mobile Ads Unity Plugin and import it before entering IDs.",
                    _bodyStyle);
                GUILayout.Space(4);
                if (GUILayout.Button("Open AdMob Unity Plugin docs", GUILayout.Height(24)))
                    Application.OpenURL("https://developers.google.com/admob/unity/quick-start");
            });
            GUILayout.Space(6);
        }
        else if (needsAdMob)
        {
            DrawCard(() => DrawSectionLabel("✓  Google Mobile Ads SDK installed", Green));
            GUILayout.Space(6);
        }

        if (needsMax && !HasMaxFolder())
        {
            DrawCard(() =>
            {
                DrawSectionLabel("✕  AppLovin MAX SDK not found", Red);
                GUILayout.Space(2);
                EditorGUILayout.LabelField(
                    "Your selected scenario requires the AppLovin MAX SDK. Download and import it, then re-open this wizard.",
                    _bodyStyle);
                GUILayout.Space(4);
                if (GUILayout.Button("Open AppLovin MAX docs", GUILayout.Height(24)))
                    Application.OpenURL("https://dash.applovin.com/documentation/mediation/unity/getting-started");
            });
            GUILayout.Space(6);
        }
        else if (needsMax)
        {
            DrawCard(() => DrawSectionLabel("✓  AppLovin MAX SDK installed", Green));
            GUILayout.Space(6);
        }

        // Scripting define checks
        if (needsAdMob)
        {
            bool defined = HasAdMobDefine();
            DrawCard(() =>
            {
                DrawSectionLabel(defined ? "✓  ADMOB_SDK define present" : "⚠  ADMOB_SDK define not found", defined ? Green : Amber);
                if (!defined)
                {
                    GUILayout.Space(2);
                    EditorGUILayout.LabelField("This define is added automatically when you click Apply. You can also add it now.", _bodyStyle);
                    GUILayout.Space(4);
                    if (GUILayout.Button("Add ADMOB_SDK Define Symbol", GUILayout.Height(26)))
                    { AddDefine("ADMOB_SDK"); Repaint(); }
                }
            });
            GUILayout.Space(6);
        }

        if (needsMax)
        {
            bool defined = HasMaxDefine();
            DrawCard(() =>
            {
                DrawSectionLabel(defined ? "✓  MAX_SDK define present" : "⚠  MAX_SDK define not found", defined ? Green : Amber);
                if (!defined)
                {
                    GUILayout.Space(2);
                    EditorGUILayout.LabelField("Import AppLovin MAX plugin first, then click below.", _bodyStyle);
                    GUILayout.Space(4);
                    if (GUILayout.Button("Add MAX_SDK Define Symbol", GUILayout.Height(26)))
                    { AddDefine("MAX_SDK"); Repaint(); }
                }
            });
            GUILayout.Space(6);
        }

        // AdMob section
        if (needsAdMob)
        {
            DrawCard(() =>
            {
                string sectionTitle = admobAoaOnly
                    ? "AdMob — App Open Only"
                    : admobAoaAndMrec
                        ? "AdMob — App Open & MREC"
                        : "AdMob — Ad Unit IDs";
                DrawSectionLabel(sectionTitle, Blue);
                GUILayout.Space(4);
                if (!admobAoaOnly && !admobAoaAndMrec)
                {
                    EditorGUILayout.LabelField("Full-screen", EditorStyles.miniBoldLabel);
                    _admobInter  = IDField("Interstitial",        _admobInter);
                    _admobStatic = IDField("Static Interstitial", _admobStatic);
                    _admobRew    = IDField("Rewarded",            _admobRew);
                    GUILayout.Space(6);
                    EditorGUILayout.LabelField("Display", EditorStyles.miniBoldLabel);
                    _admobBanner = IDField("Banner", _admobBanner);
                    _admobMrec   = IDField("MREC",   _admobMrec);
                    GUILayout.Space(6);
                }
                else if (admobAoaAndMrec)
                {
                    EditorGUILayout.LabelField("Display", EditorStyles.miniBoldLabel);
                    _admobMrec = IDField("MREC", _admobMrec);
                    GUILayout.Space(6);
                }
                EditorGUILayout.LabelField("App Open", EditorStyles.miniBoldLabel);
                _admobAOA = IDField("App Open", _admobAOA);
            });
            GUILayout.Space(6);
        }

        // MAX section
        if (needsMax)
        {
            DrawCard(() =>
            {
                DrawSectionLabel("AppLovin MAX — Ad Unit IDs", Orange);
                GUILayout.Space(4);
                EditorGUILayout.LabelField("SDK", EditorStyles.miniBoldLabel);
                _maxKey = IDField("SDK Key", _maxKey);
                GUILayout.Space(6);
                EditorGUILayout.LabelField("Full-screen", EditorStyles.miniBoldLabel);
                _maxInter  = IDField("Interstitial",        _maxInter);
                _maxStatic = IDField("Static Interstitial", _maxStatic);
                _maxRew    = IDField("Rewarded",            _maxRew);
                GUILayout.Space(6);
                EditorGUILayout.LabelField("Display", EditorStyles.miniBoldLabel);
                _maxBanner = IDField("Banner", _maxBanner);
                if (!admobAoaAndMrec)
                    _maxMrec = IDField("MREC", _maxMrec);
                if (!admobAoaOnly && !admobAoaAndMrec)
                {
                    GUILayout.Space(6);
                    EditorGUILayout.LabelField("App Open", EditorStyles.miniBoldLabel);
                    _maxAOA = IDField("App Open", _maxAOA);
                }
            });
        }

        GUILayout.Space(6);
        DrawCard(() =>
        {
            DrawSectionLabel("Where to find your IDs", Gray);
            GUILayout.Space(2);
            if (needsAdMob)
                if (GUILayout.Button("AdMob Console  →  apps.admob.com", _linkStyle, GUILayout.ExpandWidth(false)))
                    Application.OpenURL("https://apps.admob.com");
            if (needsMax)
                if (GUILayout.Button("AppLovin Dashboard  →  dash.applovin.com", _linkStyle, GUILayout.ExpandWidth(false)))
                    Application.OpenURL("https://dash.applovin.com");
        });
    }

    // ─────────────────────────────────────────────────────────────────────
    // STEP 3 — Validate & Apply
    // ─────────────────────────────────────────────────────────────────────
    private void DrawStepValidate()
    {
        GUILayout.Space(12);
        EditorGUILayout.LabelField("Validate & Apply", new GUIStyle(_titleStyle) { fontSize = 16 });
        GUILayout.Space(8);

        if (_applied)
        {
            DrawCard(() =>
            {
                GUILayout.Space(8);
                EditorGUILayout.LabelField("✓  Setup complete!", _successStyle);
                GUILayout.Space(4);
                EditorGUILayout.LabelField(
                    "MediationAdsManager has been configured.\nHit Play to test your integration.",
                    new GUIStyle(_bodyStyle) { alignment = TextAnchor.MiddleCenter });
                GUILayout.Space(8);
            });
            GUILayout.Space(8);
            if (GUILayout.Button("Close", GUILayout.Height(32))) Close();
            GUILayout.Space(4);
            if (GUILayout.Button("Run again", GUILayout.Height(24))) Reset();
            return;
        }

        Validate();

        if (_errors.Count > 0)
        {
            DrawCard(() =>
            {
                DrawSectionLabel("Errors — fix before applying", Red);
                GUILayout.Space(4);
                foreach (var e in _errors)
                    EditorGUILayout.LabelField($"✕  {e}", _errorStyle);
            });
            GUILayout.Space(6);
        }

        if (_warnings.Count > 0)
        {
            DrawCard(() =>
            {
                DrawSectionLabel("Warnings — optional", Amber);
                GUILayout.Space(4);
                foreach (var w in _warnings)
                    EditorGUILayout.LabelField($"⚠  {w}", _warnStyle);
            });
            GUILayout.Space(6);
        }

        DrawCard(() =>
        {
            DrawSectionLabel("Summary", Purple);
            GUILayout.Space(4);
            bool _needsAdMob = NeedsAdMob(_scenario);
            bool _needsMax   = NeedsMax(_scenario);

            EditorGUILayout.LabelField($"Scenario:        {ScenarioLabel(_scenario)}", _bodyStyle);
            if (_needsAdMob)
            {
                EditorGUILayout.LabelField($"AdMob SDK:       {(HasAdMob() ? "✓ installed" : "✕ not found")}", _bodyStyle);
                EditorGUILayout.LabelField($"ADMOB_SDK Define:{(HasAdMobDefine() ? "✓ present" : "⚠ added on Apply")}", _bodyStyle);
            }
            if (_needsMax)
            {
                EditorGUILayout.LabelField($"AppLovin MAX:    {(HasMaxFolder() ? "✓ installed" : "✕ not found")}", _bodyStyle);
                EditorGUILayout.LabelField($"MAX_SDK Define:  {(HasMaxDefine() ? "✓ present" : "✕ missing")}", _bodyStyle);
            }
            EditorGUILayout.LabelField($"Firebase:        {(HasFirebase() ? "✓ installed" : "✕ not found")}", _bodyStyle);
            EditorGUILayout.LabelField($"Adjust:          {(HasAdjust() ? "✓ installed" : "✕ not found")}", _bodyStyle);
            EditorGUILayout.LabelField($"google-services: {(HasGoogleServicesJson() ? "✓ found" : "✕ missing")}", _bodyStyle);
            EditorGUILayout.LabelField($"Stale defines:   {(HasStaleDefines() ? "⚠ present — clean up" : "✓ clean")}", _bodyStyle);
        });

        GUILayout.Space(8);

        // Fix stale defines directly from validate
        if (HasStaleDefines())
        {
            if (GUILayout.Button("Remove Stale Defines", GUILayout.Height(26)))
            { RemoveStaleDefines(); Repaint(); }
            GUILayout.Space(4);
        }

        GUI.enabled = _errors.Count == 0;
        if (GUILayout.Button("Apply to MediationAdsManager", GUILayout.Height(36)))
            ApplyToManager();
        GUI.enabled = true;

        if (_errors.Count > 0)
            EditorGUILayout.LabelField("Fix all errors before applying.",
                new GUIStyle(_errorStyle) { alignment = TextAnchor.MiddleCenter });
    }

    // ─────────────────────────────────────────────────────────────────────
    // Validation
    // ─────────────────────────────────────────────────────────────────────
    private void Validate()
    {
        _errors.Clear();
        _warnings.Clear();

        bool needsAdMob = NeedsAdMob(_scenario);
        bool needsMax   = NeedsMax(_scenario);
        bool admobAoaOnly = _scenario == AdProvider.AppLovinWithAdMobAOA;
        bool admobAoaAndMrec = _scenario == AdProvider.AppLovinWithAdMobAOAAndMREC;

        // Scene check — MediationAdsManager is auto-created on Apply if missing, so this is a notice, not a blocker
        if (FindFirstObjectByType<MediationAdsManager>() == null)
            _warnings.Add("MediationAdsManager not found in the active scene — a new one will be created automatically when you click Apply.");
        if (FindFirstObjectByType<FbaManager>() == null)
            _warnings.Add("FBAManager not found in the active scene — a new one will be created automatically when you click Apply. Required for Remote Config / ad init.");

        // Core SDKs
        if (!HasFirebase())
            _errors.Add("Firebase SDK not found. Download and import Firebase Unity SDK.");
        if (!HasAdjust())
            _warnings.Add("Adjust SDK not imported — optional, only needed for ad revenue tracking. Download it from the Prerequisites step if you want it.");

        // Firebase config file — only the one relevant to the active build target matters
        if (NeedsGoogleServicesJson() && !HasGoogleServicesJson())
            _warnings.Add("google-services.json not found. Firebase won't connect on Android (your active build target).");
        if (NeedsGoogleServicesPlist() && !HasGoogleServicesPlist())
            _warnings.Add("GoogleService-Info.plist not found. Firebase won't connect on iOS (your active build target).");

        if (HasStaleDefines())
            _warnings.Add("Stale GameAnalytics defines found. Use 'Remove Stale Defines' above.");

        // AdMob
        if (needsAdMob)
        {
            if (!HasAdMob()) _errors.Add("Google Mobile Ads SDK not found. Import it from the AdMob dashboard.");
            if (!admobAoaOnly && !admobAoaAndMrec)
            {
                if (Empty(_admobInter))  _errors.Add("AdMob Interstitial ID is empty.");
                if (Empty(_admobRew))    _errors.Add("AdMob Rewarded ID is empty.");
                if (Empty(_admobBanner)) _warnings.Add("AdMob Banner ID is empty — banner ads won't show.");
                if (Empty(_admobMrec))   _warnings.Add("AdMob MREC ID is empty — MREC won't show.");
                if (Empty(_admobStatic)) _warnings.Add("AdMob Static Interstitial ID is empty.");
            }
            else if (admobAoaAndMrec)
            {
                if (Empty(_admobMrec)) _warnings.Add("AdMob MREC ID is empty — MREC won't show.");
            }
            if (Empty(_admobAOA)) _warnings.Add("AdMob App Open ID is empty — App Open Ads won't show.");
        }

        // Defines — note: Apply sets these automatically, but show as errors if missing
        if (needsAdMob && !HasAdMobDefine())
            _warnings.Add("ADMOB_SDK define not set — will be added automatically on Apply.");

        // MAX
        if (needsMax)
        {
            if (!HasMaxFolder()) _errors.Add("AppLovin MAX SDK not found. Import the MAX Unity Plugin.");
            if (!HasMaxDefine()) _errors.Add("MAX_SDK scripting define missing. Add it on the IDs step.");
            if (Empty(_maxKey))    _errors.Add("AppLovin MAX SDK Key is empty.");
            if (Empty(_maxInter))  _errors.Add("AppLovin MAX Interstitial ID is empty.");
            if (Empty(_maxRew))    _errors.Add("AppLovin MAX Rewarded ID is empty.");
            if (Empty(_maxBanner)) _warnings.Add("AppLovin MAX Banner ID is empty — banner ads won't show.");
            if (!admobAoaAndMrec && Empty(_maxMrec))
                _warnings.Add("AppLovin MAX MREC ID is empty — MREC won't show.");
            if (!admobAoaOnly && !admobAoaAndMrec && Empty(_maxAOA))
                _warnings.Add("AppLovin MAX App Open ID is empty.");
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // Apply
    // ─────────────────────────────────────────────────────────────────────
    private const string MediationPrefabPath = "Assets/_Monetization/Ads/Prefabs/MediationAdsManager.prefab";

    /// <summary>
    /// Creates a MediationAdsManager in the active scene if none exists.
    /// Prefers instantiating the bundled prefab (full UI + AppOpenAdManager included).
    /// Falls back to a bare GameObject with just the required script components
    /// if the prefab is missing (e.g. it was deleted or this is a partial import).
    /// </summary>
    private MediationAdsManager CreateMediationAdsManager()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MediationPrefabPath);

        if (prefab != null)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, "Add MediationAdsManager");
            Debug.Log("[SDK] MediationAdsManager prefab instantiated into the scene.");
            return instance.GetComponent<MediationAdsManager>();
        }

        // Fallback — prefab missing, build a bare GameObject with the core components
        var go = new GameObject("MediationAdsManager");
        Undo.RegisterCreatedObjectUndo(go, "Add MediationAdsManager");
        go.AddComponent<MediationAdsManager>();
#if ADMOB_SDK
        go.AddComponent<GoogleMobileAds.Samples.GoogleMobileAdsConsentController>();
#endif
        go.AddComponent<AppOpenAdManager>();
        Debug.LogWarning("[SDK] MediationAdsManager.prefab not found at " + MediationPrefabPath +
            " — created a bare GameObject instead. UI panels (loading panel, banner background) were NOT set up automatically; assign them manually in the Inspector.");
        return go.GetComponent<MediationAdsManager>();
    }

    private const string FbaManagerPrefabPath = "Assets/_Monetization/Ads/Prefabs/FBAManager.prefab";

    /// <summary>
    /// Creates an FbaManager in the active scene if none exists. FbaManager drives Remote Config,
    /// which MediationAdsManager depends on to initialize ads — without it, ads never load.
    /// Prefers instantiating the bundled prefab. Falls back to a bare GameObject if missing.
    /// </summary>
    private FbaManager CreateFbaManager()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FbaManagerPrefabPath);

        if (prefab != null)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            Undo.RegisterCreatedObjectUndo(instance, "Add FBAManager");
            Debug.Log("[SDK] FBAManager prefab instantiated into the scene.");
            return instance.GetComponent<FbaManager>();
        }

        var go = new GameObject("FBAManager");
        Undo.RegisterCreatedObjectUndo(go, "Add FBAManager");
        go.AddComponent<FbaManager>();
        Debug.LogWarning("[SDK] FBAManager.prefab not found at " + FbaManagerPrefabPath +
            " — created a bare GameObject instead.");
        return go.GetComponent<FbaManager>();
    }

    private void ApplyToManager()
    {
        var mgr = FindFirstObjectByType<MediationAdsManager>();

        if (mgr == null)
        {
            mgr = CreateMediationAdsManager();
            if (mgr == null) { Debug.LogError("[SDK] Failed to create MediationAdsManager."); return; }
        }

        if (FindFirstObjectByType<FbaManager>() == null)
        {
            var fba = CreateFbaManager();
            if (fba == null) Debug.LogWarning("[SDK] Failed to create FBAManager — ads will not initialize without it.");
        }

        // Sync scripting defines with chosen scenario
        bool applyNeedsAdMob = NeedsAdMob(_scenario);
        bool applyNeedsMax   = NeedsMax(_scenario);

        if (applyNeedsAdMob) AddDefine("ADMOB_SDK"); else RemoveDefine("ADMOB_SDK");
        if (applyNeedsMax)   AddDefine("MAX_SDK");   else RemoveDefine("MAX_SDK");

        Debug.Log($"[SDK] Defines — ADMOB_SDK:{applyNeedsAdMob} MAX_SDK:{applyNeedsMax}");

        var so = new SerializedObject(mgr);
        so.FindProperty("adProvider").enumValueIndex             = (int)_scenario;
        so.FindProperty("admobInterstitialId").stringValue       = _admobInter;
        so.FindProperty("admobStaticInterstitialId").stringValue = _admobStatic;
        so.FindProperty("admobRewardedId").stringValue           = _admobRew;
        so.FindProperty("admobBannerId").stringValue             = _admobBanner;
        so.FindProperty("admobMrecId").stringValue               = _admobMrec;
        so.FindProperty("admobAppOpenId").stringValue            = _admobAOA;
        so.FindProperty("maxSdkKey").stringValue                 = _maxKey;
        so.FindProperty("maxInterstitialId").stringValue         = _maxInter;
        so.FindProperty("maxStaticInterstitialId").stringValue   = _maxStatic;
        so.FindProperty("maxRewardedId").stringValue             = _maxRew;
        so.FindProperty("maxBannerId").stringValue               = _maxBanner;
        so.FindProperty("maxMrecId").stringValue                 = _maxMrec;
        so.FindProperty("maxAppOpenId").stringValue              = _maxAOA;
        so.ApplyModifiedProperties();

        EditorUtility.SetDirty(mgr);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(mgr.gameObject.scene);

        // Persist the same values back onto the source prefab asset, not just the scene instance —
        // otherwise IDs are lost the moment this scene instance is deleted/recreated (e.g. by the
        // auto-create fallback above, or if a teammate re-drags the prefab into a different scene).
        var prefabInstanceRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(mgr.gameObject);
        if (prefabInstanceRoot != null)
        {
            try
            {
                PrefabUtility.ApplyPrefabInstance(prefabInstanceRoot, InteractionMode.AutomatedAction);
                Debug.Log("[SDK] Ad IDs and scenario also saved to MediationAdsManager.prefab.");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SDK] Could not apply changes back to the prefab asset: {e.Message}. Values are still set on the scene instance.");
            }
        }

        EditorPrefs.SetBool(PrefKey, true);
        _applied = true;
        Debug.Log($"[SDK] Setup applied — {ScenarioLabel(_scenario)}");
    }

    // ─────────────────────────────────────────────────────────────────────
    // Footer
    // ─────────────────────────────────────────────────────────────────────
    private void DrawFooter()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUI.enabled = _step > 0 && !_applied;
        if (GUILayout.Button("← Back", EditorStyles.toolbarButton, GUILayout.Width(70))) { _step--; _applied = false; }
        GUI.enabled = true;
        GUILayout.FlexibleSpace();
        GUILayout.Label($"Step {_step + 1} / {TotalSteps}", _stepLabelStyle, GUILayout.Width(70));
        if (GUILayout.Button("↺", EditorStyles.toolbarButton, GUILayout.Width(22))) Reset();
        GUILayout.FlexibleSpace();
        GUI.enabled = _step < TotalSteps - 1;
        if (GUILayout.Button("Next →", EditorStyles.toolbarButton, GUILayout.Width(70))) _step++;
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }

    // ─────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────
    private void Reset() { _applied = false; _step = 0; OnEnable(); Repaint(); }

    private void DrawDependencyCard(string title, string desc, string link, bool installed,
        string okMsg, string failMsg, System.Action extraAction, Color accent)
    {
        DrawCard(() =>
        {
            EditorGUILayout.BeginHorizontal();
            var dotRect = EditorGUILayout.GetControlRect(false, 16, GUILayout.Width(16));
            EditorGUI.DrawRect(new Rect(dotRect.x+1, dotRect.y+2, 12, 12), installed ? Green : new Color(0.4f,0.4f,0.4f));
            if (installed) EditorGUI.DrawRect(new Rect(dotRect.x+3, dotRect.y+4, 8, 8), Green);
            EditorGUILayout.BeginVertical();
            DrawSectionLabel(title, installed ? Green : new Color(0.85f,0.85f,0.85f));
            EditorGUILayout.LabelField(desc, _smallBodyStyle);
            GUILayout.Space(3);
            EditorGUILayout.LabelField(installed ? okMsg : failMsg,
                installed ? new GUIStyle(_smallBodyStyle){ normal={ textColor=Green } } : _warnStyle);
            if (!installed && !string.IsNullOrEmpty(link))
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Documentation: ", _fieldLabelStyle, GUILayout.Width(90));
                if (GUILayout.Button(link, _linkStyle, GUILayout.ExpandWidth(false)))
                    if (link.StartsWith("http")) Application.OpenURL(link);
                EditorGUILayout.EndHorizontal();
            }
            extraAction?.Invoke();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        });
    }

    private void DrawCard(System.Action content)
    {
        var rect = EditorGUILayout.BeginVertical();
        EditorGUI.DrawRect(rect, new Color(0.17f, 0.17f, 0.17f, 1f));
        GUILayout.Space(6);
        EditorGUILayout.BeginHorizontal(); GUILayout.Space(10);
        EditorGUILayout.BeginVertical();
        content();
        EditorGUILayout.EndVertical();
        GUILayout.Space(10); EditorGUILayout.EndHorizontal();
        GUILayout.Space(6);
        EditorGUILayout.EndVertical();
        GUILayout.Space(2);
    }

    private void DrawSectionLabel(string text, Color color) =>
        EditorGUILayout.LabelField(text, new GUIStyle(_sectionStyle) { normal = { textColor = color } });

    private string IDField(string label, string value)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(label, _fieldLabelStyle, GUILayout.Width(136));
        var style = new GUIStyle(EditorStyles.textField);
        if (!string.IsNullOrWhiteSpace(value)) style.normal.textColor = Green;
        string result = EditorGUILayout.TextField(value, style);
        EditorGUILayout.EndHorizontal();
        return result;
    }

    private static bool Empty(string s) => string.IsNullOrWhiteSpace(s);

    private static bool IsPackageInstalled(string packageId)
    {
        string projectRoot = Application.dataPath.Replace("/Assets", "");
        string manifest    = projectRoot + "/Packages/manifest.json";
        if (!System.IO.File.Exists(manifest)) return false;
        return System.IO.File.ReadAllText(manifest).Contains(packageId);
    }

    private static string Filled(string v) => Empty(v) ? "✕  empty" : "✓  set";

    private static bool NeedsAdMob(AdProvider p) =>
        p is AdProvider.AdMobOnly
            or AdProvider.AppLovinWithAdMobAOA
            or AdProvider.AppLovinWithAdMobAOAAndMREC;

    private static bool NeedsMax(AdProvider p) =>
        p is AdProvider.AppLovinOnly
            or AdProvider.AppLovinWithAdMobAOA
            or AdProvider.AppLovinWithAdMobAOAAndMREC;

    private static string ScenarioLabel(AdProvider p) => p switch
    {
        AdProvider.AdMobOnly                   => "AdMob Only",
        AdProvider.AppLovinOnly                => "AppLovin MAX Only",
        AdProvider.AppLovinWithAdMobAOA        => "AppLovin MAX + AdMob App Open",
        AdProvider.AppLovinWithAdMobAOAAndMREC => "AppLovin MAX + AdMob App Open, MREC",
        _                                      => p.ToString()
    };
}
#endif
