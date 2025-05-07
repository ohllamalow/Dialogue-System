using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject _dialoguePanel;
    [SerializeField] private TextMeshProUGUI _speakerNameText;
    [SerializeField] private TextMeshProUGUI _dialogueLineText;
    [SerializeField] private Transform _choicesContainer;
    [SerializeField] private GameObject _choiceButtonPrefab;

    [Header("Typing Speed")]
    [Space(10)]
    public float TypingSpeed = 0.02f;
    private float _currentTypingSpeed;

    [Header("Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _defaultBlip;
    private AudioClip _currentBlip;


    private DialogueNode _currentNode;
    private int _currentLineIndex = 0;
    private Coroutine _typingCoroutine;
    private bool _isTyping = false;


    public void StartDialogue(DialogueNode startingNode)
    {
        _dialoguePanel.SetActive(true);
        LoadNode(startingNode);
    }

    private void LoadNode(DialogueNode node)
    {
        ClearPreviousChoices();
        _currentNode = node;
        _currentLineIndex = 0;

        _speakerNameText.text = node.speakerName;

        _currentBlip = node.voiceBlip != null ? node.voiceBlip : _defaultBlip;

        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        _typingCoroutine = StartCoroutine(TypeLine(_currentNode.lines[_currentLineIndex]));
    }

    private void Update()
    {
        if (_currentNode == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            TryAdvancingDialogue();
        }
    }

    private void TryAdvancingDialogue()
    {
        if (_isTyping || ChoiceToBeMade())
            return;

        _currentLineIndex++;

        if (_currentLineIndex < _currentNode.lines.Count)
        {
            if (_typingCoroutine != null)
                StopCoroutine(_typingCoroutine);

            _typingCoroutine = StartCoroutine(TypeLine(_currentNode.lines[_currentLineIndex]));
        }
        else
        {
            ShowChoices();
        }
    }


    private void ShowChoices()
    {
        ClearPreviousChoices();

        if (_currentNode.choices != null && _currentNode.choices.Count > 0)
        {
            foreach (var choice in _currentNode.choices)
            {
                GameObject btnObj = Instantiate(_choiceButtonPrefab, _choicesContainer);
                var btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
                btnText.text = choice.choice;

                Button btn = btnObj.GetComponent<Button>();
                DialogueNode next = choice.nextNode;
                btn.onClick.AddListener(() =>
                {
                    if (next == null)
                    {
                        EndDialogue();
                    }
                    else
                    {
                        LoadNode(next);
                    }
                });
            }
        }
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeLine(string line)
    {
        _isTyping = true;
        _dialogueLineText.text = "";

        foreach (char letter in line)
        {
            _dialogueLineText.text += letter;
            if (letter != ' ' && _audioSource && _currentBlip)
            {
                _audioSource.pitch = Random.Range(0.95f, 1.05f);
                _audioSource.PlayOneShot(_currentBlip);
            }


            float delay = (letter == ' ') ? 0f : (Input.GetMouseButton(0) ? TypingSpeed * 0.25f : TypingSpeed);
            yield return new WaitForSeconds(delay);
        }

        _isTyping = false;
    }

    private void ClearPreviousChoices()
    {
        foreach (Transform child in _choicesContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void EndDialogue()
    {
        _dialoguePanel.SetActive(false);
        _currentNode = null;
        _currentLineIndex = 0;
    }

    private bool ChoiceToBeMade()
    {
        return _choicesContainer.childCount > 0;
    }
}
