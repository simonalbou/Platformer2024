using UnityEngine;
using UnityEngine.UI;

public class CustomButton : MonoBehaviour
{
    public Image img;
    public Color hoveredColor = Color.yellow;
    public Color neutralColor = Color.white;
    public Color focusedColor = new Color(1, 0.5f, 0.2f, 1f);

    bool isHovered, isPressed;

    void RefreshColor()
    {
        if (isPressed) img.color = focusedColor;
        else if (isHovered) img.color = hoveredColor;
        else img.color = neutralColor;
    }

    public void SetColorAsHovered()
    {
        isHovered = true;
        RefreshColor();
    }

    public void SetColorAsUnhovered()
    {
        isHovered = false;
        RefreshColor();
    }

    public void SetColorAsPressed()
    {
        isPressed = true;
        RefreshColor();
    }

    public void SetColorAsReleased()
    {
        isPressed = false;
        RefreshColor();
    }
}
