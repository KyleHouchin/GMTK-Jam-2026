using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(SpriteRenderer))]
public class CameraBackgroundFitter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;

    [Header("Settings")]
    [SerializeField] private bool fillEntireScreen = true;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        FitBackground();
    }

    private void Start()
    {
        FitBackground();
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            FitBackground();
        }
    }
#endif

    public void FitBackground()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null ||
            spriteRenderer.sprite == null ||
            !targetCamera.orthographic)
        {
            return;
        }

        float cameraHeight =
            targetCamera.orthographicSize * 2f;

        float cameraWidth =
            cameraHeight * targetCamera.aspect;

        Vector2 spriteSize =
            spriteRenderer.sprite.bounds.size;

        float widthScale =
            cameraWidth / spriteSize.x;

        float heightScale =
            cameraHeight / spriteSize.y;

        float requiredScale;

        if (fillEntireScreen)
        {
            requiredScale =
                Mathf.Max(widthScale, heightScale);
        }
        else
        {
            requiredScale =
                Mathf.Min(widthScale, heightScale);
        }

        transform.localScale =
            new Vector3(
                requiredScale,
                requiredScale,
                1f
            );
    }
}