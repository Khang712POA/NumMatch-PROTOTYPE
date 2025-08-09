using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISetting : MonoBehaviour
{
    [Header("Setting Element")]
    public Toggle SoundMuteToggle;

    private void OnEnable()
    {
        // audio
        SoundMuteToggle?.onValueChanged.AddListener((bool mute) => AudioManager.Instance.SetSFXMute(mute));

    }
}
