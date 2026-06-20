using System;
using System.Collections.Generic;
using Core;

namespace QuestSystem
{
    public interface IQuestService : ISavable<QuestSaveData>
    {
        IReadOnlyList<Quest> Quests { get; }
        event Action Changed;
        event Action<Quest> QuestChanged;
        event Action<Quest> QuestRewardClaimed;

        Quest GetQuest(string id);
        IReadOnlyList<QuestViewData> GetAllViewData();
    }
}
