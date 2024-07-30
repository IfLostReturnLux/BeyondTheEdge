using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private TextMeshProUGUI _dialogueCharacterName;
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private float _textSpeed;

    private Queue<string> _sentences;
    private PlayerInputController _input;

    public void Awake()
    {
        if (instance != null && instance != this) 
        {
            Destroy(this);
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
        _sentences = new Queue<string>();
        _input = new PlayerInputController();
        _input.Player.Interact.performed += context => DisplayNextSentence();
        _input.UI.Escape.performed += context => EndDialogue();
    }
    public void StartDialogue(string[] characterText, string name)
    {
        PlayerActions.instance.enabled = false;
        PlayerMove.instance.enabled = false;
        _input.Enable();
        
        _dialoguePanel.gameObject.SetActive(true);
        _dialogueCharacterName.text = name;

        _sentences.Clear();
        foreach (string sentence in characterText)
        {
            _sentences.Enqueue(sentence);
        }
        DisplayNextSentence();
    }
    private IEnumerator TypeText(string text)
    {
        _dialogueText.text = string.Empty;
        for (int i = 0; i < text.Length; i++) 
        {
            _dialogueText.text += text[i];
            yield return new WaitForSeconds(_textSpeed);
        }
    }

    public void DisplayNextSentence()
    {
        if(_sentences.Count == 0)
        {
            EndDialogue();
            return;
        }
        string sentence = _sentences.Dequeue();
        Debug.Log("Текущее предложение:" + sentence);

        StopAllCoroutines();
        StartCoroutine(TypeText(sentence));
    }
    public void EndDialogue()
    {
        _dialoguePanel.gameObject.SetActive(false);
        _input.Disable();
        PlayerMove.instance.enabled = true;
        PlayerActions.instance.enabled = true;
    }
}
