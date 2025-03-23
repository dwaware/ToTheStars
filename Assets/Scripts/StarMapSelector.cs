using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StarMapSelector : MonoBehaviour, IPointerClickHandler
{
    [Header("Render Setup")]
    [SerializeField] private Camera starMapCamera;
    [SerializeField] private RawImage starMapRawImage;

    [Header("Raycast Settings")]
    public float debugRayDuration = 30f; // Duration to show debug ray

    private int starMapLayerMask;

    void Start()
    {
        if (!starMapCamera) Debug.LogError("[StarMapSelector] No StarMapCamera assigned!");
        if (!starMapRawImage) Debug.LogError("[StarMapSelector] No RawImage assigned!");

        starMapLayerMask = starMapCamera.cullingMask;
        Debug.Log("[StarMapSelector] Initialized. LayerMask: " + starMapLayerMask);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"[StarMapSelector] Click at screen pos: {eventData.position}");

        RectTransform rt = starMapRawImage.rectTransform;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            Debug.LogWarning("[StarMapSelector] Click was outside RawImage bounds.");
            return;
        }

        Vector2 pivot = rt.pivot;
        Vector2 uv = new Vector2(
            (localPoint.x / rt.rect.width) + pivot.x,
            (localPoint.y / rt.rect.height) + pivot.y
        );

        // Remove inversion (flipping) if it causes incorrect behavior
        // uv.y = 1f - uv.y; // No longer necessary

        Vector2 safeUV = new Vector2(
            Mathf.Clamp01(uv.x),
            Mathf.Clamp01(uv.y)
        );

        Debug.Log($"[StarMapSelector] Normalized UV (clamped & flipped): {safeUV}");

        Ray ray = starMapCamera.ViewportPointToRay(safeUV);
        Debug.DrawRay(ray.origin, ray.direction * 2000f, Color.magenta, debugRayDuration);
        Debug.Log($"[StarMapSelector] Ray origin: {ray.origin}, dir: {ray.direction}");

        if (Physics.Raycast(ray, out RaycastHit hit, 10000f, starMapLayerMask))
        {
            string starName = hit.collider.gameObject.name;
            Vector3 pos = hit.point;

            Debug.Log($"[StarMapSelector] 🎯 HIT: {starName} at {pos}");
        }
        else
        {
            Debug.Log("[StarMapSelector] ❌ No hit on StarMap layer.");
        }
    }
}
