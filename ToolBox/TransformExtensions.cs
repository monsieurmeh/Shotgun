using UnityEngine;

namespace Shotgun.ToolBox
{
    internal static class TransformExtensions
    {
        internal static void MirrorLayout(this Transform target, Transform source)
        {
            for (int i = 0, iMax = Mathf.Min(source.childCount, target.childCount); i < iMax; i++)
            {
                Transform sourceChild = source.GetChild(i);
                Transform targetChild = target.GetChild(i);
                if (sourceChild.gameObject.activeSelf != targetChild.gameObject.activeSelf)
                    targetChild.gameObject.SetActive(sourceChild.gameObject.activeSelf);
                targetChild.localPosition = sourceChild.localPosition;
                targetChild.MirrorLayout(sourceChild);
            }
        }
    }
}
