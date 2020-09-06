namespace RaidTrackerParser.Data
{
    public class ItemTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int WOWID { get; set; }

        public ItemTO(string name, int wowid)
        {
            Name = name;
            WOWID = wowid;
        }

        public ItemTO()
        {
        }
    }
}