# SDK_For_Ads — Unity Plugin

> **Last documented:** June 29, 2026  
> **Maintained by:** SexyDevs (`SexyDevs.Utils` namespace)

---

## What Is This?

**SDK_For_Ads** is a **drop-in monetization plugin** for Unity games. It is not a game — it is a self-contained SDK that integrates ads, in-app purchases, analytics, and remote config behind a clean, unified API. A developer drops it into their project, sets one dropdown, fills in their ad unit IDs, and it works.

---

## Project Details

| Property | Value |
|---|---|
| **Project Name** | SDK_For_Ads |
| **Project Path** | `E:\Github\SDK_For_Ads` |
| **Active Scene** | `Assets/_Monetization/Scenes/DemoScene.unity` |
| **Core Namespace** | `SexyDevs.Utils` |
| **Assembly** | `Assembly-CSharp` |

---

## Developer Setup — 3 Steps

1. Set **Ad Provider** dropdown in the `MediationAdsManager` Inspector
2. Fill in the **ID fields** for that scenario (irrelevant fields are hidden automatically)
3. Hit **Play**

That's it. No code changes needed.

---

## Architecture Overview

All core systems are **Singletons** (`SexyDevs.Utils.Singleton<T>`) — one instance, persistent across scenes, globally accessible via `.Instance`.

```
                    ┌─────────────────────────┐
                    │       Your Game          │
                    └────────────┬────────────┘
                                 │
              ┌──────────────────┼──────────────────┐
              │                  │                  │
   ┌──────────▼──────┐  ┌────────▼────────┐  ┌─────▼──────────┐
   │ MediationAds    │  │  InappManager   │  │   FbaManager   │
   │    Manager      │  │  (IAP / Store)  │  │ (Firebase +    │
   │ (all ad types + │  └────────┬────────┘  │  Remote Config)│
   │  ad timer)      │           │           └────────────────┘
   └──────┬──────────┘  ┌────────▼────────┐
          │             │  InappRewards   │
          │             │ (grant rewards) │
          │             └─────────────────┘
          │
   ┌──────▼──────────────────────────────┐
   │         Provider Layer              │
   │  IAdProvider  |  IAppOpenProvider   │
   │                                     │
   │  AdMobProvider    AppLovinProvider  │
   │  AdMobAOA         AppLovinAOA       │
   └─────────────────────────────────────┘
```

---

## Ad Provider Scenarios

Set the **Ad Provider** field on `MediationAdsManager` in the Inspector:

| Scenario | Banner / Inter / Rewarded | App Open Ad |
|---|---|---|
| `AdMobOnly` | Google AdMob | Google AdMob |
| `AppLovinOnly` | AppLovin MAX | AppLovin MAX |
| `AppLovinWithAdMobAOA` | AppLovin MAX | Google AdMob |

> **To enable AppLovin scenarios:** import the AppLovin MAX Unity plugin, then add `MAX_SDK` to **Edit → Project Settings → Player → Scripting Define Symbols**.

---

## Core Systems & API

---

### `MediationAdsManager`
*Base: `Singleton<MediationAdsManager>`*  
*File: `Assets/_Monetization/Ads/Scripts/MediationAdsManager.cs`*

Central hub for all ad serving. Also owns the ad interval timer.

**Inspector fields:**

| Field | Description |
|---|---|
| `Ad Provider` | Scenario selector — the only field that changes per project |
| `Use Ad Timer` | Toggle interval gating on/off |
| `Loading Panel` | UI panel shown while a full-screen ad loads |
| `Banner BG Image` | Background shown when banner is visible |
| AdMob IDs | Interstitial, Static Interstitial, Rewarded, Banner, MREC, App Open |
| AppLovin IDs | SDK Key, Interstitial, Static Interstitial, Rewarded, Banner, MREC, App Open |

**Public API:**

