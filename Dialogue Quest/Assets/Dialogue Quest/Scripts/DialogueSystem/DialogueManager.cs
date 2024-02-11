using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [SerializeField] private Image character1Icon;
    [SerializeField] private Image character2Icon;
    [SerializeField] private TextMeshProUGUI character1Name;
    [SerializeField] private TextMeshProUGUI character2Name;
    [SerializeField] private Animator character1Animator;
    [SerializeField] private Animator character2Animator;
    [SerializeField] private TextMeshProUGUI dialogueArea;
    [SerializeField] private BackGroundManager backGroundManager;

    private Queue<DialogueLine> lines = new Queue<DialogueLine>();

    public bool isDialogueActive = false;

    public float typingSpeed = 0.05f;
    public float delayBeforeTyping = 0f;

    private bool isTyping = false;
    private DialogueLine currentLine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartDialogue(Dialogue dialogue)
    {
        isDialogueActive = true;
        lines.Clear();
        foreach (DialogueLine dialogueLine in dialogue.dialogueLines)
        {
            lines.Enqueue(dialogueLine);
        }

        currentLine = lines.Dequeue();

        SetValuesForCharacters();

        StartCoroutine(TypeSentence(currentLine));
    }

    public void DisplayNextDialogueLine()
    {
        if (isTyping)
        {
            StopAllCoroutines();
            dialogueArea.text = currentLine.line;
            isTyping = false;
        }
        else
        {
            if (lines.Count == 0)
            {
                EndDialogue();
                return;
            }

            currentLine = lines.Dequeue();

            SetValuesForCharacters();

            StopAllCoroutines();
            StartCoroutine(WaitAndTypeSentence(currentLine, delayBeforeTyping));
        }
    }

    private IEnumerator WaitAndTypeSentence(DialogueLine dialogueLine, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        StartCoroutine(TypeSentence(dialogueLine));
    }

    private IEnumerator TypeSentence(DialogueLine dialogueLine)
    {
        isTyping = true;
        StringBuilder sb = new StringBuilder();

        foreach (char letter in dialogueLine.line.ToCharArray())
        {
            sb.Append(letter);
            dialogueArea.text = sb.ToString();
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void SetValuesForCharacters()
    {
        SetCharacterValues(currentLine.character1, character1Icon, character1Name, ref character1Animator);
        SetCharacterValues(currentLine.character2, character2Icon, character2Name, ref character2Animator);
        SetWhoIsSpeaking();
        delayBeforeTyping = TryToChangeBackGround();
    }

    private void SetCharacterValues(Character character, Image characterIcon, TextMeshProUGUI characterName, ref Animator characterAnimator)
    {
        CanvasGroup canvasGroup = characterIcon.GetComponent<CanvasGroup>();
        if (character != null)
        {
            canvasGroup.alpha = 1f;
            characterIcon.sprite = character.characterIcon;
            characterName.text = character.characterName;
            characterAnimator = characterIcon.GetComponent<Animator>();
        }
        else
        {
            if (characterAnimator != null)
            {
                characterAnimator.SetBool("isSpeaking", false);
                canvasGroup.alpha = 0f;
            }
        }
    }

    private void SetWhoIsSpeaking()
    {
        switch (currentLine.whoSpeak)
        {
            case 0:
                SetSpeakingState(false, false);
                break;
            case 1:
                SetSpeakingState(true, false);
                break;
            case 2:
                SetSpeakingState(false, true);
                break;
        }
    }

    private void SetSpeakingState(bool isCharacter1Speaking, bool isCharacter2Speaking)
    {
        character1Animator.SetBool("isSpeaking", isCharacter1Speaking);
        character2Animator.SetBool("isSpeaking", isCharacter2Speaking);
    }

    private void EndDialogue()
    {
        isDialogueActive = false;

        character1Icon.gameObject.SetActive(false);
        character2Icon.gameObject.SetActive(false);
        dialogueArea.text = "The End";
    }

    public float TryToChangeBackGround()
    {
        if (currentLine.BackGround != null && currentLine.BackGround != backGroundManager.BackGround.sprite)
        {
            backGroundManager.SetNewBackGround(currentLine.BackGround);
            return backGroundManager.GetAnimationTime();
        }
        return 0f;
    }

}