namespace HealthMemo.Entities.DbEntity
{
    public class Goal
    {
        public string Id { get; set; } = nameof(Goal);
        public double OriginalWeight { get; set; }

        public double GoalWeight { get; set; }

        public string PartitionKey { get; set; } = "Setting";
    }
}
