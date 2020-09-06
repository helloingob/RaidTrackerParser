using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using RaidTrackerParser.Data;

namespace RaidTrackerParser
{
    public class RaidTrackerParser
    {
        private const string DateFormat = "dd/MM/yy HH:mm:ss";
        private readonly CultureInfo _provider = CultureInfo.InvariantCulture;

        private List<PlayerTO> _players = new List<PlayerTO>();
        private List<LootTO> _loot = new List<LootTO>();

        public RaidTO ParseLog(string filename)
        {
            var raid = new RaidTO();
            var lines = File.ReadAllLines(filename);
            var playerParse = false;
            var lootParse = false;

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (i == 0)
                {
                    raid = ParseRaidInfo(line);
                }

                if (string.IsNullOrEmpty(line))
                {
                    playerParse = false;
                    lootParse = false;
                }

                if (playerParse)
                {
                    var player = ParsePlayerInfo(line);
                    _players.Add(player);
                }

                if (lootParse)
                {
                    var loot = ParseLootInfo(line);
                    _loot.Add(loot);
                }

                if (line.Contains("Players:"))
                {
                    playerParse = true;
                }

                if (line.Contains("Loot:"))
                {
                    lootParse = true;
                }
            }

            raid.Loot = _loot;
            raid.Players = _players;

            return raid;
        }

        //1 Ayoo 09/05/20 19:37:39 - 09/05/20 20:04:45, Scourge, Orden der Horde, Rogue, 60
        //2 Xlenqt 09/05/20 18:12:49 - 09/05/20 18:17:46, 59, Scourge, Rogue
        private PlayerTO ParsePlayerInfo(string line)
        {
            var name = line.Split(' ')[1];

            var playerInfoParts = line.Split(',');

            var startEndDates = playerInfoParts[0].Trim();

            var leftDateString = startEndDates.Substring(startEndDates.Length - DateFormat.Length);
            var leftDate = DateTime.ParseExact(leftDateString, DateFormat, _provider);
            var joinedDateString = startEndDates.Substring(startEndDates.Length - ((2 * DateFormat.Length) + " - ".Length), DateFormat.Length);
            var joinedDate = DateTime.ParseExact(joinedDateString, DateFormat, _provider);

            string race;
            string guild = null;
            string playerClass;
            int level;

            //Too far away; No race & guild (#2)
            if (playerInfoParts.Length == 4)
            {
                level = Convert.ToInt32(playerInfoParts[1].Trim());
                race = playerInfoParts[2].Trim();
                playerClass = playerInfoParts[3].Trim();
            }
            //Normal format  (#1)
            else
            {
                race = playerInfoParts[1].Trim();
                guild = playerInfoParts[2].Trim();
                playerClass = playerInfoParts[3].Trim();
                level = Convert.ToInt32(playerInfoParts[4].Trim());
            }

            return new PlayerTO(name, guild, playerClass, race, level, joinedDate, leftDate);
        }

        //1 Thorium Lockbox (5759) at 09/05/20 19:52:43 from Trash mob to Phiora
        private LootTO ParseLootInfo(string line)
        {
            var dateString = line.Substring(line.IndexOf(" at ", StringComparison.Ordinal) + " at ".Length, DateFormat.Length);
            var date = DateTime.ParseExact(dateString, DateFormat, _provider);

            var foundPlayerName = line.Substring(line.LastIndexOf(' ') + " ".Length);
            var foundPlayer = _players.FirstOrDefault(player => string.Equals(player.Name, foundPlayerName));

            var mobName = line.Substring(line.IndexOf(" from ", StringComparison.Ordinal) + " from ".Length);
            mobName = mobName.Substring(0, mobName.IndexOf(" to ", StringComparison.Ordinal));
            
            var WOWIDString = line.Substring(line.IndexOf("(", StringComparison.Ordinal) + "(".Length);
            WOWIDString = WOWIDString.Substring(0, WOWIDString.IndexOf(")", StringComparison.Ordinal));
            var WOWID = Convert.ToInt32(WOWIDString);

            var ItemName = line.Substring(0, line.IndexOf("(", StringComparison.Ordinal) - "(".Length);
            ItemName = ItemName.Substring(ItemName.IndexOf(" ", StringComparison.Ordinal) + " ".Length);

            var item = new ItemTO(ItemName, WOWID);

            return new LootTO(date, item, foundPlayer, new MobTO(mobName));
        }

        //Onyxia's Lair raid 09/05/20 19:37:38 - 09/05/20 20:04:45
        private RaidTO ParseRaidInfo(string line)
        {
            var endDateString = line.Substring(line.Length - DateFormat.Length);
            var endDate = DateTime.ParseExact(endDateString, DateFormat, _provider);
            var startDateString = line.Substring(line.Length - ((2 * DateFormat.Length) + " - ".Length), DateFormat.Length);
            var startDate = DateTime.ParseExact(startDateString, DateFormat, _provider);
            var name = line.Substring(0, line.Length - ((2 * DateFormat.Length) + " - ".Length + " ".Length));
            return new RaidTO(name, startDate, endDate);
        }
    }
}