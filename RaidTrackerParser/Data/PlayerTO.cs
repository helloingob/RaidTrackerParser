using System;

namespace RaidTrackerParser.Data
{
    public class PlayerTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Guild { get; set; }
        public string PlayerClass { get; set; }
        public string Race { get; set; }
        public int Level { get; set; }
        public DateTime Joined { get; set; }
        public DateTime Left { get; set; }

        public PlayerTO(string name, string guild, string playerClass, string race, int level, DateTime joined, DateTime left)
        {
            Name = name;
            Guild = guild;
            PlayerClass = playerClass;
            Race = race;
            Level = level;
            Joined = joined;
            Left = left;
        }

        public PlayerTO()
        {
        }

        public override string ToString()
        {
            return $"{nameof(Name)}: {Name}, {nameof(Guild)}: {Guild}, {nameof(PlayerClass)}: {PlayerClass}, {nameof(Race)}: {Race}, {nameof(Level)}: {Level}, {nameof(Joined)}: {Joined}, {nameof(Left)}: {Left}";
        }
    }
}