using System;
using System.Collections.Generic;

namespace RaidTrackerParser.Data
{
    public class RaidTO
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<LootTO> Loot { get; set; }
        public List<PlayerTO> Players { get; set; }

        public RaidTO(string name, DateTime startDate, DateTime endDate)
        {
            Name = name;
            StartDate = startDate;
            EndDate = endDate;
        }

        public RaidTO()
        {
        }
    }
}