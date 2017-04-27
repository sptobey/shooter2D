using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFade : MonoBehaviour {

    public float fadeLifeTime = 2.0f;

    private Text text;
    private Color color;
    private float alpha;
    private float fadePerSec;

    void Start ()
    {
        text = GetComponent<Text>();
        color = text.color;
        alpha = 1.0f;
        fadePerSec = (1 / fadeLifeTime);
    }
	
	void Update ()
    {
        alpha -= fadePerSec * Time.deltaTime;
        color.a = alpha;
        text.color = color;
	}
}
