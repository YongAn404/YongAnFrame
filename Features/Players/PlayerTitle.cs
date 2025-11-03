using Exiled.API.Features;
using MySqlConnector;
using System;
using System.Collections.Generic;

namespace YongAnFrame.Features.Players
{
    /// <summary>
    /// <seealso cref="FramePlayer"/>的称号
    /// </summary>
    public sealed class PlayerTitle
    {
        private static readonly Dictionary<uint, PlayerTitle> dictionary = [];
        /// <summary>
        /// 获取有效的玩家称号列表
        /// </summary>
        public static IReadOnlyCollection<PlayerTitle> List => [.. dictionary.Values];

        /// <summary>
        /// 获取或设置称号的ID
        /// </summary>
        public uint Id { get; set; }
        /// <summary>
        /// 获取或设置称号的名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 获取或设置称号的颜色
        /// </summary>
        public string Color { get; set; }
        /// <summary>
        /// 获取或设置称号是否为Rank
        /// </summary>
        public bool IsRank { get; set; }
        /// <summary>
        /// 获取称号的动态指令集
        /// </summary>
        public List<string[]>? DynamicCommand { get; private set; }

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="id">ID</param>
        /// <param name="name">名称</param>
        /// <param name="color">颜色</param>
        /// <param name="isRank">是否为Rank</param>
        /// <param name="dynamicCommandString">动态指令集</param>
        public PlayerTitle(uint id, string name, string color, bool isRank, string? dynamicCommandString = null)
        {
            Id = id;
            Name = name;
            Color = color;
            IsRank = isRank;
            SetDynamicCommand(dynamicCommandString);
        }

        /// <summary>
        /// 设置称号的动态指令集
        /// </summary>
        /// <param name="dynamicCommandString"></param>
        public void SetDynamicCommand(string? dynamicCommandString)
        {
            List<string[]>? dynamicCommands = null;
            if (!string.IsNullOrEmpty(dynamicCommandString))
            {
                dynamicCommands = [];
                foreach (string dCommand in dynamicCommandString!.Split(';'))
                {
                    dynamicCommands.Add(dCommand.Split(','));
                }
            }
            DynamicCommand = dynamicCommands;
        }

        /// <summary>
        /// 获取称号
        /// </summary>
        /// <param name="id">称号ID</param>
        /// <returns>获取的称号</returns>
        public static PlayerTitle? Get(uint id)
        {
            if (dictionary.TryGetValue(id, out PlayerTitle? title))
            {
                return title;
            }
            if (title != null) dictionary.Add(id, title);
            return title;
        }

        public static PlayerTitle? Load(uint Id)
        {
            string queryString = "select * from title_data where Id = @Id";
            try
            {
                using MySqlConnection connection = new(YongAnFramePlugin.Instance.ConnectionString);
                connection.Open();

                using MySqlCommand command = new(queryString, connection);
                command.Parameters.AddWithValue("@Id", Id);

                using MySqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new(Id, reader["Name"].ToString(), reader["Color"].ToString(), (byte)reader["Pro"] != 0, reader["DynamicCommand"].ToString());
                }
                return null;
            }
            catch (Exception text)
            {
                Log.Error("数据库查找异常 错误原因:" + text);
                return null;
            }
        }

        public bool Insert(string? dynamicCommand)
        {
            try
            {
                using MySqlConnection connection = new(YongAnFramePlugin.Instance.ConnectionString);
                connection.Open();
                using MySqlCommand cmd = new("insert into title_data set Id=@Id,Name=@Name,Color=@Color,Pro=@Pro,DynamicCommand=@DynamicCommand", connection);
                Id = (uint)cmd.LastInsertedId;
                cmd.Parameters.AddWithValue("Id", Id);
                cmd.Parameters.AddWithValue("Name", Name);
                cmd.Parameters.AddWithValue("Color", Color);
                cmd.Parameters.AddWithValue("Pro", IsRank ? 1 : 0);
                cmd.Parameters.AddWithValue("DynamicCommand", dynamicCommand);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception text)
            {
                Log.Error("数据库插入数据异常 错误原因:" + text);
                return false;
            }
        }
        public bool Update()
        {
            try
            {
                using MySqlConnection connection = new(YongAnFramePlugin.Instance.ConnectionString);
                connection.Open();
                using MySqlCommand cmd = new("update title_data set Name=@Name,Color=@Color,Pro=@Pro,DynamicCommand=@DynamicCommand where Id=@Id", connection);
                cmd.Parameters.AddWithValue("Id", (uint)cmd.LastInsertedId);
                cmd.Parameters.AddWithValue("Name", Name);
                cmd.Parameters.AddWithValue("Color", Color);
                cmd.Parameters.AddWithValue("Pro", IsRank ? 1 : 0);
                string? dynamicString = null;
                if (DynamicCommand is not null)
                {
                    foreach (var command in DynamicCommand)
                    {
                        dynamicString = string.Join(",", command) + ";";
                    }
                    dynamicString = dynamicString?.Substring(0, dynamicString.Length - 1);
                }

                cmd.Parameters.AddWithValue("DynamicCommand", dynamicString);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception text)
            {
                Log.Error("数据库数据更新异常 错误原因:" + text);
                return false;
            }
        }
    }
}
