using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingManager : MonoBehaviour
{
    [SerializeField] AudioMixer _audio;
    [SerializeField] GameObject setttingsPanel, mainMenuPanel;
    [SerializeField] Slider musicSlided, sfxSlider, mousesensitivitySlider;
    void Start()
    {
        setttingsPanel.SetActive(false);
        musicSlided.value = PlayerPrefs.GetFloat("music", 0);
        sfxSlider.value = PlayerPrefs.GetFloat("sfx", 0);
        mousesensitivitySlider.value = PlayerPrefs.GetFloat("sensitivity", 1f);
        ApplyChanges();
    }

   

    public void ApplyChanges()
    {
        _audio.SetFloat("sfx", sfxSlider.value);
        _audio.SetFloat("music", musicSlided.value);
    }


    public void OpenCloseSettings()
    {
        if(!setttingsPanel.activeInHierarchy)
        {
            setttingsPanel.SetActive(true);
            mainMenuPanel.SetActive(false);
        }else 
        {
            setttingsPanel.SetActive(false);
            mainMenuPanel.SetActive(true);
            PlayerPrefs.SetFloat("music", musicSlided.value);
            PlayerPrefs.SetFloat("sfx", sfxSlider.value);
            PlayerPrefs.SetFloat("sensitivity", mousesensitivitySlider.value);
        }
    }


    public void ListenToPressedEvent(Component sender,object data)
    {
        if(data is bool )
        {
            OpenCloseSettings();
        }
    }
 
}
