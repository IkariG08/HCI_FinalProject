using UnityEngine;
using UnityEngine.Audio;

public class HeadphonesAudioController : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float diminishedVolume = -20f;
    [SerializeField] private float normalVolume = 0f;

    [SerializeField] private GameObject headphonesA;
    [SerializeField] private GameObject headphonesB;
    [SerializeField] private Transform playerHead;
    [SerializeField] private float wearDistance = 0.15f;

    private bool isHeadphonesAWorn = false;
    private bool isHeadphonesBWorn = false;
    private OVRGrabbable grabbableA;
    private OVRGrabbable grabbableB;
    private Transform headphonesATransform;
    private Transform headphonesBTransform;

    private void Start()
    {
        grabbableA = headphonesA.GetComponent<OVRGrabbable>();
        grabbableB = headphonesB.GetComponent<OVRGrabbable>();
        headphonesATransform = headphonesA.transform;
        headphonesBTransform = headphonesB.transform;

        if (playerHead == null)
        {
            var centerEyeAnchor = GameObject.Find("CenterEyeAnchor");
            if (centerEyeAnchor != null)
            {
                playerHead = centerEyeAnchor.transform;
            }
        }

        SetNormalVolume();
    }

    private void Update()
    {
        CheckHeadphonesPosition();
    }

    private void CheckHeadphonesPosition()
    {
        if (playerHead == null) return;

        float distanceA = Vector3.Distance(headphonesATransform.position, playerHead.position);
        bool previousAWornState = isHeadphonesAWorn;
        isHeadphonesAWorn = distanceA <= wearDistance && !grabbableA.isGrabbed;

        float distanceB = Vector3.Distance(headphonesBTransform.position, playerHead.position);
        bool previousBWornState = isHeadphonesBWorn;
        isHeadphonesBWorn = distanceB <= wearDistance && !grabbableB.isGrabbed;

        if (previousAWornState != isHeadphonesAWorn || previousBWornState != isHeadphonesBWorn)
        {
            UpdateAudioState();
        }
    }

    private void UpdateAudioState()
    {
        if (isHeadphonesBWorn)
        {
            audioMixer.SetFloat("MasterVolume", -80f);
        }
        else if (isHeadphonesAWorn)
        {
            audioMixer.SetFloat("MasterVolume", diminishedVolume);
        }
        else
        {
            SetNormalVolume();
        }
    }

    private void SetNormalVolume()
    {
        audioMixer.SetFloat("MasterVolume", normalVolume);
    }
}