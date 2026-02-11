using System.Collections.Generic;
using UnityEngine;

namespace FishONU.Player
{
    public static class IdentifierHelper
    {
        private static readonly List<string> Adjectives = new List<string>
        {
            "狂笑的",
            "愤怒的",
            "机智的",
            "勇敢的",
            "狡猾的",
            "聪明的",
            "无情的",
            "冷酷的",
            "狡诈的",
            "狡猾的",
            "复仇的",
            "极度空虚的",
            "刚睡醒的",
            "没吃翻的",
            "停止思考的",
            "忧郁的",
            "疯狂的",
            "优雅的",
            "神秘的",
            "傲慢的",
            "温柔的",
            "暴躁的",
            "天真的",
            "老练的",
            "幽默的",
            "沉默的",
            "活泼的",
            "懒散的",
            "勤奋的",
            "贪婪的",
            "慷慨的"
        };

        private static readonly List<string> Nouns = new List<string>
        {
            "哈基米",
            "猫",
            "狗",
            "鱼",
            "蛇",
            "熊",
            "狼",
            "虎",
            "鹰",
            "鹦鹉",
            "猫头鹰",
            "兔子",
            "狐狸",
            "熊猫",
            "猴子",
            "鹿",
            "喜鹊",
            "水猴子",
            "脆皮鸡",
            "电磁炉",
            "咸鱼",
            "鲸鱼",
            "海豚",
            "章鱼",
            "企鹅",
            "考拉",
            "松鼠",
            "刺猬",
            "浣熊",
            "袋鼠",
            "河马",
            "长颈鹿",
            "犀牛",
            "骆驼",
            "孔雀",
            "天鹅",
            "蜘蛛",
            "蝴蝶"
        };

        private static readonly List<string> Verbs = new List<string>
        {
            "将写散文",
            "将写诗歌",
            "将写小说",
            "将写剧本",
            "会画画",
            "会唱歌",
            "会跳舞",
            "会Rap",
            "会打篮球",
            "会踢足球",
            "会打乒乓球",
            "会打羽毛球",
            "会打网球",
            "在写代码",
            "在发呆",
            "在学习",
            "在挂科",
            "在蹦迪",
            "在燃放核弹",
            "在黑入系统",
            "在仰卧起坐",
            "在玩游戏",
            "在睡觉",
            "将写代码",
            "会做游戏",
            "会做菜",
            "会弹吉他",
            "会弹钢琴",
            "会下棋",
            "会武术",
            "会游泳",
            "会滑雪",
            "会冲浪",
            "会摄影",
            "会设计",
            "会投资",
            "会种花",
            "会做饭",
            "会翻译",
            "会魔术"
        };

        public static string RandomIdentifier()
        {
            string adj = Adjectives[Random.Range(0, Adjectives.Count)];
            string color = Nouns[Random.Range(0, Nouns.Count)];
            string noun = Verbs[Random.Range(0, Verbs.Count)];

            // 组合并返回
            return $"{adj}{color}{noun}";
        }
    }
}