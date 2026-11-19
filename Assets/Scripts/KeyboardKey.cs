using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KeyboardKey : MonoBehaviour
{
    [SerializeField] private string legend = "";
    [SerializeField] private bool usable = false;
    [SerializeField] private TMP_Text text;
    [SerializeField] private SpriteRenderer border;
    [SerializeField] private Color lightUpColour = Color.yellow;
    private bool isFlashing = true;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float duration = 1f;
    private List<string> kneadingKeys = new List<string>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        legend = text.text;
        PopulateKeyboardKeysList();
        if (kneadingKeys.Contains(legend))
        {
            usable = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (usable && isFlashing)
        {
            float time = Time.time;

            float linear = Mathf.PingPong(time / duration, 1f);

            float t = Mathf.SmoothStep(0f, 1f, linear);

            Color c = Color.Lerp(Color.white, lightUpColour, t);
            c.a = 1;
            text.color = c;
            border.color = c;
        }

        //if (usable)
        //{
        //    text.color = lightUpColour;
        //    border.color = lightUpColour;
        //}

    }

    private void PopulateKeyboardKeysList()
    {
        kneadingKeys.Add("3");
        kneadingKeys.Add("4");
        kneadingKeys.Add("5");
        kneadingKeys.Add("6");
        kneadingKeys.Add("7");
        kneadingKeys.Add("8");
        kneadingKeys.Add("9");
        kneadingKeys.Add("0");
        kneadingKeys.Add("E");
        kneadingKeys.Add("R");
        kneadingKeys.Add("T");
        kneadingKeys.Add("Y");
        kneadingKeys.Add("U");
        kneadingKeys.Add("I");
        kneadingKeys.Add("O");
        kneadingKeys.Add("D");
        kneadingKeys.Add("F");
        kneadingKeys.Add("G");
        kneadingKeys.Add("H");
        kneadingKeys.Add("J");
        kneadingKeys.Add("K");
        kneadingKeys.Add("C");
        kneadingKeys.Add("V");
        kneadingKeys.Add("B");
        kneadingKeys.Add("N");
        kneadingKeys.Add("M");
        kneadingKeys.Add("SPACE");

    }
}
