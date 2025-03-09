using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HpPanel : MonoBehaviour
{
    [SerializeField] Slider hpSlider;

    public void SetValue(float value)
    {
        hpSlider.value = value;
    }
}
