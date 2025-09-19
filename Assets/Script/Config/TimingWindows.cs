using UnityEngine;

[CreateAssetMenu(fileName = "TimingWindows", menuName = "HellsMelody/Timing Windows")]
public class TimingWindows : ScriptableObject
{
    [Header("Timing windows (s)")]
    public float perfect = 0.05f;
    public float great   = 0.10f;
    public float good    = 0.15f;
    public float miss    = 0.15f;
}
