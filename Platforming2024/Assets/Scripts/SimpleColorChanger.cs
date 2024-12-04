using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimpleColorChanger : MonoBehaviour
{
    public Image img;
    public TextMeshProUGUI text;

    public void RecolorImage()
    {
        img.color = Color.yellow;
    }

    public void EnableImage()
    {
        img.enabled = true;
    }

    public void HideImage()
    {
        img.enabled = false;
    }

    public void ToggleRaycast()
    {
        img.raycastTarget = !img.raycastTarget;
    }

    public void ChangeText()
    {
        text.color = Color.blue;
        text.text = "Hello world!"; 
        
        //text.SetText("Hello world!"); // ne fonctionne pas !
    }
}
