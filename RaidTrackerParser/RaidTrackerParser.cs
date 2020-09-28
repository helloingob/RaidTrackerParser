using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using RaidTrackerParser.Data;

namespace RaidTrackerParser
{
    public class RaidTrackerParser
    {
        private const string DateFormat = "MM/dd/yy HH:mm:ss";
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
                if (i == 2)
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
                    if (player != null) _players.Add(player);
                }

                if (lootParse)
                {
                    var loot = ParseLootInfo(line);
                    if (loot != null) _loot.Add(loot);
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

        private PlayerTO ParsePlayerInfo(string line)
        {
            if (line.Equals("Index,Name,Join,Leave,Race,Class,Level,Guild,Note")) return null;
            var playerInformation = line.Split(',');
            try
            {
                var name = playerInformation[1].Replace("\"", "");
                var leftDateString = playerInformation[2];
                var leftDate = DateTime.ParseExact(leftDateString, DateFormat, _provider);
                var joinedDateString = playerInformation[3];
                var joinedDate = DateTime.ParseExact(joinedDateString, DateFormat, _provider);
                var race = playerInformation[4].Replace("\"", "");
                var playerClass = playerInformation[5].Replace("\"", "");
                var level = Convert.ToInt32(playerInformation[6]);
                var guild = playerInformation[7].Replace("\"", "");
                return new PlayerTO(name, guild, playerClass, race, level, joinedDate, leftDate);
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't parse player format: '" + line +"'");
            }

            return null;
        }

        private LootTO ParseLootInfo(string line)
        {
            if (line.Equals("Index,Name,ID,Time,Mob,Player,Cost,Note")) return null;
            var csvParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))");
            var lootInformation = csvParser.Split(line);
            try
            {
                var itemName = lootInformation[1].Replace("\"", "");
                var wowId = Convert.ToInt32(lootInformation[2]);
                var dateString = lootInformation[3];
                var date = DateTime.ParseExact(dateString, DateFormat, _provider);
                var mobName = lootInformation[4].Replace("\"", "");
                var foundPlayerName = lootInformation[5].Replace("\"", "");
                var foundPlayer = _players.FirstOrDefault(player => string.Equals(player.Name, foundPlayerName));

                var item = new ItemTO(itemName, wowId);

                return new LootTO(date, item, foundPlayer, new MobTO(mobName));
            }
            catch (Exception)
            {
                Console.WriteLine("Couldn't parse loot format: '" + line + "'");
            }

            return null;
        }

        private RaidTO ParseRaidInfo(string line)
        {
            var raidInformation = line.Split(',');

            var name = raidInformation[0].Replace("\"", "");
            var startDateString = raidInformation[2];
            var startDate = DateTime.ParseExact(startDateString, DateFormat, _provider);
            var endDateString = raidInformation[3];
            var endDate = DateTime.ParseExact(endDateString, DateFormat, _provider);
            
            return new RaidTO(name, startDate, endDate);
        }
    }
}