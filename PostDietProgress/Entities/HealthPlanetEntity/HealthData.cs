using System;
using System.Collections.Generic;
using System.Linq;

namespace PostDietProgress.Entities.HealthPlanetEntity
{
    public class HealthData
    {

        public string DateTime { get; set; }

        /* 体重 (kg) */
        public string Weight { get; set; }

        /* 体脂肪率(%) */
        public string BodyFatPerf { get; set; }

        /* 筋肉量(kg) */
        public string MuscleMass { get; set; }

        /* 筋肉スコア */
        public string MuscleScore { get; set; }

        /* 内臓脂肪レベル2(小数点有り、手入力含まず) */
        public string VisceralFatLevel2 { get; set; }

        /* 内臓脂肪レベル(小数点無し、手入力含む) */
        public string VisceralFatLevel { get; set; }

        /* 基礎代謝量(kCal) */
        public string BasalMetabolism { get; set; }

        /* 体内年齢(歳) */
        public string BodyAge { get; set; }

        /* 推定骨量(kg) */
        public string BoneQuantity { get; set; }

        public HealthData() { }

        public HealthData(string dateTime, Dictionary<string, string> dic)
        {
            DateTime = dateTime;

            foreach (var enumVal in dic.Select(item => (HealthTagEnum)Enum.ToObject(typeof(HealthTagEnum), int.Parse(item.Key))))
            {
                switch (enumVal)
                {
                    case HealthTagEnum.WEIGHT:
                        Weight = dic[((int)HealthTagEnum.WEIGHT).ToString()];
                        break;
                    case HealthTagEnum.BODYFATPERF:
                        BodyFatPerf = dic[((int)HealthTagEnum.BODYFATPERF).ToString()];
                        break;
                    case HealthTagEnum.MUSCLEMASS:
                        MuscleMass = dic[((int)HealthTagEnum.MUSCLEMASS).ToString()];
                        break;
                    case HealthTagEnum.MUSCLESCORE:
                        MuscleScore = dic[((int)HealthTagEnum.MUSCLESCORE).ToString()];
                        break;
                    case HealthTagEnum.VISCERALFATLEVEL2:
                        VisceralFatLevel2 = dic[((int)HealthTagEnum.VISCERALFATLEVEL2).ToString()];
                        break;
                    case HealthTagEnum.VISCERALFATLEVEL:
                        VisceralFatLevel = dic[((int)HealthTagEnum.VISCERALFATLEVEL).ToString()];
                        break;
                    case HealthTagEnum.BASALMETABOLISM:
                        BasalMetabolism = dic[((int)HealthTagEnum.BASALMETABOLISM).ToString()];
                        break;
                    case HealthTagEnum.BODYAGE:
                        BodyAge = dic[((int)HealthTagEnum.BODYAGE).ToString()];
                        break;
                    case HealthTagEnum.BONEQUANTITY:
                        BoneQuantity = dic[((int)HealthTagEnum.BONEQUANTITY).ToString()];
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
