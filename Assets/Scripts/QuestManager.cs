using UnityEngine;
using TMPro;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance; 
    [SerializeField] private TextMeshProUGUI questText;
    public TextMeshProUGUI QuestText => questText;
    public enum QuestAction { None, TakeAChance, TakeARest, TakeARisk, TakeSomeTimeOff }
    public QuestAction CurrentQuest { get; private set; } = QuestAction.None;   

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); 
            return;              
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterQuestText(TextMeshProUGUI tmp)
    {
        questText = tmp;
    }

    public void SetQuest(string text)
    {
        if (questText == null) return; 

        questText.text = text;

        Color c = questText.color;
        c.a = 1f;
        questText.color = c;

        questText.gameObject.SetActive(true);
    }

    public void ClearQuest()
    {
        if (questText == null) return;

        questText.text = "";
        questText.gameObject.SetActive(false);
        CurrentQuest = QuestAction.None;
    }

    public void ShowQuest() => questText.gameObject.SetActive(true);
    public void HideQuest() => questText.gameObject.SetActive(false);

    public void SetQuest_TakeAChance() { CurrentQuest = QuestAction.TakeAChance; SetQuest("[  <b>Take a chance</b>  ]\n      <color=#ffffff99>◆ Look for opportunities</color>"); } // MODIFIED
    public void SetQuest_TakeARest() { CurrentQuest = QuestAction.TakeARest; SetQuest("[  <b>Take a rest</b>  ]\n      <color=#ffffff99>◆ Go home</color>"); } // MODIFIED
    public void SetQuest_TakeARisk() { CurrentQuest = QuestAction.TakeARisk; SetQuest("[  <b>Take a risk</b>  ]\n      <color=#ffffff99>◆ Go to Vamonos</color>"); } // MODIFIED
    public void SetQuest_TakeSomeTimeOff() { CurrentQuest = QuestAction.TakeSomeTimeOff; SetQuest("[  <b>Take some time off</b>  ]\n      <color=#ffffff99>◆ Take the bus home</color>"); } // MODIFIED

    public void SetQuestFromAction(QuestAction action)
    {
        switch (action)
        {
            case QuestAction.TakeAChance:     SetQuest_TakeAChance(); break;
            case QuestAction.TakeARest:       SetQuest_TakeARest(); break;
            case QuestAction.TakeARisk:       SetQuest_TakeARisk(); break;
            case QuestAction.TakeSomeTimeOff: SetQuest_TakeSomeTimeOff(); break;
            case QuestAction.None: break;
        }
    }
}