using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace FineWork.Colla.Models
{
    public static class AlarmTimeConverter
    {
        public static int? Converter(string alarmTime)
        {
            if (string.IsNullOrEmpty(alarmTime)) return null;
            var converDict = new Dictionary<string, int>()
            {
                ["分钟"] = 1,
                ["小时"] = 60,
                ["天"] = 60 * 24,
                ["周"] = 60 * 24 * 7
            };
            var match = Regex.Match(alarmTime, @"([\u4e00-\u9fa5]+)(\d+)([\u4e00-\u9fa5]+)");
            if (match.Success && match.Groups.Count > 1)
                return (int.Parse(match.Groups[2].Value)) * converDict[match.Groups[3].Value];
            return 0;
        }

        public static string BackConverter(int alarmTimeMins)
        {
            if (alarmTimeMins == 0) return "事情开始时";
            var format = "开始前{0}{1}";
            var hour = alarmTimeMins / 60;
            if (hour < 1)
                return string.Format(format, alarmTimeMins, "分钟");
            if (hour >= 1 && hour < 24)
                return string.Format(format, hour, "小时");
            if (hour >= 24 && hour < 24 * 7)
                return string.Format(format, hour / 24, "天");

            return string.Format(format, hour / (24 * 7), "周");
        }
    }

    public class CreateAnncAlarmModel
    {
        public  Guid? AnncId { get; set; }

        public DateTime? Time { get; set; }

        [MaxLength(100, ErrorMessage = "闹铃的值不能超过100字符")]
        public  string Bell { get; set; } 

        public bool IsEnabled { get; set; }

        public  string BeforeStart { get; set; }

        public List<Tuple<AnncRoles,Guid>> Recs { get; set; }

        public  int? BeforeMins {
            get {return  AlarmTimeConverter.Converter(BeforeStart); } 
        }
    }

    public class UpdateAnncAlarmModel
    {
        public Guid? AnncAlarmId { get; set; } 

        public DateTime? Time { get; set; }

        [MaxLength(100,ErrorMessage = "闹铃的值不能超过100字符")]
        public string Bell { get; set; } 

        public bool IsEnabled { get; set; }

        public string BeforeStart { get; set; }

        public int? BeforeMins
        {
            
                get { return AlarmTimeConverter.Converter(BeforeStart); }
           
        }

        public List<Tuple<AnncRoles, Guid>> Recs { get; set; }
    }
}