```csharp
// Interstitials — gated by timer when useAdTimer is on
MediationAdsManager.Instance.ShowInterstitial(state => { });
MediationAdsManager.Instance.ShowStaticInterstitial(state => { });

// Rewarded — never gated by timer
MediationAdsManager.Instance.ShowRewarded(state => { });

// Banner / MREC
MediationAdsManager.Instance.ShowBanner();
MediationAdsManager.Instance.HideBanner();
MediationAdsManager.Instance.ShowMrec();
MediationAdsManager.Instance.HideMrec();

// Timer state (useful for greying out UI buttons)
bool ready = MediationAdsManager.Instance.CanShowInterstitial;
```

---

### Ad Timer (built into `MediationAdsManager`)

Two independent timers, both driven by Remote Config. Toggle with the **Use Ad Timer** checkbox.

| Timer | Remote Config Key | Default | Behaviour |
|---|---|---|---|
| `FirstAdInterval` | `first_ad_interval` | 60s | Session-start gate. No interstitial fires until this expires. Fires `OnFirstAdIntervalCompleted` when done. Never resets after that. |
| `AdsInterval` | `ad_interval` | 30s | Between-ad cooldown. Resets every time any full-screen ad closes (inter, static, or rewarded). |

**Session flow:**

```
App starts
    │
    ▼  [FirstAdInterval: 60s — no interstitials allowed]
    │
    ▼  OnFirstAdIntervalCompleted fires
    │
    ▼  [AdsInterval: 0s — first ad fires immediately]
    │
    ▼  Ad shown and closed → AdsInterval resets to 30s
    │
    ▼  [AdsInterval: 30s cooldown]
    │
    ▼  Next ad allowed
```

When `useAdTimer` is off, `ShowInterstitial()` always fires immediately.  
When gated, `ShowInterstitial()` logs the remaining time and returns silently — no crashes, no extra checks needed in game code.

---

### `AppOpenAdManager`
*Base: `Singleton<AppOpenAdManager>`*  
*File: `Assets/_Monetization/Ads/Scripts/AppOpenAdManager.cs`*

Manages App Open Ads. Provider is injected automatically by `MediationAdsManager` at init.

```csharp
// Call from OnApplicationFocus or AppStateEventNotifier:
AppOpenAdManager.Instance.ShowAdIfReady();

// Gate during gameplay:
AppOpenAdManager.Instance.canShowAD = false;
AppOpenAdManager.Instance.canShowAD = true;
```

---

### `InappManager`
*Base: `Singleton<InappManager>`*  
*File: `Assets/_Monetization/InApps/InappManager.cs`*

Wraps Unity IAP (`com.unity.purchasing` v5.1.2). Products configured in the Inspector via `InappManagerEditor`.

```csharp
InappManager.Instance.Purchase_product("remove_ads");
bool owned  = InappManager.Instance.IsProductOwned("remove_ads");
bool active = InappManager.Instance.IsSubscriptionActive("premium_sub");
```

---

### `InappRewards`
*Base: `MonoBehaviour`*  
*File: `Assets/_Monetization/InApps/InappRewards.cs`*

Reward callbacks wired to IAP products via the Inspector:

| Method | Trigger |
|---|---|
| `RemoveAds()` | Non-consumable purchased — grants ad-free permanently |
| `RemoveAdsExpired()` | Remove Ads subscription lapsed |
| `MegaBundle()` | Consumable bundle purchased |
| `MegaBundleExpired()` | Bundle subscription lapsed |
| `AddCurrency()` | Currency consumable purchased |

---

### `FbaManager`
*Base: `Singleton<FbaManager>`*  
*File: `Assets/_Monetization/Analytics/FBAManager.cs`*

Wraps Firebase Analytics and Firebase Remote Config. Fires `OnRemoteConfigUpdated` when values are ready.

```csharp
FbaManager.Instance.SendCustomEvent("level_complete");
T val   = FbaManager.Instance.GetRemoteConfigValue<T>(FbaManager.RemoteConfigKeys.AdsInterval);
bool ok = FbaManager.Instance.IsRemoteConfigReady;
```

---

### Remote Config Keys

