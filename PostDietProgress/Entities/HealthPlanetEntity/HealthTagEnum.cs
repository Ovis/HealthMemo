namespace PostDietProgress.Entities.HealthPlanetEntity
{
    public enum HealthTagEnum
    {
        WEIGHT = 6021, /* 体重 (kg) */
        BODYFATPERF = 6022, /* 体脂肪率(%) */
        MUSCLEMASS = 6023, /* 筋肉量(kg) */
        MUSCLESCORE = 6024, /* 筋肉スコア */
        VISCERALFATLEVEL2 = 6025, /* 内臓脂肪レベル2(小数点有り、手入力含まず) */
        VISCERALFATLEVEL = 6026, /* 内臓脂肪レベル(小数点無し、手入力含む) */
        BASALMETABOLISM = 6027, /* 基礎代謝量(kcal) */
        BODYAGE = 6028, /* 体内年齢(歳) */
        BONEQUANTITY = 6029 /* 推定骨量(kg) */
    }
}
