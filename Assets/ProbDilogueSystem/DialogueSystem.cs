using System.Collections.Generic;
using UnityEngine;

public class DialogueSystem : MonoBehaviour
{
    Dictionary<string, string> playerLookup = new Dictionary<string, string>();
    Dictionary<int, ConversationData> conversationSetup = new Dictionary<int, ConversationData>();
    void Start()
    {
        DummyData();
        PlayConversation(1);
        PlayConversation(2);

    }
    void AddDialogueData(int convID, string speakerID, string message)
    {
        if (!conversationSetup.ContainsKey(convID))
        {
            conversationSetup[convID] = new ConversationData();
        }
        conversationSetup[convID].Dialogues.Add(new Dialogue { SpeakerID = speakerID, Message = message });
    }
    void PlayConversation(int convID)
    {
        ConversationData data = conversationSetup[convID];
        Debug.Log("Conversation " + convID);
        for (int i = 0; i < data.Dialogues.Count; i++)
        {
            Debug.Log(playerLookup[data.Dialogues[i].SpeakerID] + ":" + data.Dialogues[i].Message);
        }
    }
    [System.Serializable]
    public class Dialogue
    {
        public string SpeakerID;
        public string Message;
    }
    [System.Serializable]
    public class ConversationData
    {
        public List<Dialogue> Dialogues = new List<Dialogue>();
    }
    void DummyData()
    {
        playerLookup["P1"] = "Alice";
        playerLookup["P2"] = "Bob";
        playerLookup["P3"] = "Charlie";
        AddDialogueData(1, "P1", "Hey Bob!");
        AddDialogueData(1, "P2", "Hey Alice!");
        AddDialogueData(2, "P2", "Yo Charlie!");
        AddDialogueData(2, "P3", "Bob my man!");
    }
}