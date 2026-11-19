using System.Collections;
using UnityEngine;

namespace BreadBowl.Levels
{
    /// <summary>
    /// One camera move in a LevelIntro. The camera starts at From and moves
    /// to To over Duration, then LevelIntro cuts to the next shot.
    /// To place From or To: frame the view in the Scene view, select the
    /// object, then GameObject > Align With View (Ctrl+Shift+F).
    /// Swap From and To to reverse the direction of the move.
    /// </summary>
    public class IntroShot : MonoBehaviour
    {
        [Tooltip("Where the camera starts. Its rotation is the first view.")]
        [SerializeField] private Transform from;

        [Tooltip("Where the camera ends. Its rotation is the last view.")]
        [SerializeField] private Transform to;

        [Tooltip("Seconds the camera takes to move from From to To.")]
        [SerializeField][Min(0.1f)] private float duration = 3f;

        [Tooltip("Progress over the shot. A straight line is constant speed.")]
        [SerializeField] private AnimationCurve ease =
            AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Tooltip("Optional. The camera keeps facing this point instead of " +
            "turning from the From rotation to the To rotation.")]
        [SerializeField] private Transform lookAt;

        [SerializeField] private Color gizmoColor = new Color(1f, 0.6f, 0.2f);

        public bool IsSetUp => from != null && to != null;

        public IEnumerator Play(Transform cameraRig)
        {
            float elapsed = 0f;
            Apply(cameraRig, 0f);

            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;
                Apply(cameraRig, elapsed / duration);
            }
        }

        private void Apply(Transform cameraRig, float progress)
        {
            float amount = ease.Evaluate(Mathf.Clamp01(progress));
            Vector3 position = Vector3.LerpUnclamped(
                from.position, to.position, amount);
            Quaternion rotation = lookAt != null
                ? Quaternion.LookRotation(lookAt.position - position)
                : Quaternion.SlerpUnclamped(
                    from.rotation, to.rotation, amount);

            cameraRig.SetPositionAndRotation(position, rotation);
        }

        private void OnDrawGizmos()
        {
            if (!IsSetUp)
            {
                return;
            }

            Color faded = gizmoColor;
            faded.a = 0.5f;

            Gizmos.color = faded;
            DrawView(from);
            Gizmos.color = gizmoColor;
            DrawView(to);
            Gizmos.DrawLine(from.position, to.position);
            DrawArrowHead();

        #if UNITY_EDITOR
            UnityEditor.Handles.Label(from.position, name);
        #endif
        }

        private void DrawView(Transform view)
        {
            Camera main = Camera.main;
            float fieldOfView = main != null ? main.fieldOfView : 60f;
            float aspect = main != null ? main.aspect : 16f / 9f;

            Gizmos.matrix = Matrix4x4.TRS(
                view.position, view.rotation, Vector3.one);
            Gizmos.DrawFrustum(Vector3.zero, fieldOfView, 2f, 0.1f, aspect);
            Gizmos.matrix = Matrix4x4.identity;
        }

        private void DrawArrowHead()
        {
            Vector3 path = to.position - from.position;

            // From and To can share a spot for a shot that only turns.
            if (path.sqrMagnitude < 0.0001f)
            {
                return;
            }

            Quaternion facing = Quaternion.LookRotation(path);
            Vector3 left = facing * Quaternion.Euler(0f, 150f, 0f) * Vector3.forward;
            Vector3 right = facing * Quaternion.Euler(0f, -150f, 0f) *
                Vector3.forward;

            Gizmos.DrawLine(to.position, to.position + left);
            Gizmos.DrawLine(to.position, to.position + right);
        }
    }
}
