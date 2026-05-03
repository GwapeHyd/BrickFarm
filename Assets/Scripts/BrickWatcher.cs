using UnityEngine;

public class BrickWatcher : MonoBehaviour
{
    [SerializeField] private MapGenerator mapGenerator;
    private bool isRegenerating = false;
    private Ball ball;

    private void Start()
    {
        ball = FindFirstObjectByType<Ball>();
    }

    private void Update()
    {
        if (!isRegenerating && mapGenerator.mapParent.childCount == 0)
        {
            isRegenerating = true;
            Invoke(nameof(DelayedGeneration), 0.5f); // délai de 0.5 secondes
        }
    }

    private void DelayedGeneration()
    {
        StartCoroutine(mapGenerator.GenerateMapWithDelay());
        isRegenerating = false;
    }

}
