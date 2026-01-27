using UnityEngine;

namespace Shotgun.Util
{
    internal static class Primitives
    {
        /// <summary>
        /// Create a collider-less, colored, radius-configurable cylinder from start
        /// pointing toward end. If length is 0, it connects start and end.
        /// The cylinder is parented to start.
        /// </summary>
        internal static void ShootRay(
            Transform start,
            Transform end,
            float radius,
            Color color,
            float length = 0f)
        {
            Vector3 localDir =
                start.InverseTransformDirection(end.position - start.position).normalized;

            float finalLength = (length <= 0f)
                ? Vector3.Distance(start.position, end.position)
                : length;

            GameObject ray = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Component.Destroy(ray.GetComponent<CapsuleCollider>());

            ray.transform.SetParent(start, false);

            ray.transform.localRotation =
                Quaternion.FromToRotation(Vector3.up, localDir);

            ray.transform.localPosition =
                localDir * (finalLength * 0.5f);

            ray.transform.localScale = new Vector3(
                radius * 2f,
                finalLength * 0.5f,
                radius * 2f
            );

            var r = ray.GetComponent<Renderer>();
            r.material = new Material(Shader.Find("Standard"));
            r.material.color = color;
        }

    }
}
