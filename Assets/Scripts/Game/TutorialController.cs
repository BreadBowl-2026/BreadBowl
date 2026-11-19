using BreadBowl;
using BreadBowl.Dough;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{
    [SerializeField] private TMP_Text dialogueTextBox;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private Transform chalkboardViewPoint;
    [SerializeField] private Transform kneadViewPoint;
    [SerializeField] private Canvas kneadCanvas;
    [SerializeField] private KneadStage kneadStage;
    [SerializeField] private ShapeStage shapeStage;
    [SerializeField] private BreadBaker breadBaker;
    [SerializeField] private GameObject chef;
    [SerializeField] private Transform chefBakingPosition;
    [SerializeField] private Transform chefShapingPosition;
    [SerializeField] private Transform chefBowlingPosition;
    [SerializeField] private Button returnToKneadButton;
    [SerializeField] private Button bakeButton;
    [SerializeField] private Button bowlButton;
    [SerializeField] private int currentStageInd = 0;
    [SerializeField] private string currentStage = "Kneading";
    [SerializeField] private List<string> dialogue = new()
    {
        "Hello Chef! I am your Head Chef. Today is your first day at Upper Crust Academy! We have very high standards here, and we want to start you rolling down the right path. Today's lesson starts with some simple breadmaking. In front of you lies your dough. Bask in its beauty.",
        "Look to the chalkboard and you'll see what tools are available to you. The highlighted keys are what you'll be using for the kneading. Hold space bar, and gently roll you hands over the keys. To spin your dough, use the left and right shift keys. An important step is folding your dough, press any 3 number keys to fold.",
        "Now that you've gotten a feel for kneading, I'll let you in on a little trick on telling how kneaded your dough is. Select \"BakerVision\" at the top left, and you'll see your dough turn beautiful colours. Underknead your dough, and it'll stay red and won't rise. Overknead your dough, and it'll turn purple and rise unpredictably. Find your own sweet spot!",
        "Next, you can choose to shape your bread into a traditional boule shape, or go for the long and lean baguette!",
        "Using our SuperOven9000, bake your bread and watch it rise! Take it out too early and you'll end up with a sticky flat loaf. Take it out too late, and you'll end up with a smooth crusty dusty loaf. Better keep an eye on it!",
        "Your bread turned out terribly! No one will want to eat this disgusting lump of gluten. The only thing it would be good for is throwing it out!",
        "Use the arrows on the left to move your bread ball left and right. Use the arrows on the right to rotate your bread ball. Press Launch to send your bread flying!",
    };
    [SerializeField]
    private Dictionary<string, List<int>> dialogueSections = new Dictionary<string, List<int>>()
    {
        ["Kneading"] = new List<int> { 0, 1, 2 },
        ["Shaping"] = new List<int> { 3 },
        ["Baking"] = new List<int> { 4 },
        ["BakingResult"] = new List<int> { 5 },
        ["Bowling"] = new List<int> { 6 },
    };
    [SerializeField] private float duration = 1f;

    private Coroutine flightRoutine;

    private InputAction dialogueNextAction;
    private Transform cameraMain;
    
    private void Awake()
    {
        dialogueNextAction = InputSystem.actions.FindAction("UI/DialogueNext", true);
        bowlButton.onClick.AddListener(() => HandleBowling());

    }

    private void OnEnable()
    {
        dialogueNextAction.Enable();
        kneadStage.Finished += HandleKneadFinished;
        shapeStage.Finished += HandleShapeFinished;
        breadBaker.Finished += HandleBakerFinished;

    }

    private void OnDisable()
    {
        dialogueNextAction.Disable();
        kneadStage.Finished -= HandleKneadFinished;
        shapeStage.Finished -= HandleShapeFinished;
        breadBaker.Finished -= HandleBakerFinished;


    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueTextBox.text = dialogue[currentStageInd];
        cameraMain = Camera.main.transform;
    }

    // Update is called once per frame
    void Update()
    {
        // overrides interactable in LaneUI script
        bakeButton.interactable = false;
        if (dialogueNextAction.WasPressedThisFrame())
        {
            currentStageInd++;
            ChangeDialogueBox();
        }
    }

    public void FlyToChalkboard()
    {
        returnToKneadButton.gameObject.SetActive(true);
        FlyTo(chalkboardViewPoint);
        kneadCanvas.enabled = false;
    }

    public void FlyToKnead()
    {
        returnToKneadButton.gameObject.SetActive(false);
        FlyTo(kneadViewPoint);
        kneadCanvas.enabled = true;
    }
    private void FlyTo(Transform target)
    {
        if (flightRoutine != null) StopCoroutine(flightRoutine);
        flightRoutine = StartCoroutine(FlyRoutine(target.position, target.rotation));
    }
    private IEnumerator FlyRoutine(Vector3 endPos, Quaternion endRot)
    {
        Vector3 startPos = cameraMain.position;
        Quaternion startRot = cameraMain.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = Mathf.SmoothStep(0f, 1f, t);

            cameraMain.SetPositionAndRotation(
                Vector3.Lerp(startPos, endPos, eased),
                Quaternion.Slerp(startRot, endRot, eased)
            );

            yield return null;
        }

        cameraMain.SetPositionAndRotation(endPos, endRot);
        flightRoutine = null;
    }

    private void HandleKneadFinished(KneadResult result)
    {
        currentStage = "Shaping";
        currentStageInd = 0;
        ChangeDialogueBox();
        chef.transform.SetPositionAndRotation(chefShapingPosition.transform.position, chefShapingPosition.transform.rotation);
    }

    private void HandleShapeFinished(BreadShape shape)
    {
        currentStage = "Baking";
        currentStageInd = 0;
        ChangeDialogueBox();
        chef.transform.SetPositionAndRotation(chefBakingPosition.transform.position, chefBakingPosition.transform.rotation);
    }

    private void HandleBakerFinished()
    {
        currentStage = "BakingResult";
        currentStageInd = 0;
        ChangeDialogueBox();
    }

    private void HandleBowling()
    {
        currentStage = "Bowling";
        currentStageInd = 0;
        ChangeDialogueBox();
        chef.transform.SetPositionAndRotation(chefBowlingPosition.transform.position, chefBowlingPosition.transform.rotation);


    }

    private void ChangeDialogueBox()
    {
        List<int> dialogueOptions = dialogueSections[currentStage];
        if (currentStageInd >= dialogueOptions.Count)
        {
            dialogueBox.gameObject.SetActive(false);
        }
        else
        {
            dialogueBox.gameObject.SetActive(true);
            dialogueTextBox.text = dialogue[dialogueSections[currentStage][currentStageInd]];
        }

    }
}
