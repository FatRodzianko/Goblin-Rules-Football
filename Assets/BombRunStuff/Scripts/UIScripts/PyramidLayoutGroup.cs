using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class PyramidLayoutGroup : MonoBehaviour
{
    [SerializeField] private float horizontalSpacing = 2f;
    [SerializeField] private float verticalSpacing = 2f;

    private bool isArranging;
    private bool layoutQueued;

    private void OnEnable()
    {
        QueueArrangeChildren();
    }

    private void OnValidate()
    {
        QueueArrangeChildren();
    }

    private void OnTransformChildrenChanged()
    {
        QueueArrangeChildren();
    }

    private void QueueArrangeChildren()
    {
        if (layoutQueued)
            return;

        layoutQueued = true;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            EditorApplication.delayCall += DelayedArrangeChildren;
            return;
        }
#endif

        // In Play Mode, arrange on the next frame.
        Invoke(nameof(DelayedArrangeChildren), 0f);
    }

    private void DelayedArrangeChildren()
    {
        layoutQueued = false;

        if (this == null || !isActiveAndEnabled)
            return;

        ArrangeChildren();
    }

    public void ArrangeChildren()
    {
        if (isArranging)
            return;

        isArranging = true;

        int childCount = transform.childCount;

        if (childCount == 0)
        {
            isArranging = false;
            return;
        }

        const int itemsPerRow = 2;

        // Calculate how many rows are needed.
        int totalRows = Mathf.CeilToInt(childCount / (float)itemsPerRow);

        int childIndex = 0;

        for (int row = 0; row < totalRows; row++)
        {
            int remainingChildren = childCount - childIndex;
            int childrenInRow = Mathf.Min(itemsPerRow, remainingChildren);

            // Center the row horizontally.
            float rowWidth = (childrenInRow - 1) * horizontalSpacing;
            float startingX = -rowWidth / 2f;

            // The bottom row is row 0.
            // Higher rows move upward.
            float centeredRow = row - (totalRows - 1) / 2f;
            float y = -centeredRow * verticalSpacing;

            for (int column = 0; column < childrenInRow; column++)
            {
                Transform child = transform.GetChild(childIndex);

                float x = startingX + column * horizontalSpacing;

                child.localPosition = new Vector3(x, y, 0f);

                childIndex++;
            }
        }

        isArranging = false;
    }

#if UNITY_EDITOR
    private void OnDisable()
    {
        EditorApplication.delayCall -= DelayedArrangeChildren;
    }
#endif
}