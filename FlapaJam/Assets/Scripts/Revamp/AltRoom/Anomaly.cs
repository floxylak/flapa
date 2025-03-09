using UnityEngine;

public class Anomaly : MonoBehaviour
{
    public enum AnomalyType
    {
        ObjectMoved, ObjectDisappear, ObjectMoving, Noise, Intruder,
        Static, StrangeAudio, ObjectChange, FlickeringLights, LargeObject,
        Foliage, Friend, TickingBomb
    }

    private AnomalyType type;

    public void Initialize()
    {
        type = (AnomalyType)Random.Range(0, System.Enum.GetValues(typeof(AnomalyType)).Length);
        Debug.Log($"Anomaly in {gameObject.name}: {type}");
        
        // Implement anomaly effects here (e.g., spawn object, flicker lights)
        switch (type)
        {
            case AnomalyType.FlickeringLights:
                // Add flickering light component or script
                break;
            case AnomalyType.TickingBomb:
                // Add bomb with timer
                break;
            // Add more cases as needed
        }
    }
}
