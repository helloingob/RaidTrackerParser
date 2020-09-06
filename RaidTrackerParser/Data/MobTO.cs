namespace RaidTrackerParser.Data
{
    public class MobTO
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public MobTO(string name)
        {
            Name = name;
        }

        public MobTO()
        {
        }

        public override string ToString()
        {
            return $"{nameof(ID)}: {ID}, {nameof(Name)}: {Name}";
        }
    }
}