using System;

namespace RaidTrackerParser.Data
{
    public class LootTO
    {
        public int ID { get; set; }
        public DateTime Date { get; set; }
        public ItemTO Item { get; set; }
        public PlayerTO Player { get; set; }
        public MobTO Mob { get; set; }

        public LootTO(DateTime date, ItemTO item, PlayerTO player, MobTO mob)
        {
            Date = date;
            Item = item;
            Player = player;
            Mob = mob;
        }

        public override string ToString()
        {
            return $"{nameof(Date)}: {Date}, {nameof(Item)}: {Item}, {nameof(Player)}: {Player}, {nameof(Mob)}: {Mob}";
        }
    }
}