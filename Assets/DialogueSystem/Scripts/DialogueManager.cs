using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [SerializeField] private DialogueUIReferences _ui;

    [SerializeField] private DialogueAudioReferences _audio;

    [Header("Typing Dialogue")]
    [Space(5)]
    [SerializeField] private float _typingSpeed = 0.02f;
    [SerializeField] private float _baseVolume = 1f;
    [Tooltip("Lower volume when inscreasing dialogue speed.")]
    [SerializeField] private float _fastTypeVolumeMultiplier = 0.7f;
    [Tooltip("Prevents too many overlapping voices when typing fast. Higher: Less voicing/ Lower: More voicing.")]
    [SerializeField] private float _blipCooldown = 0.001f;
    private float _blipTimer = 0f;
    private AudioClip _currentBlip;
    private const float HoldThreshold = 0.2f;
    private float _mouseHoldTime = 0f;


    private DialogueNode _currentNode;
    private int _currentLineIndex = 0;
    private Coroutine _typingCoroutine;
    private bool _isTyping = false;


    public void StartDialogue(DialogueNode startingNode)
    {
        _ui.DialoguePanel.SetActive(true);
        LoadNode(startingNode);
    }

    private void LoadNode(DialogueNode node)
    {
        ClearPreviousChoices();
        _currentNode = node;
        _currentLineIndex = 0;

        _ui.SpeakerNameText.text = node.speakerName;

        _currentBlip = node.voiceBlip != null ? node.voiceBlip : _audio.DefaultBlip;

        StartTyping(_currentNode.lines[_currentLineIndex]);
    }

    private void Update()
    {
        if (_currentNode == null) return;

        if (_isTyping)
        {
            if (Input.GetMouseButton(0))
            {
                _mouseHoldTime += Time.deltaTime;
            }
            else
            {
                _mouseHoldTime = 0f;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            TryAdvancingDialogue();
        }
    }

    private void TryAdvancingDialogue()
    {
        if (_isTyping || ChoiceToBeMade()) return;

        AdvanceToNextLine();
    }


    private void ShowChoices()
    {
        ClearPreviousChoices();

        if (_currentNode.choices != null && _currentNode.choices.Count > 0)
        {
            foreach (var choice in _currentNode.choices)
            {
                CreateChoiceButton(choice);
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
        _ui.DialogueLineText.text = "";
        _blipTimer = 0f;

        foreach (char letter in line)
        {
            _ui.DialogueLineText.text += letter;
            _blipTimer -= Time.deltaTime;

            PlayBlip(letter);

            float delay = (letter == ' ') ? 0f : (_mouseHoldTime >= HoldThreshold ? _typingSpeed * 0.25f : _typingSpeed);

            yield return new WaitForSeconds(delay);
        }

        _mouseHoldTime = 0f;
        _isTyping = false;
    }

    private void ClearPreviousChoices()
    {
        foreach (Transform child in _ui.ChoicesContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void EndDialogue()
    {
        _ui.DialoguePanel.SetActive(false);
        _currentNode = null;
        _currentLineIndex = 0;
    }

    private bool ChoiceToBeMade()
    {
        return _ui.ChoicesContainer.childCount > 0;
    }

    private void CreateChoiceButton(DialogueNode.DialogueChoice choice)
    {
        GameObject btnObj = Instantiate(_ui.ChoiceButtonPrefab, _ui.ChoicesContainer);
        var btnText = btnObj.GetComponentInChildren<TextMeshProUGUI>();
        btnText.text = choice.choice;

        Button btn = btnObj.GetComponent<Button>();
        DialogueNode next = choice.nextNode;

        btn.onClick.AddListener(() => HandleChoice(next));
    }

    private void HandleChoice(DialogueNode next)
    {
        if (next == null)
        {
            EndDialogue();
        }
        else
        {
            LoadNode(next);
        }
    }

    private void AdvanceToNextLine()
    {
        _currentLineIndex++;

        if (_currentLineIndex < _currentNode.lines.Count)
        {
            StartTyping(_currentNode.lines[_currentLineIndex]);
        }
        else
        {
            ShowChoices();
        }
    }

    private void StartTyping(string line)
    {
        if (_typingCoroutine != null)
            StopCoroutine(_typingCoroutine);

        _typingCoroutine = StartCoroutine(TypeLine(line));
    }

    private void PlayBlip(char letter)
    {
        if (letter == ' ' || !_audio.AudioSource || !_currentBlip) return;

        float blipVolume = (_mouseHoldTime >= HoldThreshold)
            ? _baseVolume * _fastTypeVolumeMultiplier
            : _baseVolume;

        _audio.AudioSource.pitch = Random.Range(0.95f, 1.05f);
        _audio.AudioSource.PlayOneShot(_currentBlip, blipVolume);
        _blipTimer = _blipCooldown;
    }

}