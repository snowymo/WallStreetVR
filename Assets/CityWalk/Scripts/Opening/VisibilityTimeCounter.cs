using UnityEngine;

public class VisibilityTimeCounter : MonoBehaviour
{
    public float VisibleTime { get; private set; }

    private bool isVisible = false;
    public bool resetWhenInvisible = true;

    void Start()
    {

    }

    public void OnEnable()
    {
        VisibleTime = 0;
    }

    void Update()
    {
        if (InputManager.Instance.HasAnyKeyPressed && isVisible)
        {
            VisibleTime += Time.deltaTime;
        }
    }

    void OnBecameVisible()
    {
        isVisible = true;
    }

    void OnBecameInvisible()
    {
        isVisible = false;

        if (resetWhenInvisible)
        {
            VisibleTime = 0;
        }
    }
}
