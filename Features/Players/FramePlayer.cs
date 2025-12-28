using Exiled.API.Features;
using Exiled.CustomRoles.API;
using Exiled.CustomRoles.API.Features;
using Exiled.Events.EventArgs.Player;
using Exiled.Events.Features;
using MEC;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using YongAnFrame.Events.EventArgs.FramePlayer;
using YongAnFrame.Extensions;
using YongAnFrame.Features.Players.Interfaces;
using YongAnFrame.Features.Roles;
using YongAnFrame.Features.UI.Enums;
using YongAnFrame.Features.UI.Texts;

namespace YongAnFrame.Features.Players
{
    /// <summary>
    /// 永安框架的玩家类
    /// </summary>
    public sealed class FramePlayer : ICustomAlgorithm
    {
        private PlayerTitle? usingTitles = null;
        private PlayerTitle? usingRankTitles = null;
        private static readonly Dictionary<int, FramePlayer> dictionary = [];

        private Player? exPlayer;
        /// <summary>
        /// 获取该实例拥有的<seealso cref="Player"/>
        /// </summary>
        /// <remarks>
        /// 在运行<seealso cref="Events.Handlers.FramePlayer.FramePlayerInvalidating"/>事件后实例无效，再调用可能会引发<seealso cref="InvalidCastException"/>异常<br/>
        /// 玩家退出后必须不再引用<seealso cref="Player"/>，否则会造成<seealso cref="Player"/>数字ID重复的问题
        /// </remarks>
        public Player ExPlayer
        {
            get
            {
                if (exPlayer is null)
                {
                    throw new InvalidCastException("FramePlayer实例已无效");
                }
                return exPlayer;
            }
        }
        /// <summary>
        /// 获取有效的框架玩家列表
        /// </summary>
        public static IReadOnlyCollection<FramePlayer> List => [.. dictionary.Values];
        /// <summary>
        /// 获取玩家拥有的自定义角色
        /// </summary>
        public CustomRolePlus? CustomRolePlus
        {
            get
            {
                ReadOnlyCollection<CustomRole> customRoleList = ExPlayer.GetCustomRoles();
                if (customRoleList.Count != 0 && customRoleList[0] is CustomRolePlus custom)
                {
                    return custom;
                }
                return null;
            }
        }
        /// <summary>
        /// 获取玩家的UI
        /// </summary>
        public PlayerUI UI { get; private set; }
        /// <summary>
        /// 获取或设置玩家正在使用的主要自定义算法
        /// </summary>
        public ICustomAlgorithm CustomAlgorithm { get; set; }

        /// <summary>
        /// 获取或设置玩家的等级
        /// </summary>
        public ulong Level { get; set; }
        /// <summary>
        /// 获取或设置玩家的经验
        /// </summary>
        public ulong Exp { get; set; }
        /// <summary>
        /// 获取全局的经验加成
        /// </summary>
        public float GlobalExpMultiplier => YongAnFramePlugin.Instance.Config.GlobalExpMultiplier;
        /// <summary>
        /// 获取或设置玩家的经验倍率
        /// </summary>
        public float ExpMultiplier { get; set; }
        /// <summary>
        /// 获取或设置玩家的批准绕过DNT
        /// </summary>
        public bool IsBDNT { get; set; } = false;
        /// <summary>
        /// 获取玩家是否无效
        /// </summary>
        public bool IsInvalid => exPlayer is null;
        public List<PlayerTitle> PosTitles { get; private set; } = [];
        /// <summary>
        /// 获取或设置玩家正在使用的名称称号
        /// </summary>
        public PlayerTitle? UsingTitles
        {
            get => usingTitles;
            set
            {
                if (value is not null && !value.IsRank)
                {
                    usingTitles = value;
                }
            }
        }

        /// <summary>
        /// 获取或设置玩家正在使用的地位称号
        /// </summary>
        public PlayerTitle? UsingRankTitles
        {
            get => usingRankTitles;
            set
            {
                if (value is not null && value.IsRank)
                {
                    usingRankTitles = value;
                }
            }
        }

        #region EX增强
        /// <summary>
        /// 获取或设置玩家的地位名称。
        /// </summary>
        public string? RankName
        {
            get => ExPlayer.RankName;
            set
            {
                if (RankName != value)
                {
                    ExPlayer.RankName = value;
                }
            }
        }
        /// <summary>
        /// 获取或设置玩家的地位颜色。
        /// </summary>
        public string? RankColor
        {
            get => ExPlayer.RankColor;
            set
            {
                if (RankColor != value)
                {
                    ExPlayer.RankColor = value;
                }
            }
        }
        /// <summary>
        /// 获取或设置玩家的昵称，如果为 null，则设置原始昵称。
        /// </summary>
        public string CustomName
        {
            get => ExPlayer.CustomName;
            set
            {
                if (CustomName != value)
                {
                    ExPlayer.CustomName = value;
                }
            }
        }
        #endregion