| Key | Type | Default | Purpose |
|---|---|---|---|
| `ad_interval` | float | 30s | Between-ad cooldown |
| `first_ad_interval` | float | 60s | Session-start gate |
| `inter_ad_show` | bool | true | Whether to show interstitials |
| `inter_ad_trigger` | bool | true | Trigger condition |
| `inter_ad_type` | bool | false | Which type to show |
| `Inter_ad_start` | int | 1 | Level to start showing ads |
| `afk_interval` | float | 15s | Gameplay idle ad interval |
| `app_open_ad` | bool | true | AOA enabled |
| `app_open_ses` | bool | false | AOA per session |
| `resume_ad` | bool | true | Resume/foreground ad |
| `resume_ad_type` | bool | true | Resume ad type |
| `ban_large` | bool | true | Show MREC |
| `ad_loading` | bool | false | Show loading panel |
| `remove_ad_popup` | bool | false | Remove-ads popup |
| `remove_ad_level` | int | 7 | Level to show it |
| `rate_us` | bool | true | Rate Us prompt |
| `rate_us_level` | int | 20 | Level to show it |
| `push_notification` | bool | true | Push notifications |
| `low_end_device` | int | 2 | Low-end device threshold |
| `no_net_popup` | bool | false | No internet popup |
| `no_net_pos` | int | 5 | Level to show it |
| `iap_subscription` | bool | true | IAP subscription popup |
| `CanUseMemoryCheck` | bool | true | Enable memory guard |
| `MinMemoryCheck` | int | 400 | Min free MB to load ads |

---

## Provider Layer

Located in `Assets/_Monetization/Ads/Scripts/Providers/`

| File | Role |
|---|---|
| `IAdProvider.cs` | Interface every ad backend implements |
| `IAppOpenProvider.cs` | Interface every AOA backend implements |
| `AdMobProvider.cs` | AdMob — plain single-unit loading, Adjust revenue tracking |
| `AppLovinProvider.cs` | AppLovin MAX (compiled only with `MAX_SDK` define) |
| `AdMobAppOpenProvider.cs` | AdMob AOA — Scenarios 1 & 3 |
| `AppLovinAppOpenProvider.cs` | MAX AOA — Scenario 2 (requires `MAX_SDK`) |

---

## Ad Formats

| Format | Timer gated | Notes |
|---|---|---|
| Banner | No | Adaptive, bottom position |
| MREC | No | Medium rectangle, top position |
| Interstitial | Yes | Video full-screen |
| Static Interstitial | Yes | Image full-screen, separate unit ID |
| Rewarded | No | Reward callback on `AdState.Reward` |
| App Open | No | Managed by `AppOpenAdManager` |

---

## Ad State Enum

```csharp
public enum AdState
{
    Opening,   // ad is on screen
    Reward,    // reward earned (rewarded only)
    Closed,    // user dismissed
    Failed,    // show or load failed
    Canceled,  // timed out or blocked
    Loaded     // informational
}
```

---

## Events (`MonetizationEvents`)

```csharp
// Fire these from your game:
MonetizationEvents.Gameplay.OnLevelStart?.Invoke();
MonetizationEvents.Gameplay.OnLevelComplete?.Invoke();
MonetizationEvents.Gameplay.OnLevelFailed?.Invoke();
MonetizationEvents.Gameplay.OnLevelRestart?.Invoke();
MonetizationEvents.Gameplay.OnLevelSpawned?.Invoke();

// Fired by the SDK (subscribe in your game):
MonetizationEvents.Generic.OnRemoteConfigUpdated        // Remote Config ready
MonetizationEvents.Generic.OnFirstAdIntervalCompleted   // first-ad gate expired
MonetizationEvents.Generic.OnAdTimerReset               // any full-screen ad closed
MonetizationEvents.Generic.OnIAPInit                    // IAP store ready
MonetizationEvents.Generic.OnIAPPurchased               // purchase completed
MonetizationEvents.Generic.OnRemoveAdsPurchased         // remove ads granted
MonetizationEvents.Generic.OnShowAdsRemoved             // ads hidden
```

---

## Data (`MonetizationData`)

Static PlayerPrefs wrappers — accessible anywhere without a reference:

