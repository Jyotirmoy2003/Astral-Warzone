using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderBar : MonoBehaviour
{
    public Slider slider;
    public Gradient gradient;
    [SerializeField] Image fill;

    public void SetMax(float maxValue)
    {
        slider.maxValue = maxValue;
    }

    public void SetValue(float Value)
    {
        slider.value = Value;
        fill.color = gradient.Evaluate(slider.normalizedValue);
        if (slider.value <= 0)
        {
            slider.value = 0;
        }
    }


    public void ListenToHealthChangeEvent(Component sender,object data)
    {
        if(data is float)
        {
            SetValue((float)data);
        }
    }
}
