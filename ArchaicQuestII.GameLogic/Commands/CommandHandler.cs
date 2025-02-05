﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArchaicQuestII.GameLogic.Character;
using ArchaicQuestII.GameLogic.Core;
using ArchaicQuestII.GameLogic.Skill;
using ArchaicQuestII.GameLogic.Skill.Enum;
using ArchaicQuestII.GameLogic.Socials;
using ArchaicQuestII.GameLogic.World.Room;

namespace ArchaicQuestII.GameLogic.Commands
{
    /// <summary>
    /// This is to handle commands that can't fit in the switch case
    /// found in command.cs as a catch all this class will figure out what to 
    /// do next.
    /// This is to solve the issue of dynamic skills and socials
    /// It wasn't a problem for spells as they're preceded by c or cast 
    /// so was trivial to handle with the other commands.
    /// but when dealing with skills or socials with no prefix and are not hard coded
    /// it's a little more trickier 
    /// </summary>
    public class CommandHandler : ICommandHandler
    {
        private readonly ISocials _socials;
        private readonly ICache _cache;
        private readonly IWriteToClient _writeToClient;
        private readonly ISKill _Skill;
        public CommandHandler(ISocials social, ICache cache, IWriteToClient writeToClient, ISKill skill)
        {
            _socials = social;
            _cache = cache;
            _writeToClient = writeToClient;
            _Skill = skill;
        }
        public void HandleCommand(string key, string obj, string target, Player player, Room room)
        {

            // oddballs is shit name
            // but might be times where we need a command to trigger
            // a skill that does not match the name
            // here second matches on second attack
            // but I want second to allow selecting a second weapon
            var oddBalls = new List<Tuple<string, string>>()
            {
                new Tuple<string, string>("second", "dual wield"),
                new Tuple<string, string>("warcry", "war cry"),
            };

            var oddBallMatch = oddBalls.FirstOrDefault(x => x.Item1.StartsWith(key));

            if (oddBallMatch != null)
            {
                key = oddBallMatch.Item2;
            }


            //check player skill
            var foundSkill = _cache.GetAllSkills()
                .FirstOrDefault(x => x.Name.StartsWith(key, StringComparison.CurrentCultureIgnoreCase) && x.Type != SkillType.Passive);
            if (foundSkill != null)
            {
                _Skill.PerfromSkill(foundSkill, key, player, obj, room);
                return;
            }



            //check socials
            var social = _cache.GetSocials().Keys.FirstOrDefault(x => x.StartsWith(key));
            if (social != null)
            {
                var emoteTarget = key == obj ? "" : obj;
                _socials.EmoteSocial(player, room, _cache.GetSocials()[social], emoteTarget);
                return;
            }


            _writeToClient.WriteLine("That is not a command.", player.ConnectionId);
        }
    }
}
