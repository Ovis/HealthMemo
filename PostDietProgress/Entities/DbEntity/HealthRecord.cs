using System;

namespace PostDietProgress.Entities.DbEntity
{
    public class HealthRecord
    {
        public string Id { get; set; }

        //測定日
        public DateTime? AssayDate { get; set; }

        //身長
        public double? Height { get; set; }

        // 体重 (kg)
        public double? Weight { get; set; }

        //体脂肪率(%)
        public double? BodyFatPerf { get; set; }

        //筋肉量(kg)
        public double? MuscleMass { get; set; }

        //筋肉スコア
        public string MuscleScore { get; set; }

        //内臓脂肪レベル2(小数点有り、手入力含まず)
        public double? VisceralFatLevel2 { get; set; }

        //内臓脂肪レベル(小数点無し、手入力含む)
        public long? VisceralFatLevel { get; set; }

        //基礎代謝量(kCal)
        public double? BasalMetabolism { get; set; }

        //体内年齢(歳) 
        public string BodyAge { get; set; }

        //推定骨量(kg)
        public double? BoneQuantity { get; set; }

        public string Type { get; set; }
    }
}
