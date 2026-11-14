using UnityEngine;

namespace BreadBowl.Levels
{
    [CreateAssetMenu(fileName = "LevelInfo", menuName = "BreadBowl/Level Info")]
    public class LevelInfo : ScriptableObject
    {
        [SerializeField] private string displayName = "New Level";
        [SerializeField] private string sceneName;
        [SerializeField][TextArea(3, 6)] private string description;
        [SerializeField] private Sprite screenshot;

        public string DisplayName => displayName;
        public string SceneName => sceneName;
        public string Description => description;
        public Sprite Screenshot => screenshot;
        public bool IsPlayable => 
            !string.IsNullOrEmpty(sceneName) &&
            Application.CanStreamedLevelBeLoaded(sceneName);
    }
}

