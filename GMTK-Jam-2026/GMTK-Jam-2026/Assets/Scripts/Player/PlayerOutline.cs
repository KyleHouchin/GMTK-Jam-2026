using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerOutline : MonoBehaviour
{
    [Header("Materials")]
    [SerializeField] private Material outlineMaterial;
    [SerializeField] private Material glowMaterial;

    [Header("Outline")]
    [SerializeField, Min(1f)] private float outlineScale = 1.06f;
    [SerializeField] private int outlineOrderOffset = -1;

    [Header("Glow")]
    [SerializeField, Min(1f)] private float glowScale = 1.12f;
    [SerializeField] private int glowOrderOffset = -2;

    private const string OutlineObjectName = "PlayerOutline";
    private const string GlowObjectName = "PlayerGlow";

    private SpriteRenderer playerRenderer;
    private SpriteRenderer outlineRenderer;
    private SpriteRenderer glowRenderer;

    private void Awake()
    {
        playerRenderer = GetComponent<SpriteRenderer>();

        CreateEffectRenderers();
        UpdateEffectRenderers();
    }

    private void LateUpdate()
    {
        UpdateEffectRenderers();
    }

    private void OnValidate()
    {
        outlineScale = Mathf.Max(1f, outlineScale);
        glowScale = Mathf.Max(outlineScale, glowScale);

        if (!Application.isPlaying)
        {
            playerRenderer = GetComponent<SpriteRenderer>();

            CreateEffectRenderers();
            UpdateEffectRenderers();
        }
    }

    private void CreateEffectRenderers()
    {
        if (playerRenderer == null)
        {
            playerRenderer = GetComponent<SpriteRenderer>();
        }

        outlineRenderer = FindOrCreateRenderer(
            OutlineObjectName,
            outlineMaterial
        );

        glowRenderer = FindOrCreateRenderer(
            GlowObjectName,
            glowMaterial
        );
    }

    private SpriteRenderer FindOrCreateRenderer(
        string objectName,
        Material effectMaterial)
    {
        Transform effectTransform =
            transform.Find(objectName);

        if (effectTransform == null)
        {
            GameObject effectObject =
                new GameObject(objectName);

            effectTransform =
                effectObject.transform;

            effectTransform.SetParent(
                transform,
                false
            );
        }

        SpriteRenderer effectRenderer =
            effectTransform.GetComponent<SpriteRenderer>();

        if (effectRenderer == null)
        {
            effectRenderer =
                effectTransform.gameObject
                    .AddComponent<SpriteRenderer>();
        }

        effectRenderer.sharedMaterial =
            effectMaterial;

        return effectRenderer;
    }

    private void UpdateEffectRenderers()
    {
        if (playerRenderer == null ||
            outlineRenderer == null ||
            glowRenderer == null)
        {
            return;
        }

        CopyRendererSettings(
            outlineRenderer,
            outlineScale,
            outlineOrderOffset,
            outlineMaterial
        );

        CopyRendererSettings(
            glowRenderer,
            glowScale,
            glowOrderOffset,
            glowMaterial
        );
    }

    private void CopyRendererSettings(
        SpriteRenderer effectRenderer,
        float effectScale,
        int orderOffset,
        Material effectMaterial)
    {
        effectRenderer.sprite =
            playerRenderer.sprite;

        effectRenderer.flipX =
            playerRenderer.flipX;

        effectRenderer.flipY =
            playerRenderer.flipY;

        effectRenderer.sortingLayerID =
            playerRenderer.sortingLayerID;

        effectRenderer.sortingOrder =
            playerRenderer.sortingOrder +
            orderOffset;

        effectRenderer.maskInteraction =
            playerRenderer.maskInteraction;

        effectRenderer.spriteSortPoint =
            playerRenderer.spriteSortPoint;

        effectRenderer.sharedMaterial =
            effectMaterial;

        effectRenderer.enabled =
            playerRenderer.enabled &&
            playerRenderer.sprite != null &&
            effectMaterial != null;

        effectRenderer.transform.localPosition =
            Vector3.zero;

        effectRenderer.transform.localRotation =
            Quaternion.identity;

        effectRenderer.transform.localScale =
            new Vector3(
                effectScale,
                effectScale,
                1f
            );
    }
}