using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DamageTextUpdate : NetworkBehaviour {

    [SyncVar(hook = "OnTextChange")]
    public string textDamage = "-10";
    private Text textComponent;

	void Start ()
    {
        textComponent = GetComponentInChildren<Text>();
        if (textComponent != null)
        {
            textComponent.text = textDamage;
        }
	}

    private void OnTextChange(string txt)
    {
        if (textComponent != null)
        {
            textComponent.text = txt;
        }
    }
}
