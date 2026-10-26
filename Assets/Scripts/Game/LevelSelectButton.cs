using UnityEngine;
using UnityEngine.UI;

namespace BreadBowl
{
    [RequireComponent(typeof(Button))]
    public class LevelSelectButton : MonoBehaviour
    {
        [SerializeField] private string sceneName;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Load);
        }

        private void Load()
        {
            GameController.Instance.LoadLevel(sceneName);
        }
    }
}
