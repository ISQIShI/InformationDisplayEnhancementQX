using Duckov.Quests;
using Duckov.Quests.Tasks;
using Duckov.UI;
using HarmonyLib;
using InformationDisplayEnhancementQX.Utils;
using SodaCraft.Localizations;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

#nullable disable
namespace InformationDisplayEnhancementQX
{
    [HarmonyPatch]
    [PatchGroup(nameof(KillCountTaskTargetDisplay))]
    public class KillCountTaskTargetDisplay : MonoBehaviour
    {
        public static KillCountTaskTargetDisplay Instance { get; private set; }

        private Dictionary<HealthBar, TargetWrapper> _targetWrappers = new Dictionary<HealthBar, TargetWrapper>();

        private Dictionary<QuestTask_KillCount, TaskWrapper> _taskWrappers = new Dictionary<QuestTask_KillCount, TaskWrapper>();

        public void Awake()
        {
            Instance = this;
        }

        public void OnDestroy()
        {
            Instance = null;
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuestManager), "Awake")]
        public static void AfterQuestManagerAwake(QuestManager __instance)
        {
            // 清理旧资源
            Instance._targetWrappers.Clear();
            Instance._taskWrappers.Clear();

            var quests = __instance.ActiveQuests;
            LogHelper.Instance.LogTest($"HandleActivatedQuest: 处理已激活的请求,共 {quests.Count} 个 Quest");
            foreach (var quest in quests)
            {
                Instance.OnQuestActivated(quest);
            }

            LogHelper.Instance.LogTest($"总计有 {Instance._taskWrappers.Count} 个 KillCount 任务被监视");

            Quest.onQuestActivated += Instance.OnQuestActivated;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(QuestManager), "OnDestroy")]
        public static void AfterQuestManagerOnDestroy()
        {
            Quest.onQuestActivated -= Instance.OnQuestActivated;
        }



        private void OnQuestActivated(Quest quest)
        {
            LogHelper.Instance.LogTest($"OnQuestActivated: {quest}");

            List<Task> tasks = quest.Tasks;
            foreach (Task task in tasks)
            {
                if (task is QuestTask_KillCount killCountTask && !_taskWrappers.ContainsKey(killCountTask))
                {
                    if (killCountTask.IsFinished())
                    {
                        LogHelper.Instance.LogTest($"OnQuestActivated: 任务已完成，跳过监视: {killCountTask}");
                        continue;
                    }

                    var taskWrapper = new TaskWrapper(this, killCountTask);
                    _taskWrappers.Add(killCountTask, taskWrapper);
                    killCountTask.onStatusChanged += taskWrapper.OnTaskStatusChanged;
                    // 尝试验证更新已经存在的 HealthBar
                    foreach (var targetWrapper in _targetWrappers.Values)
                    {
                        if (taskWrapper.VertifyTarget(targetWrapper.HealthBar))
                        {
                            if (taskWrapper.Targets.Add(targetWrapper))
                            {
                                targetWrapper.Tasks.Add(taskWrapper);
                                // 更新文本
                                targetWrapper.UpdateText(targetWrapper.CurrentAmount + (taskWrapper.RequireAmount - taskWrapper.Amount));
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HealthBar), "RegisterEvents")]
        public static void AfterHealthBarRegisterEvents(HealthBar __instance)
        {
            if (!__instance.target) return;

            if (Instance._targetWrappers.ContainsKey(__instance)) return;

            TargetWrapper targetWrapper = null;

            // 遍历所有任务检查是否满足任务要求
            foreach (var taskWrapper in Instance._taskWrappers.Values)
            {
                if (taskWrapper.VertifyTarget(__instance))
                {
                    if (targetWrapper == null)
                    {
                        targetWrapper = new TargetWrapper(__instance);
                        Instance._targetWrappers.Add(__instance, targetWrapper);
                    }
                    if (taskWrapper.Targets.Add(targetWrapper))
                    {
                        targetWrapper.Tasks.Add(taskWrapper);
                        targetWrapper.CurrentAmount += (taskWrapper.RequireAmount - taskWrapper.Amount);
                    }
                }
            }
            if (targetWrapper != null)
            {
                targetWrapper.UpdateText();
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HealthBar), "UnregisterEvents")]
        public static void AfterHealthBarUnregisterEvents(HealthBar __instance)
        {
            if (!__instance.target) return;

            if (Instance._targetWrappers.TryGetValue(__instance, out var targetWrapper))
            {
                GameObject taskTargetTextObj = __instance.transform.Find("TaskTargetTextObj")?.gameObject;
                if (taskTargetTextObj != null)
                {
                    LogHelper.Instance.LogTest($"DeathTaskPrefix: 隐藏任务目标文本: {taskTargetTextObj}");
                    taskTargetTextObj.SetActive(false);
                    // Destroy(taskTargetTextObj);
                }
                foreach (var task in targetWrapper.Tasks)
                {
                    task.Targets.Remove(targetWrapper);
                }
                targetWrapper.Tasks.Clear();
                Instance._targetWrappers.Remove(__instance);
            }

        }


        public class TaskWrapper
        {
            public KillCountTaskTargetDisplay Master { get; }

            public QuestTask_KillCount Task { get; }

            public int Amount { get; private set; }

            public int RequireAmount { get; }

            public CharacterRandomPreset RequireEnemyType { get; }

            /// <summary>
            /// 影响的目标
            /// </summary>
            public HashSet<TargetWrapper> Targets { get; } = new HashSet<TargetWrapper>();

            public static AccessTools.FieldRef<QuestTask_KillCount, int> AmountFieldRef { get; } = AccessTools.FieldRefAccess<QuestTask_KillCount, int>("amount");

            public static AccessTools.FieldRef<QuestTask_KillCount, int> RequireAmountFieldRef { get; } = AccessTools.FieldRefAccess<QuestTask_KillCount, int>("requireAmount");

            public static AccessTools.FieldRef<QuestTask_KillCount, CharacterRandomPreset> RequireEnemyTypeFieldRef { get; } = AccessTools.FieldRefAccess<QuestTask_KillCount, CharacterRandomPreset>("requireEnemyType");

            public TaskWrapper(KillCountTaskTargetDisplay master, QuestTask_KillCount task)
            {
                Master = master;
                Task = task;
                // 解析任务信息
                Amount = AmountFieldRef(task);
                RequireAmount = RequireAmountFieldRef(task);
                RequireEnemyType = RequireEnemyTypeFieldRef(task);

                Debug.Log($"TaskWrapper 被创建: task={task}, RequireEnemyType={RequireEnemyType}, RequireAmount={RequireAmount}, Amount={Amount}");
            }

            public void OnTaskStatusChanged(Task task)
            {
                if (task is QuestTask_KillCount killCountTask)
                {
                    int decrement = 0;
                    bool finished = killCountTask.IsFinished();
                    if (finished)
                    {
                        killCountTask.onStatusChanged -= OnTaskStatusChanged;
                        decrement = RequireAmount - Amount;
                        Amount = RequireAmount;

                        foreach (var healthBarWrapper in Targets)
                        {
                            int newAmount = healthBarWrapper.CurrentAmount - decrement;
                            healthBarWrapper.UpdateText(newAmount);
                            healthBarWrapper.Tasks.Remove(this);
                        }
                        Targets.Clear();

                        Master._taskWrappers.Remove(killCountTask);
                    }
                    else
                    {
                        int currentAmount = AmountFieldRef(killCountTask);
                        decrement = currentAmount - Amount;
                        Amount = currentAmount;

                        foreach (var healthBarWrapper in Targets)
                        {
                            int newAmount = healthBarWrapper.CurrentAmount - decrement;
                            healthBarWrapper.UpdateText(newAmount);
                        }

                    }
                }
                else
                {
                    Debug.LogError($"{typeof(TaskWrapper).FullName}::OnTaskStatusChanged: task 不是 QuestTask_KillCount 类型");
                }
            }

            public bool VertifyTarget(HealthBar healthBar)
            {
                Health health = healthBar.target;
                if (!health) return false;
                // 过滤玩家阵营
                if (health.team == Teams.player)
                {
                    return false;
                }
                // 检测场景是否满足
                if (!Task.SceneRequirementSatisfied) return false;
                // 检测敌人类型是否满足
                if (RequireEnemyType != null)
                {

                    CharacterMainControl characterMainControl = health.TryGetCharacter();
                    if (!characterMainControl) return false;
                    CharacterRandomPreset characterPreset = characterMainControl.characterPreset;
                    if (!characterPreset || characterPreset.nameKey != RequireEnemyType.nameKey) return false;
                }
                Debug.Log($"TaskWrapper::VertifyTarget: 任务目标匹配成功: healthBar={healthBar}, RequireEnemyType={RequireEnemyType}");
                return true;
            }
        }

        public class TargetWrapper
        {
            public HealthBar HealthBar { get; }

            private GameObject _taskTargetTextObj;

            public GameObject TaskTargetTextObj
            {
                get
                {
                    if (_taskTargetTextObj == null)
                    {
                        _taskTargetTextObj = TextHelper.Instance.GetText(new TextHelper.TextConfigure
                        {
                            active = true,
                            parent = HealthBar.transform,
                            localPosition = new Vector3(0, -150, 0),
                            localScale = Vector3.one,
                            textTemplateName = "TaskTargetTextObj",
                            textName = "TaskTargetText"
                        });
                    }
                    return _taskTargetTextObj;
                }
            }

            private int _currentAmount = 0;

            public int CurrentAmount
            {
                get
                {
                    return _currentAmount;
                }
                set
                {
                    if (_currentAmount != value)
                    {
                        _currentAmount = value;
                        IsDirty = true;
                    }
                }
            }

            public bool IsDirty { get; private set; } = false;

            /// <summary>
            /// 受影响的任务
            /// </summary>
            public HashSet<TaskWrapper> Tasks { get; } = new HashSet<TaskWrapper>();

            public TargetWrapper(HealthBar healthBar)
            {
                HealthBar = healthBar;
                _taskTargetTextObj = HealthBar.transform.Find("TaskTargetTextObj")?.gameObject;
                if (_taskTargetTextObj != null)
                {
                    _taskTargetTextObj.SetActive(true);
                }
            }

            public void UpdateText(int amount)
            {
                CurrentAmount = amount;
                UpdateText();
            }

            public void UpdateText()
            {
                if (!IsDirty) return;
                var taskTargetTextObj = TaskTargetTextObj;
                if (CurrentAmount == 0)
                {
                    taskTargetTextObj.SetActive(false);
                    return;
                }
                GameObject taskTargetText = taskTargetTextObj.transform.Find("TaskTargetText").gameObject;
                if (taskTargetText.TryGetComponent<TextMeshProUGUI>(out var textMeshProUGUI))
                {
                    textMeshProUGUI.text = $"{($"{LocalizationHelper.KeyPrefix}Text_TaskTarget").ToPlainText()}{CurrentAmount:N0}";
                }
                IsDirty = false;
            }
        }
    }
}