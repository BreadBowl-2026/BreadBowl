using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

public class StageController : MonoBehaviour
{
    [SerializeField] private GameObject breadMesh;
    [SerializeField] private string laneScene;
    [SerializeField] private int waitingTime = 2;
    [SerializeField] private float breadScale = 1f;

    public async void MoveBreadToLane()
    {
        Debug.Log("Waiting before loading scene: " + laneScene);
        await Task.Delay(waitingTime * 1000);

        
        // this prevents the warning about multiple event systems
        // loadsceneasync keeps the previous scene running in the background
        // deactivating it means there is no conflict
        if (EventSystem.current != null)
        {
            EventSystem.current.gameObject.SetActive(false);
        }

        // same idea as above, the camera from the first scene has an audio 
        // listener, disabling it before the new scene means only 1 ever
        // exists at a time.
        AudioListener listener = FindFirstObjectByType<AudioListener>();
        if (listener != null)
        {
            listener.enabled = false;
        }

        Debug.Log("Opening scene: " + laneScene);   
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(laneScene, LoadSceneMode.Additive);

        while (!loadOp.isDone)
        {
            await Task.Yield();
        }

        GameObject bread = GameObject.Find("Breadshape");
        if (bread == null)
        {
            Debug.LogError("Could not find GameObject named 'Breadshape' in the loaded scene!");
            return;
        }

        if (breadMesh != null)
        {
            MeshFilter myBreadMeshFilter = breadMesh.GetComponent<MeshFilter>();
            if (myBreadMeshFilter != null && myBreadMeshFilter.sharedMesh != null)
            {
                bread.GetComponent<MeshFilter>().mesh = myBreadMeshFilter.sharedMesh;

                bread.GetComponent<MeshCollider>().sharedMesh = myBreadMeshFilter.sharedMesh;

                bread.GetComponent<MeshRenderer>().material = breadMesh.GetComponent<MeshRenderer>().material;
                bread.transform.localScale = breadMesh.transform.lossyScale * breadScale;
            }
            else
            {
                Debug.LogError("Source 'breadMesh' doesn't have a valid MeshFilter or Mesh assigned!");
            }
        }

        Debug.Log("Unloading scene...");
        await SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
    }

}
