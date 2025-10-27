using Duckov.Quests;
using Duckov.Quests.Tasks;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;

#nullable disable
namespace InformationDisplayEnhancementQX
{
    //[HarmonyPatch]
    public class TaskObjectiveMarker : MonoBehaviour
    {
        public static TaskObjectiveMarker Instance { get; private set; }

        private Dictionary<string, int> killTargetAmount = new Dictionary<string, int>();

        private Dictionary<QuestTask_KillCount, int> lastAmount = new Dictionary<QuestTask_KillCount, int>();

        private Dictionary<string, LinkedList<TextMeshProUGUI>> killTargetListeners = new Dictionary<string, LinkedList<TextMeshProUGUI>>();

        public Dictionary<TextMeshProUGUI, string> TextToType { get; } = new Dictionary<TextMeshProUGUI, string>();

        private static FieldInfo _requireTypeFieldInfo;

        private static FieldInfo _requireAmountFieldInfo;

        private static FieldInfo _amountFieldInfo;

        static TaskObjectiveMarker()
        {
            var bindingFlags = System.Reflection.BindingFlags.Instance |
                       System.Reflection.BindingFlags.Public |
                       System.Reflection.BindingFlags.NonPublic;
            _requireTypeFieldInfo = typeof(QuestTask_KillCount).GetField("requireEnemyType", bindingFlags);
            _requireAmountFieldInfo = typeof(QuestTask_KillCount).GetField("requireAmount", bindingFlags);
            _amountFieldInfo = typeof(QuestTask_KillCount).GetField("amount", bindingFlags);
        }


        public void Awake()
        {
            Instance = this;
        }

        public void OnDestroy()
        {
            Instance = null;
        }

        public void OnEnable()
        {
            Quest.onQuestActivated += OnQuestActivated;
        }

        public void OnDisable()
        {
            Quest.onQuestActivated -= OnQuestActivated;
        }

        public void AddListener(string typeName, TextMeshProUGUI textMeshProUGUI)
        {
            if (killTargetListeners.TryGetValue(typeName, out var linkedList))
            {
                linkedList.AddLast(textMeshProUGUI);
            }
            else
            {
                linkedList = new LinkedList<TextMeshProUGUI>();
                linkedList.AddLast(textMeshProUGUI);
                killTargetListeners.Add(typeName, linkedList);
            }
        }

        public void RemoveListener(string typeName, TextMeshProUGUI textMeshProUGUI)
        {
            if (killTargetListeners.TryGetValue(typeName, out var linkedList))
            {
                linkedList.Remove(textMeshProUGUI);
                if (linkedList.Count == 0)
                {
                    killTargetListeners.Remove(typeName);
                }
            }
        }


        public bool IsKillTarget(string typeName)
        {
            // Debug.LogWarning($"IsKillTarget: {typeName}, hasSpecific: {killTargetAmount.ContainsKey(typeName)}, hasAny: {killTargetAmount.ContainsKey(string.Empty)}");
            return killTargetAmount.ContainsKey(typeName) || killTargetAmount.ContainsKey(string.Empty);
        }

        public int GetKillTargetAmount(string typeName)
        {
            int amount = 0;
            if (killTargetAmount.TryGetValue(typeName, out int currentAmount))
            {
                amount += currentAmount;
            }
            if (typeName == string.Empty) return amount;
            if (killTargetAmount.TryGetValue(string.Empty, out currentAmount))
            {
                amount += currentAmount;
            }
            return amount;
        }

        private void OnQuestActivated(Quest quest)
        {
            Debug.LogWarning($"OnQuestActivated: {quest}");
            var typeFieldInfo = _requireTypeFieldInfo;
            var requireAmountFieldInfo = _requireAmountFieldInfo;
            var amountFieldInfo = _amountFieldInfo;
            List<Task> tasks = quest.Tasks;
            foreach (Task task in tasks)
            {
                if (task is QuestTask_KillCount killCountTask)
                {
                    Debug.LogWarning($"OnQuestActivated: found killCountTask={killCountTask}");
                    CharacterRandomPreset enemyType = (CharacterRandomPreset)typeFieldInfo.GetValue(killCountTask);
                    Debug.LogWarning($"OnQuestActivated: enemyType={enemyType}");
                    int requiredAmount = (int)requireAmountFieldInfo.GetValue(killCountTask);
                    Debug.LogWarning($"OnQuestActivated: requiredAmount={requiredAmount}");
                    int amount = (int)amountFieldInfo.GetValue(killCountTask);
                    Debug.LogWarning($"OnQuestActivated: amount={amount}");
                    var typeName = enemyType?.nameKey;
                    Debug.LogWarning($"OnQuestActivated: task={killCountTask}, typeName={typeName}, requiredAmount={requiredAmount}, amount={amount}");
                    if (typeName is null) typeName = string.Empty;
                    if (killTargetAmount.TryGetValue(typeName, out var lastAmount))
                    {
                        killTargetAmount[typeName] = lastAmount + (requiredAmount - amount);

                    }
                    else
                    {
                        killTargetAmount[typeName] = (requiredAmount - amount);
                    }

                    this.lastAmount[killCountTask] = amount;
                    killCountTask.onStatusChanged += OnTaskStatusChanged;
                }
            }
        }

        private void OnTaskStatusChanged(Task task)
        {
            bool finished = task.IsFinished();
            if (finished) task.onStatusChanged -= OnTaskStatusChanged;

            var killCountTask = task as QuestTask_KillCount;
            if (killCountTask != null)
            {
                var typeFieldInfo = _requireTypeFieldInfo;
                CharacterRandomPreset enemyType = (CharacterRandomPreset)typeFieldInfo.GetValue(killCountTask);
                var typeName = enemyType?.nameKey;
                if (typeName is null) typeName = string.Empty;
                int deltaAmount = 0;
                if (finished)
                {
                    var requireAmountFieldInfo = _requireAmountFieldInfo;
                    int requiredAmount = (int)requireAmountFieldInfo.GetValue(killCountTask);
                    deltaAmount = requiredAmount - lastAmount[killCountTask];
                    lastAmount.Remove(killCountTask);
                }
                else
                {
                    var amountFieldInfo = _amountFieldInfo;
                    int currentAmount = (int)amountFieldInfo.GetValue(killCountTask);
                    deltaAmount = currentAmount - lastAmount[killCountTask];
                    lastAmount[killCountTask] = currentAmount;
                }
                killTargetAmount[typeName] -= deltaAmount;
                if (typeName == string.Empty)
                {
                    // 全类型目标变化，更新所有监听器
                    foreach (var kvp in killTargetListeners)
                    {
                        var tName = kvp.Key;
                        var listeners = kvp.Value;
                        int amount = GetKillTargetAmount(tName);
                        string text = string.Empty;
                        if (amount > 0) text = $"任务目标：{amount}";
                        foreach (var listener in listeners)
                        {
                            listener.text = text;
                        }
                    }
                }
                else
                {
                    if (killTargetListeners.TryGetValue(typeName, out var listeners))
                    {
                        int amount = GetKillTargetAmount(typeName);
                        string text = string.Empty;
                        if (amount > 0) text = $"任务目标：{amount}";
                        foreach (var listener in listeners)
                        {
                            listener.text = text;
                        }
                    }
                }

                if (killTargetAmount[typeName] == 0)
                {
                    killTargetAmount.Remove(typeName);
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuestManager), "Load")]
        public static void HandleActivatedQuest()
        {
            var quests = QuestManager.Instance.ActiveQuests;
            foreach (var quest in quests)
            {
                Instance.OnQuestActivated(quest);
            }
        }
    }
}
