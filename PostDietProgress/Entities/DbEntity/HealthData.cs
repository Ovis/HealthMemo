namespace PostDietProgress.Entities.DbEntity
{
    public class HealthData
    {
        public string Id { get; set; }

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

        public string Type { get; set; }
    }
}