        #region Static
        /// <summary>
        /// 注册全局事件
        /// </summary>
        public static void SubscribeStaticEvents()
        {
            Exiled.Events.Handlers.Player.Verified += new CustomEventHandler<VerifiedEventArgs>(OnStaticVerified);
            Exiled.Events.Handlers.Player.Destroying += new CustomEventHandler<DestroyingEventArgs>(OnStaticDestroying);
        }
        /// <summary>
        /// 注销全局事件
        /// </summary>
        public static void UnsubscribeStaticEvents()
        {
            Exiled.Events.Handlers.Player.Verified -= new CustomEventHandler<VerifiedEventArgs>(OnStaticVerified);
            Exiled.Events.Handlers.Player.Destroying -= new CustomEventHandler<DestroyingEventArgs>(OnStaticDestroying);
        }

        private static void OnStaticVerified(VerifiedEventArgs args)
        {
            Load(args.Player)!.UpdateShowInfo();
        }
        private static void OnStaticDestroying(DestroyingEventArgs args)
        {
            FramePlayer fPlayer = args.Player.ToFPlayer();
            if (!fPlayer.IsInvalid)
            {
                fPlayer.Invalid();
            }
        }

        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="player">Exiled玩家</param>
        internal FramePlayer(Player player)
        {
            exPlayer = player;
            dictionary.Add(ExPlayer.Id, this);
            UI = new(this);
            CustomAlgorithm = this;
            Events.Handlers.FramePlayer.OnFramePlayerCreated(new FramePlayerCreatedEventArgs(this));
        }

        /// <summary>
        /// 添加经验
        /// </summary>
        /// <param name="exp">数值</param>
        /// <param name="name">原因</param>
        public void AddExp(ulong exp, string name = "未知原因")
        {
            float expMultiplier = ExpMultiplier * GlobalExpMultiplier;
            ulong addExp = (ulong)(exp * expMultiplier);

            Exp += addExp;
            UI.MessageList.Add(new MessageText($"{name}，获得{exp}+{addExp - exp}经验({expMultiplier}倍经验)", 5, MessageType.System));

            ulong needExp = CustomAlgorithm.GetNeedUpLevel(Level);
            ulong oldLevel = Level;
            while (Exp >= needExp)
            {
                Log.Debug($"{Exp}/{needExp}");
                Level++;
                Exp -= needExp;
                needExp = CustomAlgorithm.GetNeedUpLevel(Level);
            }
            if (oldLevel < Level)
            {
                UpdateShowInfo();
                UI.MessageList.Add(new MessageText($"恭喜你从{oldLevel}级到达{Level}级,距离下一级需要{Exp}/{needExp}经验", 8, MessageType.System));
            }
        }


        #region ShowRank

        private readonly CoroutineHandle[] coroutines = new CoroutineHandle[2];

        /// <summary>
        /// 更新显示的服务器列表信息
        /// </summary>
        public void UpdateShowInfo()
        {
            if (ExPlayer.GlobalBadge is not null)
            {
                CustomName = $"[LV:{Level}][全球徽章]{ExPlayer.Nickname}";
                if (CustomRolePlus is not null)
                {
                    RankName = $"*{ExPlayer.GlobalBadge.Value.Text}* {CustomRolePlus.Name}";
                }
                else
                {
                    RankName = $"{ExPlayer.GlobalBadge.Value.Text}";
                }
                RankColor = $"{ExPlayer.GlobalBadge.Value.Color}";
                return;
            }

            string? rankColor = null;
            string? rankName = null;

            if (CustomRolePlus is not null)
            {
                rankName = CustomRolePlus.Name;
                rankColor = CustomRolePlus.NameColor;
            }

            if (usingTitles is not null)
            {
                if (usingTitles.DynamicCommand is not null)
                {
                    Timing.KillCoroutines(coroutines[1]);
                    coroutines[1] = Timing.RunCoroutine(DynamicTitlesShow());
                }
                else
                {
                    CustomName = $"[LV:{Level}][{usingTitles.Name}]{ExPlayer.Nickname}";
                    if (!string.IsNullOrEmpty(usingTitles.Color))
                    {
                        rankColor = usingTitles.Color;
                    }
                }
            }
            else
            {
                ExPlayer.CustomName = $"[LV:{Level}]{ExPlayer.Nickname}";
            }

            if (usingRankTitles is not null)
            {
                if (usingRankTitles.DynamicCommand is not null)
                {
                    Timing.KillCoroutines(coroutines[0]);
                    coroutines[0] = Timing.RunCoroutine(DynamicRankTitlesShow());
                }
                else
                {
                    if (CustomRolePlus is not null)
                    {
                        rankName = $"{CustomRolePlus.Name} *{usingRankTitles.Name}*";
                    }
                    else
                    {
                        rankName = usingRankTitles.Name;
                    }

                    if (!string.IsNullOrEmpty(usingRankTitles.Color))
                    {
                        rankColor = usingRankTitles.Color;
                    }
                }
            }

            RankColor = rankColor;
            RankName = rankName;
        }

