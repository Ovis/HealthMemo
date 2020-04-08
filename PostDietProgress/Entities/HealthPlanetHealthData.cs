using System;
using System.Collections.Generic;
using System.Linq;

namespace PostDietProgress.Entities
{
    public class HealthPlanetHealthData
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

        /* 基礎代謝量(kcal) */
        public string BasalMetabolism { get; set; }

        /* 体内年齢(歳) */
        public string BodyAge { get; set; }

        /* 推定骨量(kg) */
        public string BoneQuantity { get; set; }

        public HealthPlanetHealthData() { }

        public HealthPlanetHealthData(string dateTime, Dictionary<string, string> dic)
        {
            DateTime = dateTime;

            foreach (var enumVal in dic.Select(item => (HealthPlanetHealthTagEnum)Enum.ToObject(typeof(HealthPlanetHealthTagEnum), int.Parse(item.Key))))
            {
                switch (enumVal)
                {
                    case HealthPlanetHealthTagEnum.WEIGHT:
                        Weight = dic[((int)HealthPlanetHealthTagEnum.WEIGHT).ToString()];
                        break;
                    case HealthPlanetHealthTagEnum.BODYFATPERF:
                        BodyFatPerf = dic[((int)HealthPlanetHealthTagEnum.BODYFATPERF).ToString()];
                        break;
                    case HealthPlanetHealthTagEnum.MUSCLEMASS:
                        MuscleMass = dic[((int)HealthPlanetHealthTagEnum.MUSCLEMASS).ToString()];
                        break;
                    case HealthPlanetHealthTagEnum.MUSCLESCORE:
                        MuscleScore = dic[((int)HealthPlanetHealthTagEnum.MUSCLESCORE).ToString()];
                        break;
                    case HealthPlanetHealthTagEnum.VISCERALFATLEVEL2:
                        VisceralFatLevel2 = dic[((int)HealthPlanetHealthTagEnum.VISCERALFATLEVEL2).ToString()];
                        break;
                    case HealthPlanetHealthTagEnum.VISCERALFATLEVEL:
                        VisceralFatLevel = dic[((int)HealthPlanetHealthTagEnum.VISCERALFATLEVEL).ToString()];
                        break;
                    case HealthPlanetHealthTagEnum.BASALMETABOLISM:
                        BasalMetabolism = dic[((int)HealthPlanetHealthTagEnum.BASALMETABOLISM).ToString()];
                        break;
                    case HealthPlanetHealthTagEnum.BODYAGE:
                        BodyAge = dic[((int)HealthPlanetHealthTagEnum.BODYAGE).ToString()];
                        break;
                    case HealthPlanetHealthTagEnum.BONEQUANTITY:
                        BoneQuantity = dic[((int)HealthPlanetHealthTagEnum.BONEQUANTITY).ToString()];
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
