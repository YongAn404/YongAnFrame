﻿using YongAnFrame.Players;

namespace YongAnFrame.Roles.Interfaces
{
    public interface ISkillActiveEnd : ISkill
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="yPlayer"></param>
        /// <returns>方法的音乐文件名称</returns>
        string ActiveEnd(FramePlayer yPlayer,byte id);
    }
}
