using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;

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