```csharp
MonetizationData.RemoveAds              // bool
MonetizationData.RemoveAdsPurchased     // bool
MonetizationData.SubscriptionPurchased  // bool
MonetizationData.CurrentLevel           // int
MonetizationData.SessionCounter         // int
MonetizationData.Currency               // int
MonetizationData.MegaBundle             // bool
```

---

## External SDKs (Non-UPM)

Imported as compiled assemblies — not tracked in `manifest.json`:

| SDK | Assemblies |
|---|---|
| **Google Mobile Ads (AdMob)** | `GoogleMobileAds`, `GoogleMobileAds.Core`, `GoogleMobileAds.Unity`, `GoogleMobileAds.Editor` |
| **Firebase** | `Firebase.App`, `Firebase.Analytics`, `Firebase.RemoteConfig`, `Firebase.TaskExtension`, `Firebase.Editor` |
| **Adjust** | `AdjustSdk` |

> `com.unity.ads` is **not used**. All ad serving goes through AdMob or AppLovin MAX.

---

## UPM Packages

| Package | Version | Purpose |
|---|---|---|
| `com.unity.purchasing` | 5.1.2 | In-App Purchasing |
| `com.unity.inputsystem` | 1.18.0 | UI input |
| `com.unity.visualscripting` | 1.9.7 | Visual Scripting |
| `com.unity.timeline` | 1.8.10 | Timeline |
| `com.unity.collab-proxy` | 2.11.3 | Version Control |
| `com.coplaydev.unity-mcp` | 9.7.3 | MCP for Unity (tooling) |
| `com.unity.ugui` | 2.0.0 | Unity UI |

---

## How to Integrate Into a Game

1. **Import** the `_Monetization` folder into your game project
2. **Add** `MediationAdsManager`, `AppOpenAdManager`, `FbaManager`, `InappManager`, and `CoroutineRunner` GameObjects to your persistent scene
3. **Set** the `Ad Provider` dropdown on `MediationAdsManager`
4. **Fill in** ad unit IDs for the chosen scenario
5. **Configure** IAP products in the `InappManager` Inspector
6. **Fire** `MonetizationEvents.Gameplay` events from your game — the SDK handles ads automatically
7. **Call** `AppOpenAdManager.Instance.ShowAdIfReady()` from `OnApplicationFocus`

### Enabling AppLovin MAX (Scenarios 1 & 2)

1. Import the AppLovin MAX Unity plugin from the AppLovin dashboard
2. **Edit → Project Settings → Player → Scripting Define Symbols** → add `MAX_SDK`
3. `AppLovinProvider` and `AppLovinAppOpenProvider` activate automatically — no other changes needed

---

## File Structure

```
Assets/
└── _Monetization/
    ├── Singleton.cs
    ├── CoroutineRunner.cs
    ├── MonetizationData.cs
    ├── MonetizationEvents.cs
    ├── ButtonTap.cs
    │
    ├── Ads/
    │   ├── Editor/
    │   │   └── MediationAdsManagerEditor.cs   ← custom Inspector (scenario-aware)
    │   ├── Prefabs/
    │   ├── Scripts/
    │   │   ├── MediationAdsManager.cs          ← main entry point + ad timer
    │   │   ├── AppOpenAdManager.cs
    │   │   ├── Interstitial.cs
    │   │   ├── Rewarded.cs
    │   │   ├── GoogleMobileAdsConsentController.cs
    │   │   └── Providers/
    │   │       ├── IAdProvider.cs
    │   │       ├── IAppOpenProvider.cs
    │   │       ├── AdMobProvider.cs
    │   │       ├── AppLovinProvider.cs         ← requires MAX_SDK define
    │   │       ├── AdMobAppOpenProvider.cs
    │   │       └── AppLovinAppOpenProvider.cs  ← requires MAX_SDK define
    │   └── SO/
    │
    ├── Analytics/
    │   └── FBAManager.cs
    │
    └── InApps/
        ├── Editor/
        │   └── InappManagerEditor.cs
        ├── Prefabs/
        ├── InappManager.cs
        ├── InappRewards.cs
        └── InAppButton.cs
```

---

*Generated from live Unity Editor reflection — June 29, 2026.*
