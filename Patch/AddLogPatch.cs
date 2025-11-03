using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using YongAnFrame.Features;
using static ServerLogs;

namespace YongAnFrame.Patch
{
//    /// <summary>
//    /// 在<seealso cref="ServerLogs"/>添加<seealso cref="AddLog"/>的补丁
//    /// </summary>
//    [HarmonyPatch(typeof(ServerLogs), nameof(AddLog))]
//    public static class AddLogPatch
//    {
//#pragma warning disable IDE0060 // 删除未使用的参数
//        private static void Prefix(Modules module, string msg, ServerLogType type, bool init = false)
//#pragma warning restore IDE0060 // 删除未使用的参数
//        {
//            SaveLog(q);
//        }

//        private static readonly Queue<InfoData> logQueue = new();
//        private static readonly Task logTask = new(async () =>
//        {
//            while (true)
//            {
//                while (logQueue.Count != 0)
//                {
//                    InfoData infoData = logQueue.Dequeue();
//                    using StreamWriter writer = new($"{PathManager.Log}/{DateTime.Now:yyyy-MM-dd}.log", true, Encoding.UTF8);
//                    writer.WriteLine(infoData);
//                }
//                await Task.Delay(1000);
//            }
//        });

//        /// <summary>
//        /// 启动日志任务
//        /// </summary>
//        public static void StartTask()
//        {
//            if (logTask.Status == TaskStatus.Created)
//            {
//                logTask.Start();
//            }
//        }

//        private static void SaveLog(string log, StackTrace trace)
//        {
//            logQueue.Enqueue(new InfoData(log, trace));
//        }
//    }
}
