namespace RaidTrackerParser.Data
{
    public class ItemTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int WowId { get; set; }
        public int Quality { get; set; }

        public ItemTO(string name, int wowId)
        {
            Name = name;
            WowId = wowId;
        }

        public ItemTO()
        {
        }

        public override string ToString()
        {
            return $"{nameof(ID)}: {ID}, {nameof(Name)}: {Name}, {nameof(WowId)}: {WowId}, {nameof(Quality)}: {Quality}";
        }
    }
}