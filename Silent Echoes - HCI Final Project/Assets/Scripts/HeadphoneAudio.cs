using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class HeadphoneAudio : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float diminishedVolume = -20f;
    [SerializeField] private float normalVolume = 0f;
    [SerializeField] private float volumeTransitionSpeed = 5f;
    [SerializeField] private AudioSource feedbackAudio;
    [SerializeField] private AudioClip putOnSound;
    [SerializeField] private AudioClip takeOffSound;

    [SerializeField] private GameObject headphonesA;
    [SerializeField] private GameObject headphonesB;
    [SerializeField] private Transform playerHead;
    [SerializeField] private float wearDistance = 0.15f;
    [SerializeField] private float maxWearAngle = 45f;

    [SerializeField] private Material headphonesMaterial;
    [SerializeField] private Color wornColor = Color.green;
    [SerializeField] private Color normalColor = Color.white;

    private bool isHeadphonesAWorn = false;
    private bool isHeadphonesBWorn = false;
    private OVRGrabbable grabbableA;
    private OVRGrabbable grabbableB;
    private Transform headphonesATransform;
    private Transform headphonesBTransform;
    private float currentVolume;
    private float targetVolume;
    private Color currentColor;
    private string VolumeParameter = "MasterVolume";

    [SerializeField] private bool enableSmoothTransitions = true;
    [SerializeField] private bool checkOrientation = true;
    [SerializeField] private bool enableVisualFeedback = true;
    [SerializeField] private bool enableSoundFeedback = true;

    private void Start()
    {
        InitializeComponents();
        SetNormalVolume();
        currentVolume = normalVolume;
        targetVolume = normalVolume;
        currentColor = normalColor;
    }

    private void InitializeComponents()
    {
        grabbableA = headphonesA.GetComponent<OVRGrabbable>();
        grabbableB = headphonesB.GetComponent<OVRGrabbable>();
        headphonesATransform = headphonesA.transform;
        headphonesBTransform = headphonesB.transform;

        if (playerHead == null)
        {
            var centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
            if (centerEyeAnchor != null) playerHead = centerEyeAnchor.transform;
        }

        if (headphonesMaterial != null) headphonesMaterial.color = normalColor;
    }

    private void Update()
    {
        CheckHeadphonesPosition();
        UpdateVolumeTransition();
        UpdateVisualFeedback();
    }

    private void CheckHeadphonesPosition()
    {
        if (playerHead == null) return;

        bool previousAWornState = isHeadphonesAWorn;
        bool previousBWornState = isHeadphonesBWorn;

        isHeadphonesAWorn = IsHeadphoneWorn(headphonesATransform, grabbableA);
        isHeadphonesBWorn = IsHeadphoneWorn(headphonesBTransform, grabbableB);

        if (previousAWornState != isHeadphonesAWorn || previousBWornState != isHeadphonesBWorn)
        {
            UpdateAudioState();
            PlayFeedbackSound(isHeadphonesAWorn || isHeadphonesBWorn);
        }
    }

    private bool IsHeadphoneWorn(Transform headphoneTransform, OVRGrabbable grabbable)
    {
        float distance = Vector3.Distance(headphoneTransform.position, playerHead.position);
        bool isInRange = distance <= wearDistance && !grabbable.isGrabbed;

        if (!checkOrientation || !isInRange) return isInRange;

        float angle = Vector3.Angle(headphoneTransform.up, playerHead.up);
        return angle <= maxWearAngle;
    }

    private void UpdateAudioState()
    {
        targetVolume = isHeadphonesBWorn ? -80f :
                      isHeadphonesAWorn ? diminishedVolume :
                      normalVolume;

        if (!enableSmoothTransitions)
        {
            currentVolume = targetVolume;
            audioMixer.SetFloat(VolumeParameter, currentVolume);
        }
    }

    private void UpdateVolumeTransition()
    {
        if (!enableSmoothTransitions || Mathf.Approximately(currentVolume, targetVolume)) return;

        currentVolume = Mathf.Lerp(currentVolume, targetVolume, Time.deltaTime * volumeTransitionSpeed);
        audioMixer.SetFloat(VolumeParameter, currentVolume);
    }

    private void UpdateVisualFeedback()
    {
        if (!enableVisualFeedback || headphonesMaterial == null) return;

        Color targetColor = (isHeadphonesAWorn || isHeadphonesBWorn) ? wornColor : normalColor;
        currentColor = Color.Lerp(currentColor, targetColor, Time.deltaTime * volumeTransitionSpeed);
        headphonesMaterial.color = currentColor;
    }

    private void PlayFeedbackSound(bool putting0n)
    {
        if (!enableSoundFeedback || feedbackAudio == null) return;

        AudioClip clipToPlay = putting0n ? putOnSound : takeOffSound;
        if (clipToPlay != null)
        {
            feedbackAudio.PlayOneShot(clipToPlay);
        }
    }

    private void SetNormalVolume()
    {
        audioMixer.SetFloat(VolumeParameter, normalVolume);
    }
}