        private IEnumerator<float> DynamicRankTitlesShow()
        {
            while (true)
            {
                if (usingRankTitles is null || usingRankTitles.DynamicCommand is null)
                {
                    yield break;
                }
                foreach (var command in usingRankTitles.DynamicCommand)
                {
                    if (CustomRolePlus is not null)
                    {
                        RankName = $"{CustomRolePlus.Name} *{command[0]}*";
                    }
                    else
                    {
                        RankName = $"{command[0]}";
                    }
                    if (usingRankTitles is null)
                    {
                        RankColor = command[1];
                    }
                    yield return Timing.WaitForSeconds(float.Parse(command[2]));
                }
            }
        }
        private IEnumerator<float> DynamicTitlesShow()
        {
            while (true)
            {
                if (usingTitles is null || usingTitles.DynamicCommand is null)
                {
                    yield break;
                }
                foreach (var command in usingTitles.DynamicCommand)
                {
                    CustomName = $"[LV:{Level}][{command[0]}]{ExPlayer.Nickname}";
                    if (usingRankTitles is null)
                    {
                        RankColor = command[1];
                    }
                    yield return Timing.WaitForSeconds(float.Parse(command[2]));
                }
            }
        }
        #endregion

        ///<inheritdoc/>
        public ulong GetNeedUpLevel(ulong level) => (ulong)(100 + Math.Floor(level / 10f) * 100);

        /// <summary>
        /// 获取框架玩家
        /// </summary>
        /// <param name="player">Exiled玩家</param>
        /// <returns>框架玩家</returns>
        public static FramePlayer Get(Player? player)
        {
            if (player is null)
            {
                throw new InvalidCastException("Player实例无效");
            }
            if (!dictionary.TryGetValue(player.Id, out FramePlayer framePlayer))
            {
                throw new InvalidCastException("FramePlayer实例无效");
            }
            return framePlayer;
        }

        /// <summary>
        /// 获取框架玩家
        /// </summary>
        /// <param name="numId">玩家数字ID</param>
        /// <returns>框架玩家</returns>
        public static FramePlayer Get(int numId) => Get(Player.Get(numId));

        /// <summary>
        /// 不推荐直接访问，请尝试使用ToFPlayer()
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public static FramePlayer? Load(Player player)
        {
            if (player.IsNPC)
            {
                return new(player)
                {
                    Level = 1,
                    Exp = 0,
                    PosTitles = [],
                    UsingTitles = null,
                    UsingRankTitles = null,
                };
            }

            FramePlayer? framePlayer = null;

            string queryString = "select * from player_data where Id = @Id";
            try
            {
                using MySqlConnection connection = new(YongAnFramePlugin.Instance.ConnectionString);
                connection.Open();
                using MySqlCommand command = new(queryString, connection);
                command.Parameters.AddWithValue("Id", player.UserId);
                using MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    List<PlayerTitle> posTitles = [];
                    if (reader["PosTitles"].ToString() != "")
                    {
                        foreach (string posTitleIdString in reader["PosTitles"].ToString().Split(','))
                        {
                            PlayerTitle? title = PlayerTitle.Get(uint.Parse(posTitleIdString));
                            if (title != null)
                            {
                                posTitles.Add(title);
                            }
                        }
                    }

                    framePlayer = new(player)
                    {
                        Level = (ulong)reader["Level"],
                        Exp = (ulong)reader["Exp"],
                        ExpMultiplier = (float)reader["ExpMultiplier"],
                        PosTitles = posTitles,
                        UsingTitles = PlayerTitle.Get((uint)reader["UsingTitles"]),
                        UsingRankTitles = PlayerTitle.Get((uint)reader["UsingRankTitles"]),
                    };
                }
            }
            catch (Exception text)
            {
                Log.Error($"数据库查找FramePlayer数据异常({player.UserId}) 错误原因:{text}");
                if (YongAnFramePlugin.Instance.Config.IsMySqlErrorKick)
                {
                    player.Kick("不要慌张！你的数据库数据可能存在异常，为了保证你的游戏数据不被覆盖，你已被踢出服务器！\n请将错误联系到管理员(数据库查找FramePlayer数据异常)");
                }
                return null;
            }

