using UnityEngine;

[CreateAssetMenu(fileName = "CharacterProfile", menuName = "Simon/CharacterProfile")]
public class CharacterProfile : ScriptableObject
{
    public float moveSpeed = 5;
    public float gravity = 14;
    public int maxAllowedJumps = 3;
    public float maxCoyoteTime = 0.3f;
    public AnimationCurve gravityMultiplierCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}
