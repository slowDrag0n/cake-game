using System;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Ump.Api;

public class GoogleUMPHandler : MonoBehaviour
{
    // We pass an Action so we know exactly when to initialize the AdMob SDK
    public void GatherConsent(Action onConsentGathered)
    {
        var debugSettings = new ConsentDebugSettings
        {
            // Geography appears as in EEA for debug devices.
            DebugGeography = DebugGeography.EEA,
            TestDeviceHashedIds = new List<string>
            {
                "TEST-DEVICE-HASHED-ID" // Replace with your test device ID
            }
        };

        // Create a ConsentRequestParameters object.
        ConsentRequestParameters request = new ConsentRequestParameters
        {
            ConsentDebugSettings = debugSettings,
        };

        // Check the current consent information status.
        ConsentInformation.Update(request, (FormError consentError) =>
        {
            if(consentError != null)
            {
                Debug.LogError(consentError);
                // If an error occurs, check if we can still request ads using prior session consent.
                if(ConsentInformation.CanRequestAds())
                {
                    onConsentGathered?.Invoke();
                }
                return;
            }

            // Load and show the consent form if required.
            ConsentForm.LoadAndShowConsentFormIfRequired((FormError formError) =>
            {
                if(formError != null)
                {
                    // Consent gathering failed.
                    Debug.LogError(formError);
                }

                // Consent has been gathered or is not required. Check if we can request ads.
                if(ConsentInformation.CanRequestAds())
                {
                    onConsentGathered?.Invoke();
                }
            });
        });
    }
}