# 项目记忆（My project (2)）

> 这是本项目的历史总结与长期上下文，供之后的对话自动参考。内容基于本地历史对话整理。

## 项目概况

- 项目类型：JRPG 回合制角色扮演游戏（Unity / 团结引擎 Tuanjie）。
- 项目路径：`C:\unity\My project (2)`
- 引擎版本：Unity `2022.3.61t5` / 团结引擎 `1.6.4`。
- 用途背景：学校「专业综合课程设计」项目，项目周期 `2026-09-01` 至 `2026-09-25`。
  - 计划表参考：`C:\Users\tanxu\Downloads\1_【组号】_【学号姓名】_专业综合课程设计_计划表.xlsx`
- 版本控制：Git，远程 `origin/main`，已打过 `1.0` 标签。

## 目录与场景结构

- 探索地图场景：`Assets/Scenes/forest/BattleForest.scene`
- 战斗场景：原为 `Assets/Scenes/CombatScene/CombatScene.scene`，最近改名/迁移为
  `Assets/Scenes/kingdom/kingdom.scene`（迁移尚未完全收尾，见“待办”）。
- 核心代码集中在 `Assets/ziyuan/`：
  - `gm/`：GameManager、BattleManager、BattleUIController、SkillUIController、
    PauseMenu、PartyMenuController、InventoryManager、InventoryMenuController、
    BedSleep、HomeDoorTeleport、SaveSystem、BattleResultUI、PauseMenuStyle。
  - `gm/BattleServices/`：BattleTurnController、BattleRewardService、
    BattleEnemyController、BattleActionResolver（战斗逻辑已拆服务化）。
  - `statControl/`：CombatUnit、PlayerUnit、EnemyUnit。
  - `data/`：SkillData、PlayerData、EnemyData、BattleConfig、DataDefinitions、
    RuntimeDataHelpers；数据文件在 `data/skills/`、`data/players/`、`data/enemies/`、
    `data/battle/`。
  - `monsters/wolf/`：WolfAI、EncounterTrigger。
  - `monsters/slime/`：SlimeAI（史莱姆只会随机走动 + 攻击）。
  - `Craven/`：玩家移动（PlayerMovement）。
  - `tests/Editor/`：GameDataTests。
- 主角名字：`Craven`（Tag 为 `Player`）。

## 已实现功能

- 随机多敌人遭遇、回合制战斗、经验升级、速度/攻击/防御属性参与实际战斗计算。
- 队伍/编队系统（PARTY 页面，显示队伍与角色）。
- 战斗 UI（BattleCanvas、命令面板、敌人面板、队伍面板）。
- 技能系统：固定三技能「普通技能 / 大招 / 回血」，消耗 MP，技能 UI 独立在
  `SkillUIController`。
- 暂停菜单、存档/读档（SaveSystem / PlayerPrefs）、背包系统（JRPG 风格，
  Potion 回 30 HP、Ether 回 20 MP，支持堆叠与存档）。
- 战斗胜利结算页：显示经验、升级、掉落物；`CONTINUE` 后返回地图并回到战斗前位置。
- 击败怪后地图怪物消失并持久化；逃跑/战败不消失。
- 睡觉功能：靠近床按 `F` 回满 HP/MP、刷新地图怪物（`BedSleep`）。
- 家（小屋）：`BattleForest` 内同场景传送（`HomeDoorTeleport`，同一个脚本做进出）。
- 摄像机：八方旅人式斜俯视跟随（Virtual Camera，FOV 45，斜上偏移，看向 Craven）。
  已移除 GameManager 里硬编码相机参数，改为由 Inspector 控制。
- 中文/英文字体处理：早期中文显示为方框改为英文，后期补充了中文字体和 TMP 字体图集。
- 数据驱动重构：技能/角色/怪物/战斗配置外部化为 `.asset`（ScriptableObject），
  战斗逻辑拆到 `gm/BattleServices/`，并新增 Editor 测试。

## 最近一次改动（部分未推送）

- 本地最新提交：`ed3404bf Update battle data and combat systems`（已提交到本地 `main`，
  但推 GitHub 失败，原因是当时无法连接 `github.com:443`）。
- 工作区有未提交改动：删除 `CombatScene` 目录、删除 Pure Poly Nature Pack 的 Demo 场景、
  新增 `Assets/New Terrain.asset`、新增 `Assets/Scenes/kingdom/`（`kingdom.scene`）。

## 待办 / 需要注意

- 战斗场景从 `CombatScene` 改名为 `kingdom` 尚未收尾：仍有脚本硬编码 `"CombatScene"`，
  例如 `EncounterTrigger.cs`（`SceneManager.LoadScene("CombatScene")`）和
  `SaveLoadController.cs`（`battleSceneName = "CombatScene"`）。若继续改名，需同步更新这些
  引用并确认 Build Settings 场景顺序。
- 网络恢复后推送远端：`git push origin main`。

## 用户偏好（重要）

- 修改 `.scene` 或场景内对象前，先提醒用户按 `Ctrl+S` 保存；纯脚本文件修改不受影响。
- 数据驱动优先：角色/怪物的技能与面板用 data（ScriptableObject）控制，不要硬编码在代码里；
  保持代码分类清晰，不要把东西全堆进 `GameManager` 或 `BattleUIController`。
- Inspector 字段偏好固定字段直接拖拽绑定，不要用 List / Size / 加号。
- 不要动无关的未提交改动（例如历史上 PlayerMovement.cs 的未提交修改曾被要求保留）。