            if (framePlayer == null)
            {
                framePlayer = new(player)
                {
                    Level = 1,
                    Exp = 0,
                    ExpMultiplier = 1,
                    PosTitles = [],
                    UsingTitles = null,
                    UsingRankTitles = null,
                };
                try
                {
                    using MySqlConnection connection = new(YongAnFramePlugin.Instance.ConnectionString);
                    connection.Open();
                    using MySqlCommand cmd = new("insert into player_data set Id=@Id,Level=@Level,Exp=@Exp,ExpMultiplier=@ExpMultiplier,PosTitles=@PosTitles,UsingTitles=@UsingTitles,UsingRankTitles=@UsingRankTitles", connection);
                    cmd.Parameters.AddWithValue("Id", framePlayer.ExPlayer.UserId);
                    cmd.Parameters.AddWithValue("Level", framePlayer.Level);
                    cmd.Parameters.AddWithValue("Exp", framePlayer.Exp);
                    cmd.Parameters.AddWithValue("ExpMultiplier", framePlayer.ExpMultiplier);
                    string posTitleString = "";
                    foreach (var item in framePlayer.PosTitles)
                    {
                        posTitleString += $",{item.Id}";
                    }
                    if (posTitleString.Length > 0)
                        posTitleString.Remove(0, 1);

                    cmd.Parameters.AddWithValue("PosTitles", posTitleString);
                    cmd.Parameters.AddWithValue("UsingTitles", framePlayer.UsingTitles != null ? framePlayer.UsingTitles.Id : 0);
                    cmd.Parameters.AddWithValue("UsingRankTitles", framePlayer.UsingRankTitles != null ? framePlayer.UsingRankTitles.Id : 0);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception text)
                {
                    Log.Error("数据库插入数据异常 错误原因:" + text);
                    return null;
                }
            }

            return framePlayer;
        }

        public bool Save()
        {
            if (ExPlayer.IsNPC || (!IsBDNT && ExPlayer.DoNotTrack)) return false;

            try
            {
                using MySqlConnection connection = new(YongAnFramePlugin.Instance.ConnectionString);
                connection.Open();
                using MySqlCommand cmd = new("update player_data set Level=@Level,Exp=@Exp,ExpMultiplier=@ExpMultiplier,PosTitles=@PosTitles,UsingTitles=@UsingTitles,UsingRankTitles=@UsingRankTitles where Id=@Id", connection);
                cmd.Parameters.AddWithValue("Id", ExPlayer.UserId);
                cmd.Parameters.AddWithValue("Exp", Exp);
                cmd.Parameters.AddWithValue("ExpMultiplier", ExpMultiplier);
                cmd.Parameters.AddWithValue("Level", Level);
                string posTitleString = "";
                foreach (var item in PosTitles)
                {
                    posTitleString += $",{item.Id}";
                }
                if (posTitleString.Length > 0)
                    posTitleString.Remove(0, 1);
                cmd.Parameters.AddWithValue("PosTitles", posTitleString);
                cmd.Parameters.AddWithValue("UsingTitles", UsingTitles != null ? UsingTitles.Id : 0);
                cmd.Parameters.AddWithValue("UsingRankTitles", UsingRankTitles != null ? UsingRankTitles.Id : 0);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception text)
            {
                Log.Error("数据库数据更新异常 错误原因:" + text);
                return false;
            }
        }

        /// <summary>
        /// 调用后该实例会立刻无效<br/>
        /// 调用后该实例会立刻无效<br/>
        /// 调用后该实例会立刻无效
        /// </summary>
        public void Invalid()
        {
            Save();
            Events.Handlers.FramePlayer.OnFramePlayerInvalidating(new FramePlayerInvalidatingEventArgs(this));
            CustomRolePlus?.RemoveRole(this);
            dictionary.Remove(ExPlayer.Id);
            UI.Clean();
            exPlayer = null;
        }

        /// <summary>
        /// 隐性转换
        /// </summary>
        /// <param name="framePlayer">框架玩家</param>
        public static implicit operator Player(FramePlayer framePlayer) => framePlayer.ExPlayer;

        /// <summary>
        /// 隐性转换
        /// </summary>
        /// <param name="framePlayer">框架玩家</param>
        public static implicit operator ReferenceHub(FramePlayer framePlayer) => framePlayer;

    }
}
