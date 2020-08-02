﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArchaicQuestII.GameLogic.Character;
using ArchaicQuestII.GameLogic.World.Room;


namespace ArchaicQuestII.GameLogic.Core
{

    /// <summary>
    /// Refactor me
    /// TODO: refactor cache
    /// </summary>
    public class Cache : ICache
    {
        private readonly ConcurrentDictionary<string, Player> _playerCache = new ConcurrentDictionary<string, Player>();
        private readonly ConcurrentDictionary<int, Room> _roomCache = new ConcurrentDictionary<int, Room>();
        private readonly ConcurrentDictionary<int, Skill.Model.Skill> _skillCache = new ConcurrentDictionary<int, Skill.Model.Skill>();
        private Config _configCache = new Config();

        #region PlayerCache

        public bool AddPlayer(string id, Player player)
        {
            return _playerCache.TryAdd(id, player);
        }

        public Player GetPlayer(string id)
        {
            _playerCache.TryGetValue(id, out Player player);

            return player;
        }

        /// <summary>
        /// Only for the main loop
        /// </summary>
        /// <returns></returns>
        public ConcurrentDictionary<string, Player> GetPlayerCache()
        {
            return _playerCache;
        }

        public Player PlayerAlreadyExists(Guid id)
        {
            return _playerCache.Values.FirstOrDefault(x => x.Id.Equals(id));
        }

        #endregion

        #region RoomCache


        public bool AddRoom(int id, Room room)
        {
            return _roomCache.TryAdd(id, room);
        }

        public Room GetRoom(int id)
        {
            _roomCache.TryGetValue(id == 0 ? 1 : id, out Room room);

            return room;
        }
        /// <summary>
        /// This is not as efficient as get by id
        /// Won't be an issue for a long time so can be revisted.
        /// to fix, need a field in the admin exit modal for roomId
        /// of the room you want to move to from your current position
        /// </summary>
        /// <param name="id"></param>
        /// <param name="coords"></param>
        /// <returns></returns>
        public Room GetRoom(int id, Coordinates coords)
        {
            var room = _roomCache.Values.FirstOrDefault(x => x.AreaId == id && x.Coords.X == coords.X && x.Coords.Y == coords.Y && x.Coords.Z == coords.Z);

            return room;
        }

        public bool UpdateRoom(int id, Room room, Player player)
        {
            var existingRoom = room;
            var newRoom = room;
            newRoom.Players.Add(player);


            return _roomCache.TryUpdate(id, existingRoom, newRoom);
        }

        #endregion

        #region SkillCache


        public bool AddSkill(int id, Skill.Model.Skill skill)
        {
            return _skillCache.TryAdd(id, skill);
        }

        public Skill.Model.Skill GetSkill(int id)
        {
            _skillCache.TryGetValue(id == 0 ? 1 : id, out var skill);

            return skill;
        }
         
 
        #endregion

        public void SetConfig(Config config)
        {
            _configCache = config;
        }

        public Config GetConfig()
        {
            return _configCache;
        }

        public void ClearRoomCache()
        {
            _roomCache.Clear();

        }


    }
}
