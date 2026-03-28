using TMPro;
using UnityEngine;

public class DisplayCurrency : MonoBehaviour
{
    public TMP_Text text;
    public Currency currency;

    void Awake()
    {
        currency.ValueChangedTo += OnValueUpdate;
    }

    public void OnValueUpdate(float newVal)
    {
        text.text = ConvertToString(newVal);
    }

    private string ConvertToString(float toConvert)
    {
        // TODO
        return toConvert.ToString();
    }
}