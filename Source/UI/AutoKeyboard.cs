using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.MixedReality.Toolkit.Experimental.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace CWVR.UI;

public class AutoKeyboard : MonoBehaviour
{
    private readonly Dictionary<TMP_InputField, UnityAction<string>> inputFields = [];

    private Coroutine populateCoroutine;
    public NonNativeKeyboard keyboard;

    private void OnEnable()
    {
        populateCoroutine = StartCoroutine(PopulateInputsRoutine());
    }
    
    private void OnDisable()
    {
        StopCoroutine(populateCoroutine);
    }

    private IEnumerator PopulateInputsRoutine()
    {
        while (true)
        {
            RemoveDestroyed();
            PopulateInputs();

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void RemoveDestroyed()
    {
        foreach (var item in inputFields.Where(kvp => kvp.Key == null).ToList())
        {
            inputFields.Remove(item.Key);
        }
    }

    private void PopulateInputs()
    {
        var inputs = FindObjectsOfType<TMP_InputField>(true);

        foreach (var input in inputs)
        {
            if (inputFields.ContainsKey(input))
                continue;

            var action = new UnityAction<string>((_) =>
            {
                keyboard.InputField = input;
                keyboard.PresentKeyboard();
            });
            
            inputFields.Add(input, action);
            input.onSelect.AddListener(action);
        }
    }

    private void OnDestroy()
    {
        foreach (var kv in inputFields)
            kv.Key.onSelect.RemoveListener(kv.Value);
    }
}