using System;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Ump.Api;
using UnityEngine.Events;

public class GoogleUMPHandler : MonoBehaviour
{
    public UnityEvent OnGathered;

    // We pass an Action so we know exactly when to initialize the AdMob SDK
    public void GatherConsent(Action onConsentGathered)
    {
        var debugSettings = new ConsentDebugSettings
        {
            // Geography appears as in EEA for debug devices.
            DebugGeography = DebugGeography.EEA,
            TestDeviceHashedIds = new List<string>
            {
                "e7f20ad5-35f4-4e11-9b16-1b450a11f74e" // a50 ld player emulator
            }
        };

        // Create a ConsentRequestParameters object.
        ConsentRequestParameters request = new ConsentRequestParameters
        {
            ConsentDebugSettings = debugSettings,
        };

        // Check the current consent information status.
        ConsentInformation.Update(request, OnConsentInfoUpdated);
    }

    void OnConsentInfoUpdated(FormError consentError)
    {
        if(consentError != null)
        {
            Debug.LogError(consentError);
            // If an error occurs, check if we can still request ads using prior session consent.
            if(ConsentInformation.CanRequestAds())
            {
                //onConsentGathered?.Invoke();
                //OnGathered?.Invoke();
            }
            return;
        }

        // Load and show the consent form if required.
        ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
        {
            if(formError != null)
            {
                // Consent gathering failed.
                Debug.LogError($"Consent form error:{formError.ErrorCode}-{formError.Message}");
            }

            // Consent has been gathered or is not required. Check if we can request ads.
            if(ConsentInformation.CanRequestAds())
            {
                //onConsentGathered?.Invoke();
                //OnGathered?.Invoke();
            }
        });
    }
}