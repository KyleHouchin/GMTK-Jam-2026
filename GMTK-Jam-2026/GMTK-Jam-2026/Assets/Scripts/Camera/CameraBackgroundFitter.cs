using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class CameraBackgroundController : MonoBehaviour
{
    [Header("Background")]
    [SerializeField] private Sprite backgroundSprite;

    [Header("Fitting")]
    [SerializeField] private bool fillEntireScreen = true;
    [SerializeField] private float distanceFromCamera = 10f;

    [Header("Rendering")]
    [SerializeField] private string sortingLayerName = "Background";
    [SerializeField] private int orderInLayer = -100;

    private const string BackgroundObjectName =
        "LevelBackground";

    private Camera targetCamera;
    private SpriteRenderer backgroundRenderer;

    private float previousOrthographicSize = -1f;
    private float previousAspect = -1f;
    private Sprite previousBackgroundSprite;

    private void Awake()
    {
        SetUpBackground();
    }

    private void OnEnable()
    {
        SetUpBackground();
    }

    private void Start()
    {
        SetUpBackground();
    }

    private void Update()
    {
        if (targetCamera == null ||
            backgroundRenderer == null)
        {
            SetUpBackground();
        }

        if (targetCamera == null ||
            backgroundRenderer == null)
        {
            return;
        }

        KeepBackgroundCentered();

        bool cameraSizeChanged =
            !Mathf.Approximately(
                previousOrthographicSize,
                targetCamera.orthographicSize
            );

        bool cameraAspectChanged =
            !Mathf.Approximately(
                previousAspect,
                targetCamera.aspect
            );

        bool backgroundSpriteChanged =
            previousBackgroundSprite !=
            backgroundSprite;

        if (cameraSizeChanged ||
            cameraAspectChanged ||
            backgroundSpriteChanged)
        {
            ApplyBackgroundSettings();
            FitBackgroundToCamera();
        }
    }

    private void OnValidate()
    {
        distanceFromCamera =
            Mathf.Max(0.01f, distanceFromCamera);

        SetUpBackground();
    }

    private void SetUpBackground()
    {
        targetCamera = GetComponent<Camera>();

        if (targetCamera == null)
        {
            return;
        }

        FindOrCreateBackgroundRenderer();
        ApplyBackgroundSettings();
        FitBackgroundToCamera();
    }

    private void FindOrCreateBackgroundRenderer()
    {
        Transform existingBackground =
            transform.Find(BackgroundObjectName);

        if (existingBackground == null)
        {
            GameObject backgroundObject =
                new GameObject(BackgroundObjectName);

            existingBackground =
                backgroundObject.transform;

            existingBackground.SetParent(
                transform,
                false
            );
        }

        backgroundRenderer =
            existingBackground
                .GetComponent<SpriteRenderer>();

        if (backgroundRenderer == null)
        {
            backgroundRenderer =
                existingBackground.gameObject
                    .AddComponent<SpriteRenderer>();
        }
    }

    private void ApplyBackgroundSettings()
    {
        if (backgroundRenderer == null)
        {
            return;
        }

        backgroundRenderer.sprite =
            backgroundSprite;

        backgroundRenderer.sortingLayerName =
            sortingLayerName;

        backgroundRenderer.sortingOrder =
            orderInLayer;

        backgroundRenderer.enabled =
            backgroundSprite != null;

        backgroundRenderer.color =
            Color.white;

        backgroundRenderer.drawMode =
            SpriteDrawMode.Simple;

        previousBackgroundSprite =
            backgroundSprite;
    }

    private void FitBackgroundToCamera()
    {
        if (targetCamera == null ||
            backgroundRenderer == null ||
            backgroundRenderer.sprite == null ||
            !targetCamera.orthographic)
        {
            return;
        }

        float cameraHeight =
            targetCamera.orthographicSize * 2f;

        float cameraWidth =
            cameraHeight * targetCamera.aspect;

        Vector2 spriteSize =
            backgroundRenderer.sprite.bounds.size;

        if (spriteSize.x <= 0f ||
            spriteSize.y <= 0f)
        {
            return;
        }

        float widthScale =
            cameraWidth / spriteSize.x;

        float heightScale =
            cameraHeight / spriteSize.y;

        float requiredScale;

        if (fillEntireScreen)
        {
            requiredScale =
                Mathf.Max(
                    widthScale,
                    heightScale
                );
        }
        else
        {
            requiredScale =
                Mathf.Min(
                    widthScale,
                    heightScale
                );
        }

        backgroundRenderer.transform.localScale =
            new Vector3(
                requiredScale,
                requiredScale,
                1f
            );

        previousOrthographicSize =
            targetCamera.orthographicSize;

        previousAspect =
            targetCamera.aspect;

        KeepBackgroundCentered();
    }

    private void KeepBackgroundCentered()
    {
        if (backgroundRenderer == null)
        {
            return;
        }

        backgroundRenderer.transform.localPosition =
            new Vector3(
                0f,
                0f,
                distanceFromCamera
            );

        backgroundRenderer.transform.localRotation =
            Quaternion.identity;
    }
}