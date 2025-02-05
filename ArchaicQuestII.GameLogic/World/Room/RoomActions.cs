﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Encodings.Web;
using ArchaicQuestII.Core.World;
using ArchaicQuestII.GameLogic.Character;
using ArchaicQuestII.GameLogic.Character.Gain;
using ArchaicQuestII.GameLogic.Character.Status;
using ArchaicQuestII.GameLogic.Combat;
using ArchaicQuestII.GameLogic.Core;
using ArchaicQuestII.GameLogic.Item;
using Newtonsoft.Json;

namespace ArchaicQuestII.GameLogic.World.Room
{
    public class RoomActions : IRoomActions
    {

        private readonly IWriteToClient _writeToClient;
        private readonly ITime _time;
        private readonly ICache _cache;
        private readonly IDice _dice;
        private readonly IGain _gain;
        private readonly IFormulas _formulas;
        public RoomActions(IWriteToClient writeToClient, ITime time, ICache cache, IDice dice, IGain gain, IFormulas formulas)
        {
            _writeToClient = writeToClient;
            _time = time;
            _cache = cache;
            _dice = dice;
            _gain = gain;
            _formulas = formulas;
        }
        /// <summary>
        /// Displays current room 
        /// </summary>
        public void Look(string target, Room room, Player player)
        {

            if (player.Status == CharacterStatus.Status.Sleeping)
            {
                _writeToClient.WriteLine("You can't do that while asleep.", player.ConnectionId);
                return;
            }

            if (player.Affects.Blind)
            {
                _writeToClient.WriteLine("<p>You are blind and can't see a thing!</p>", player.ConnectionId);
                return;
            }

            if (!string.IsNullOrEmpty(target) && !target.Equals("look", StringComparison.CurrentCultureIgnoreCase) && !string.IsNullOrEmpty(target) && !target.Equals("l", StringComparison.CurrentCultureIgnoreCase))
            {
                LookObject(target, room, player);
                return;
            }

            var showVerboseExits = player.Config.VerboseExits;
            string exits = FindValidExits(room, showVerboseExits);

            var items = DisplayItems(room, player);
            var mobs = DisplayMobs(room, player);
            var players = DisplayPlayers(room, player);

            var roomDesc = new StringBuilder();
            var isDark = RoomIsDark(room, player);

            roomDesc
                .Append($"<p class=\"room-title {(isDark ? "room-dark" : "")}\">{room.Title}<br /></p>")
                .Append($"<p class=\"room-description  {(isDark ? "room-dark" : "")}\">{room.Description}</p>");

            if (!showVerboseExits)
            {
                roomDesc.Append(
                    $"<p class=\"room-exit  {(isDark ? "room-dark" : "")}\"> <span class=\"room-exits\">[</span>Exits: <span class=\"room-exits\">{exits}</span><span class=\"room-exits\">]</span></p>");
            }
            else
            {
                roomDesc.Append(
                    $"<div class=\" {(isDark ? "room-dark" : "")}\">Obvious exits: <table class=\"room-exits\"><tbody>{exits}</tbody></table></div>");
            }

            roomDesc.Append($"<p  class=\" {(isDark ? "room-dark" : "")}\">{items}</p>")
                .Append($"<p  class=\"{(isDark ? "room-dark" : "")}\">{mobs}</p>")
                .Append($"<p  class=\"  {(isDark ? "room-dark" : "")}\">{players}</p>");


            _writeToClient.WriteLine(roomDesc.ToString(), player.ConnectionId);

        }

        public void LookInContainer(string target, Room room, Player player)
        {

            //check room, then check player if no match
            if (player.Status == CharacterStatus.Status.Sleeping)
            {
                _writeToClient.WriteLine("You can't do that while asleep.", player.ConnectionId);
                return;
            }

            var nthTarget = Helpers.findNth(target);
            var container = Helpers.findRoomObject(nthTarget, room) ?? Helpers.findObjectInInventory(nthTarget, player);

            if (container != null && (container.ItemType != Item.Item.ItemTypes.Container && container.ItemType != Item.Item.ItemTypes.Cooking))
            {
                if (container.ItemType == Item.Item.ItemTypes.Portal)
                {
                    LookInPortal(container, room, player);
                    return;
                }

                _writeToClient.WriteLine($"<p>{container.Name} is not a container", player.ConnectionId);
                return;
            }

            if (container == null)
            {
                _writeToClient.WriteLine("<p>You don't see that here.", player.ConnectionId);
                return;
            }

            if (container.Container.IsOpen == false)
            {
                _writeToClient.WriteLine("<p>You need to open it first.", player.ConnectionId);
                return;
            }

            _writeToClient.WriteLine($"<p>{container.Name} contains:</p>", player.ConnectionId);
            if (container.Container.Items.Count == 0)
            {
                _writeToClient.WriteLine($"<p>Nothing.</p>", player.ConnectionId);
            }

            var isDark = RoomIsDark(room, player);
            foreach (var obj in container.Container.Items.List(false))
            {
                _writeToClient.WriteLine($"<p class='item {(isDark ? "room-dark" : "")}'>{obj.Name}</p>", player.ConnectionId);
            }



            foreach (var pc in room.Players)
            {
                if (pc.Name == player.Name)
                {
                    continue;
                }

                _writeToClient.WriteLine($"<p>{player.Name} looks inside {container.Name.ToLower()}.</p>", pc.ConnectionId);
            }
        }

        public void LookInPortal(Item.Item portal, Room room, Player player)
        {
            var getPortalLocation = _cache.GetRoom(portal.Portal.Destination);

            if (getPortalLocation == null)
            {
                //Log error
                _writeToClient.WriteLine("<p>The dark abyss, I wouldn't enter if I were you.</p>", player.ConnectionId);
                return;
            }

            Look("", getPortalLocation, player);
        }

        public void LookObject(string target, Room room, Player player)
        {
            if (player.Status == CharacterStatus.Status.Sleeping)
            {
                _writeToClient.WriteLine("You can't do that while asleep.", player.ConnectionId);
                return;
            }
            var isDark = RoomIsDark(room, player);
            var nthTarget = Helpers.findNth(target);

            var item = Helpers.findRoomObject(nthTarget, room) ?? Helpers.findObjectInInventory(nthTarget, player);
            var character = Helpers.FindMob(nthTarget, room) ?? Helpers.FindPlayer(nthTarget, room);


            RoomObject roomObjects = null;
            if (room.RoomObjects.Count >= 1 && room.RoomObjects[0].Name != null)
            {
                roomObjects =
                   room.RoomObjects.FirstOrDefault(x =>
                       x.Name.Contains(target, StringComparison.CurrentCultureIgnoreCase));
            }

            if (target.Equals("self", StringComparison.CurrentCultureIgnoreCase))
            {
                character = player;
            }


            if (item == null && character == null && roomObjects == null)
            {
                _writeToClient.WriteLine("<p>You don't see that here.", player.ConnectionId);
                return;
            }

            if (item != null && character == null)
            {
                _writeToClient.WriteLine($"<p  class='{(isDark ? "room-dark" : "")}'>{item.Description.Look}", player.ConnectionId);

                // display item stats via lore
                var hasLore = Helpers.FindSkill("lore", player);

                if (hasLore != null)
                {
                    var success = LoreSuccess(hasLore.Proficiency ?? 0);

                    if (success)
                    {
                        DoLore(item, player);
                    }
                    else
                    {
                        _gain.GainSkillExperience(player, hasLore.Level * 100, hasLore, _dice.Roll(1, 1, 5));
                    }
                }

                if (item.Container != null && !item.Container.CanOpen && item.Container.Items.Any())
                {
                    _writeToClient.WriteLine($"<p  class='{(isDark ? "room-dark" : "")}'>{item.Name} contains:", player.ConnectionId);

                    var listOfContainerItems = new StringBuilder();
                    foreach (var containerItem in item.Container.Items.List())
                    {
                        listOfContainerItems.Append($"<p class='{(isDark ? "room-dark" : "")}'>{containerItem.Name.Replace(" lies here.", "")}</p>");
                    }

                    _writeToClient.WriteLine(listOfContainerItems.ToString(), player.ConnectionId);

                }

                foreach (var pc in room.Players)
                {
                    if (pc.Name == player.Name)
                    {
                        continue;
                    }

                    _writeToClient.WriteLine($"<p>{player.Name} looks at {item.Name.ToLower()}.</p>", pc.ConnectionId);

                }
                return;

            }
            //for player?
            if (roomObjects != null && character == null)
            {

                _writeToClient.WriteLine($"<p class='{(isDark ? "room-dark" : "")}'>{roomObjects.Look}", player.ConnectionId);

                foreach (var pc in room.Players)
                {
                    if (pc.Name == player.Name)
                    {
                        continue;
                    }

                    _writeToClient.WriteLine($"<p>{player.Name} looks at {roomObjects.Name.ToLower()}.</p>", pc.ConnectionId);
                }

                return;
            }

            if (character == null)
            {
                _writeToClient.WriteLine("<p>You don't see them here.", player.ConnectionId);
                return;
            }



            var sb = new StringBuilder();
            if (character.ConnectionId != "mob")
            {
                sb.Append(
                    $"<table class='char-look'><tr><td><span class='cell-title'>Eyes:</span> {character.Eyes}</td><td><span class='cell-title'>Hair:</span> {character.HairColour}</td></tr>");
                sb.Append(
                    $"<tr><td><span class='cell-title'>Skin:</span> {character.Skin}</td><td><span class='cell-title'>Hair Length:</span> {character.HairLength}</td></tr>");
                sb.Append(
                    $"<tr><td><span class='cell-title'>Build:</span> {character.Build}</td><td><span class='cell-title'>Hair Texture:</span> {character.HairTexture}</td></tr>");
                sb.Append(
                    $"<tr><td><span class='cell-title'>Face:</span> {character.Face}</td><td><span class='cell-title'>Hair Facial:</span> {character.FacialHair}</td></tr><table>");
            }


            var status = Enum.GetName(typeof(CharacterStatus.Status), player.Status);

            var statusText = String.Empty;

            if (status.Equals("fighting", StringComparison.CurrentCultureIgnoreCase))
            {
                statusText = $"fighting {(character.Target.Equals(player.Name) ? "YOU!" : character.Target)}.";
            }
            else
            {
                statusText = $"{Enum.GetName(typeof(CharacterStatus.Status), player.Status)}.";

                statusText = statusText.ToLower();
            }
            var displayEquipment = new StringBuilder();
            displayEquipment.Append("<p>They are using:</p>")
                 .Append("<table>");

            if (character.Equipped.Light != null)
            {
                displayEquipment.Append("<tr><td style='width:175px;' class=\"cell-title\" title='Worn as light'>").Append("&lt;used as light&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Light?.Name ?? "(nothing)").Append("</td></tr>");
            }

            if (character.Equipped.Finger != null)
            {
                displayEquipment.Append("<tr><td class=\"cell-title\" title='Worn on finger'>").Append(" &lt;worn on finger&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Finger?.Name ?? "(nothing)").Append("</td></tr>");
            }

            if (character.Equipped.Finger2 != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Worn on finger'>").Append(" &lt;worn on finger&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Finger2?.Name ?? "(nothing)").Append("</td></tr>");
            }

            if (character.Equipped.Neck != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Worn around neck'>").Append(" &lt;worn around neck&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Neck?.Name ?? "(nothing)").Append("</td></tr>");
            }

            if (character.Equipped.Neck2 != null)
            {
                displayEquipment.Append("<tr><td class=\"cell-title\" title='Worn around neck'>").Append(" &lt;worn around neck&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Neck2?.Name ?? "(nothing)").Append("</td></tr>");
            }

            if (character.Equipped.Face != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Worn on face'>").Append(" &lt;worn on face&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Face?.Name ?? "(nothing)").Append("</td></tr>");
            }

            if (character.Equipped.Head != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Worn on head'>").Append(" &lt;worn on head&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Head?.Name ?? "(nothing)").Append("</td></tr>");
            }

            if (character.Equipped.Torso != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Worn on torso'>").Append(" &lt;worn on torso&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Torso?.Name ?? "(nothing)").Append("</td></tr>");
            }
            if (character.Equipped.Legs != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Worn on legs'>").Append(" &lt;worn on legs&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Legs?.Name ?? "(nothing)").Append("</td></tr>");
            }
            if (character.Equipped.Feet != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Worn on feet'>").Append(" &lt;worn on feet&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Feet?.Name ?? "(nothing)").Append("</td></tr>");
            }

            if (character.Equipped.Hands != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Worn on hands'>").Append(" &lt;worn on hands&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Hands?.Name ?? "(nothing)").Append("</td></tr>");
            }

            if (character.Equipped.Arms != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Worn on arms'>").Append(" &lt;worn on arms&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Arms?.Name ?? "(nothing)").Append("</td></tr>");
            }

            if (character.Equipped.AboutBody != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Worn about body'>").Append(" &lt;worn about body&gt;").Append("</td>").Append("<td>").Append(character.Equipped.AboutBody?.Name ?? "(nothing)").Append("</td></tr>");
            }

            if (character.Equipped.Waist != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Worn on waist'>").Append(" &lt;worn about waist&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Waist?.Name ?? "(nothing)").Append("</td></tr>");
            }

            if (character.Equipped.Wrist != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Worn on wrist'>").Append(" &lt;worn around wrist&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Wrist?.Name ?? "(nothing)").Append("</td></tr>");
            }

            if (character.Equipped.Wrist2 != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Worn on wrist'>").Append(" &lt;worn around wrist&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Wrist2?.Name ?? "(nothing)").Append("</td></tr>");
            }


            if (character.Equipped.Wielded != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='worn as weapon'>").Append(" &lt;wielded&gt;").Append("</td>").Append("<td>")
                 .Append(character.Equipped.Wielded?.Name ?? "(nothing)").Append("</td></tr>");
            }


            if (character.Equipped.Secondary != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='worn as weapon'>").Append(" &lt;secondary&gt;").Append("</td>").Append("<td>")
            .Append(character.Equipped.Secondary?.Name ?? "(nothing)").Append("</td></tr>");
            }


            if (character.Equipped.Shield != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Worn as shield'>").Append(" &lt;worn as shield&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Shield?.Name ?? "(nothing)").Append("</td></tr>");
            }

            if (character.Equipped.Held != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Held'>").Append(" &lt;Held&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Held?.Name ?? "(nothing)").Append("</td></tr>");
            }

            if (character.Equipped.Floating != null)
            {
                displayEquipment.Append("<tr><td  class=\"cell-title\" title='Floating Nearby'>").Append(" &lt;Floating nearby&gt;").Append("</td>").Append("<td>").Append(character.Equipped.Floating?.Name ?? "(nothing)").Append("</td></tr>").Append("</table");
            }

            if (character.Equipped.Light == null && character.Equipped.Finger == null && character.Equipped.Finger2 == null && character.Equipped.Neck == null && character.Equipped.Neck2 == null &&
                character.Equipped.Face == null && character.Equipped.Head == null && character.Equipped.Torso == null && character.Equipped.Legs == null && character.Equipped.Feet == null &&
                character.Equipped.Hands == null && character.Equipped.Arms == null && character.Equipped.AboutBody == null && character.Equipped.Waist == null && character.Equipped.Wrist == null &&
                character.Equipped.Wrist2 == null && character.Equipped.Wielded == null && character.Equipped.Secondary == null && character.Equipped.Shield == null && character.Equipped.Held == null &&
                character.Equipped.Held == null && character.Equipped.Floating == null)
            {

                displayEquipment.Append("</table").Append("<p>Nothing.</p>");
            }



            _writeToClient.WriteLine($"{sb}<p class='{(isDark ? "room-dark" : "")}'>{character.Description} <br/>{character.Name} {_formulas.TargetHealth(player, character)} and is {statusText}<br/> {displayEquipment}", player.ConnectionId);




            foreach (var pc in room.Players)
            {
                if (pc.Name == player.Name)
                {
                    continue;
                }

                _writeToClient.WriteLine($"<p>{player.Name} looks at {character.Name.ToLower()}.</p>", pc.ConnectionId);
            }




            //if (item.ItemType == Item.Item.ItemTypes.Container)
            //{
            //  LookInContainer(target, room, player);
            //}
        }

        public void ExamineObject(string target, Room room, Player player)
        {
            if (player.Status == CharacterStatus.Status.Sleeping)
            {
                _writeToClient.WriteLine("You can't do that while asleep.", player.ConnectionId);
                return;
            }

            var nthTarget = Helpers.findNth(target);

            var item = Helpers.findRoomObject(nthTarget, room) ?? Helpers.findObjectInInventory(nthTarget, player);
            var character = Helpers.FindMob(nthTarget, room) ?? Helpers.FindPlayer(nthTarget, room);

            if (character != null)
            {
                LookObject(target, room, player);
                return;
            }
            RoomObject roomObjects = null;


            var isDark = RoomIsDark(room, player);
            if (item == null && room.RoomObjects.Count >= 1 && room.RoomObjects[0].Name != null)
            {
                roomObjects =
                    room.RoomObjects.FirstOrDefault(x =>
                        x.Name.Contains(target, StringComparison.CurrentCultureIgnoreCase));

                _writeToClient.WriteLine($"<p class='{(isDark ? "room-dark" : "")}'>{roomObjects.Examine}", player.ConnectionId);

                return;
            }

            if (item == null)
            {
                _writeToClient.WriteLine("<p>You don't see that here.", player.ConnectionId);
                return;
            }

            var examMessage = item.Description.Exam == "You don't see anything special."
                ? $"On closer inspection you don't see anything special to note to what you already see. {item.Description.Look}"
                : item.Description.Exam;
            _writeToClient.WriteLine($"<p class='{(isDark ? "room-dark" : "")}'>{examMessage}", player.ConnectionId);

            foreach (var pc in room.Players)
            {
                if (pc.Name == player.Name)
                {
                    continue;
                }

                _writeToClient.WriteLine($"<p>{player.Name} examines {item.Name.ToLower()}.</p>", pc.ConnectionId);
            }





            //if (item.ItemType == Item.Item.ItemTypes.Container)
            //{
            //    _writeToClient.WriteLine($"<p>You look inside {item.Name}", player.ConnectionId);
            //    foreach (var obj in item.Container.Items.List(false))
            //    {
            //        _writeToClient.WriteLine($"<p>{obj}</p>", player.ConnectionId);
            //    }
            //}
        }

        public void SmellObject(string target, Room room, Player player)
        {
            if (player.Status == CharacterStatus.Status.Sleeping)
            {
                _writeToClient.WriteLine("You can't do that while asleep.", player.ConnectionId);
                return;
            }

            var nthTarget = Helpers.findNth(target);
            var item = Helpers.findRoomObject(nthTarget, room) ?? Helpers.findObjectInInventory(nthTarget, player);


            if (item == null)
            {
                _writeToClient.WriteLine("<p>You don't see that here.", player.ConnectionId);
                return;
            }
            var isDark = RoomIsDark(room, player);

            _writeToClient.WriteLine($"<p class='{(isDark ? "room-dark" : "")}'>{item.Description.Smell}", player.ConnectionId);

            foreach (var pc in room.Players)
            {
                if (pc.Name == player.Name)
                {
                    continue;
                }

                _writeToClient.WriteLine($"<p>{player.Name} smells {item.Name.ToLower()}.</p>", pc.ConnectionId);
            }
        }

        public void TasteObject(string target, Room room, Player player)
        {
            if (player.Status == CharacterStatus.Status.Sleeping)
            {
                _writeToClient.WriteLine("You can't do that while asleep.", player.ConnectionId);
                return;
            }

            var nthTarget = Helpers.findNth(target);
            var item = Helpers.findRoomObject(nthTarget, room) ?? Helpers.findObjectInInventory(nthTarget, player);


            if (item == null)
            {
                _writeToClient.WriteLine("<p>You don't see that here.", player.ConnectionId);
                return;
            }

            var isDark = RoomIsDark(room, player);

            _writeToClient.WriteLine($"<p class='{(isDark ? "room-dark" : "")}'>{item.Description.Taste}", player.ConnectionId);


            foreach (var pc in room.Players)
            {
                if (pc.Name == player.Name)
                {
                    continue;
                }

                _writeToClient.WriteLine($"<p>{player.Name} tastes {item.Name.ToLower()}.</p>", pc.ConnectionId);
            }

        }

        public void TouchObject(string target, Room room, Player player)
        {
            if (player.Status == CharacterStatus.Status.Sleeping)
            {
                _writeToClient.WriteLine("You can't do that while asleep.", player.ConnectionId);
                return;
            }


            var nthTarget = Helpers.findNth(target);
            var item = Helpers.findRoomObject(nthTarget, room) ?? Helpers.findObjectInInventory(nthTarget, player);

            if (item == null)
            {
                _writeToClient.WriteLine("<p>You don't see that here.", player.ConnectionId);
                return;
            }

            var isDark = RoomIsDark(room, player);

            _writeToClient.WriteLine($"<p class='{(isDark ? "room-dark" : "")}'>{item.Description.Touch}", player.ConnectionId);

            foreach (var pc in room.Players)
            {
                if (pc.Name == player.Name)
                {
                    continue;
                }

                _writeToClient.WriteLine($"<p>{player.Name} feels {item.Name.ToLower()}.</p>", pc.ConnectionId);
            }

        }

        public Item.Item GetItemAttributes(int index, Room room)
        {

            return room.Items.FirstOrDefault(x => x.Id.Equals(index));
        }

        public string DisplayItems(Room room, Player player)
        {
            var isDark = RoomIsDark(room, player);
            var items = room.Items.List();
            var x = string.Empty;
            int index = 0;
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.Name))
                {

                    var i = GetItemAttributes(item.Id, room);
                    var keyword = i.Name.Split(" ");

                    var data = $"{{detail: {{name: \" {HtmlEncoder.Default.Encode(i.Name)}\", desc: \"{HtmlEncoder.Default.Encode(i.Description.Look)}\", type: \"{i.ItemType}\", canOpen: \"{i.Container.CanOpen}\", isOpen: \"{i.Container.IsOpen}\", keyword: \"{HtmlEncoder.Default.Encode(keyword[keyword.Length - 1])}\"}}}}";

                    var clickEvent = $"window.dispatchEvent(new CustomEvent(\"open-detail\", {data}))";
                    x += $"<p onClick='{clickEvent}' class='item {(isDark ? "dark-room" : "")}' >{item.Name}</p>";
                }
                index++;

            }

            return x;

        }

        public string DisplayMobs(Room room, Player player)
        {
            var mobs = string.Empty;
            var mobName = string.Empty;
            var isDark = RoomIsDark(room, player);
            var isFightingPC = false;
            foreach (var mob in room.Mobs)
            {

                if (!string.IsNullOrEmpty(mob.LongName))
                {
                    mobName = mob.LongName;
                }
                else
                {
                    mobName = mob.Name;
                }

                if (!string.IsNullOrEmpty(mob.Mounted.MountedBy))
                {
                    mobName += " tosses it's mane and snorts";
                }
                
                if (player.Target == mob.Name)
                {
                    isFightingPC = true; 
                }
                

                if (!string.IsNullOrEmpty(mob.LongName))
                {
                    if (mob.Status == CharacterStatus.Status.Fighting)
                    {
                        mobs +=
                            $"<p class='mob {(isDark ? "dark-room" : "")}'>{mob.Name} is here{(isFightingPC ? " fighting YOU!" : "")}</p>";
                    }
                    else
                    {
                        mobs +=
                            $"<p class='mob {(isDark ? "dark-room" : "")}'>{mobName}</p>";
                    }
                }
                else
                {
                    mobs += $"<p class='mob {(isDark ? "dark-room" : "")}'>{mobName} is here{(isFightingPC ? " fighting YOU!" : ".")}.</p>";
                }

              

            }

            return mobs;

        }

        public bool RoomIsDark(Room room, Player player)
        {
            if (room.RoomLit)
            {
                return false;
            }

            if (room.Type == Room.RoomType.Inside || room.Type == Room.RoomType.Town || room.Type == Room.RoomType.Shop || room.Type == Room.RoomType.Guild)
            {
                return false;
            }

            if (room.Terrain == Room.TerrainType.Inside)
            {
                return false;
            }

            if (player.Equipped.Light != null)
            {
                return false;
            }

            if (!_time.IsNightTime())
            {
                return true;
            }

            return false;
        }

        public bool LoreSuccess(int? skillLevel)
        {
            var chance = _dice.Roll(1, 1, 100);

            return skillLevel >= chance;
        }

        public void DoLore(Item.Item item, Player player)
        {
            // only lore items that can be picked up
            if (item.Stuck)
            {
                return;
            }

            var sb = new StringBuilder();

            //It is a level 45 armor, weight 4.
            //    Locations it can be worn: finger
            //    Special properties: evil
            //    Alignments allowed: evil neutral
            //This armor has a gold value of 45000.
            //    Armor class is 6 of 6. 
            //    Affects strength by 1. 
            //    Affects wisdom by 1. 
            //    Affects mana by 60. 
            //    Affects hit roll by 4. 

            sb.Append($"It is a level {item.Level} {item.ItemType}, weight {item.Weight}.<br/>Locations it can be worn: {(item.ItemType == Item.Item.ItemTypes.Light || item.ItemType == Item.Item.ItemTypes.Weapon || item.ItemType == Item.Item.ItemTypes.Armour ? item.Slot : Character.Equipment.Equipment.EqSlot.Held)}.<br /> This {item.ItemType} has a gold value of {item.Value}.<br />");

            if (item.ArmourRating.Armour != 0 || item.ArmourRating.Magic != 0)
            {
                sb.Append($"Affects armour by {item.ArmourRating.Armour} / {item.ArmourRating.Magic}");
            }

            if (item.Modifier.Strength != 0)
            {
                sb.Append($"<br />Affects strength by {item.Modifier.Strength}");
            }

            if (item.Modifier.Dexterity != 0)
            {
                sb.Append($"<br />Affects dexterity by {item.Modifier.Dexterity}");
            }

            if (item.Modifier.Constitution != 0)
            {
                sb.Append($"<br />Affects constitution by {item.Modifier.Constitution}");
            }

            if (item.Modifier.Wisdom != 0)
            {
                sb.Append($"<br />Affects wisdom by {item.Modifier.Wisdom}");
            }

            if (item.Modifier.Intelligence != 0)
            {
                sb.Append($"<br />Affects intelligence by {item.Modifier.Intelligence}");
            }

            if (item.Modifier.Charisma != 0)
            {
                sb.Append($"<br />Affects charisma by {item.Modifier.Charisma}");
            }

            if (item.Modifier.HP != 0)
            {
                sb.Append($"<br />Affects HP by {item.Modifier.HP}");
            }


            if (item.Modifier.Mana != 0)
            {
                sb.Append($"<br />Affects mana by {item.Modifier.Mana}");
            }

            if (item.Modifier.Moves != 0)
            {
                sb.Append($"<br />Affects moves by {item.Modifier.Moves}");
            }

            if (item.Modifier.DamRoll != 0)
            {
                sb.Append($"<br />Affects damroll by {item.Modifier.DamRoll}");
            }

            if (item.Modifier.HitRoll != 0)
            {
                sb.Append($"<br />Affects hitroll by {item.Modifier.HitRoll}");
            }

            if (item.Modifier.Saves != 0)
            {
                sb.Append($"<br />Affects saves by {item.Modifier.Saves}");
            }

            if (item.Modifier.SpellDam != 0)
            {
                sb.Append($"<br />Affects spell dam by {item.Modifier.SpellDam}");
            }


            _writeToClient.WriteLine(sb.ToString(), player.ConnectionId);

        }

        public string DisplayPlayers(Room room, Player player)
        {
            var players = string.Empty;
            var isNightTime = RoomIsDark(room, player);
            var pcName = string.Empty;

            foreach (var pc in room.Players)
            {
                if (pc.Name == player.Name)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(pc.LongName))
                {
                    pcName = $"{ pc.Name}";
                }
                else
                {
                    pcName = $"{ pc.Name} {pc.LongName}";
                }

                if (!string.IsNullOrEmpty(pc.Mounted.Name))
                {
                    pcName += $", is riding {pc.Mounted.Name}";
                }
                else if (string.IsNullOrEmpty(pc.LongName))
                {
                    pcName += " is here";

                }

                pcName += pc.Pose;




                players += $"<p class='player {(isNightTime ? "dark-room" : "")}'>{pcName}.</p>";
            }

            return players;

        }


        public string GetRoom(Exit exit)
        {
            if (exit == null)
            {
                return "";
            }


            var RoomId = $"{exit.AreaId}{exit.Coords.X}{exit.Coords.Y}{exit.Coords.Z}";
            var room = _cache.GetRoom(RoomId);

            return room.Title;
        }


        /// <summary>
        /// Displays valid exits
        /// </summary>
        public string FindValidExits(Room room, bool verbose)
        {
            var exits = new List<string>();
            var exitList = string.Empty;

            /* TODO: Click event for simple exit view */

            if (room.Exits.North != null)
            {

                var clickEvent = "window.dispatchEvent(new CustomEvent(\"post-to-server\", {\"detail\":\"n\"}))";
                exits.Add(verbose
                    ? $"<tr class='verbose-exit-wrapper'><td class='verbose-exit'>{Helpers.DisplayDoor(room.Exits.North)} </td><td style='text-align:center; color:#fff'> - </td><td class='verbose-exit-name'><a href='javascript:void(0)' onclick='{clickEvent}'>{GetRoom(room.Exits.North)}</a></td></tr>"
                    : Helpers.DisplayDoor(room.Exits.North));
            }



            if (room.Exits.East != null && room.Exits.East.Coords != null)
            {
                var clickEvent = "window.dispatchEvent(new CustomEvent(\"post-to-server\", {\"detail\":\"e\"}))";
                exits.Add(verbose
                    ? $"<tr class='verbose-exit-wrapper'><td class='verbose-exit'>{Helpers.DisplayDoor(room.Exits.East)} </td><td style='text-align:center; color:#fff'> - </td><td class='verbose-exit-name'><a href='javascript:void(0)' onclick='{clickEvent}'>{GetRoom(room.Exits.East)}</a></td></tr>"
                    : Helpers.DisplayDoor(room.Exits.East));
            }



            if (room.Exits.South != null)
            {
                var clickEvent = "window.dispatchEvent(new CustomEvent(\"post-to-server\", {\"detail\":\"s\"}))";
                exits.Add(verbose
                    ? $"<tr class='verbose-exit-wrapper'><td class='verbose-exit'>{Helpers.DisplayDoor(room.Exits.South)} </td><td style='text-align:center; color:#fff'> - </td><td class='verbose-exit-name'><a href='javascript:void(0)' onclick='{clickEvent}'>{GetRoom(room.Exits.South)}</a></td></tr>"
                    : Helpers.DisplayDoor(room.Exits.South));
            }

            if (room.Exits.West != null)
            {
                var clickEvent = "window.dispatchEvent(new CustomEvent(\"post-to-server\", {\"detail\":\"w\"}))";
                exits.Add(verbose
                    ? $"<tr class='verbose-exit-wrapper'><td class='verbose-exit'>{Helpers.DisplayDoor(room.Exits.West)} </td><td style='text-align:center; color:#fff'> - </td><td class='verbose-exit-name'><a href='javascript:void(0)' onclick='{clickEvent}'>{GetRoom(room.Exits.West)}</a></td></tr>"
                    : Helpers.DisplayDoor(room.Exits.West));
            }

            if (room.Exits.NorthEast != null)
            {
                var clickEvent = "window.dispatchEvent(new CustomEvent(\"post-to-server\", {\"detail\":\"ne\"}))";
                exits.Add(verbose
                    ? $"<tr class='verbose-exit-wrapper'><td class='verbose-exit'>{Helpers.DisplayDoor(room.Exits.NorthEast)}  </td><td style='text-align:center; color:#fff'> - </td><td class='verbose-exit-name'><a href='javascript:void(0)' onclick='{clickEvent}'>{GetRoom(room.Exits.NorthEast)}</a></td></tr>"
                    : Helpers.DisplayDoor(room.Exits.NorthEast));
            }

            if (room.Exits.SouthEast != null)
            {
                var clickEvent = "window.dispatchEvent(new CustomEvent(\"post-to-server\", {\"detail\":\"se\"}))";
                exits.Add(verbose
                    ? $"<tr class='verbose-exit-wrapper'><td class='verbose-exit'>{Helpers.DisplayDoor(room.Exits.SouthEast)}  </td><td style='text-align:center; color:#fff'> - </td><td class='verbose-exit-name'><a href='javascript:void(0)' onclick='{clickEvent}'>{GetRoom(room.Exits.SouthEast)}</a></td></tr>"
                    : Helpers.DisplayDoor(room.Exits.SouthEast));
            }

            if (room.Exits.SouthWest != null)
            {
                var clickEvent = "window.dispatchEvent(new CustomEvent(\"post-to-server\", {\"detail\":\"sw\"}))";
                exits.Add(verbose
                    ? $"<tr class='verbose-exit-wrapper'><td class='verbose-exit'>{Helpers.DisplayDoor(room.Exits.SouthWest)}  </td><td style='text-align:center; color:#fff'> - </td><td class='verbose-exit-name'><a href='javascript:void(0)' onclick='{clickEvent}'>{GetRoom(room.Exits.SouthWest)}</a></td></tr>"
                    : Helpers.DisplayDoor(room.Exits.SouthWest));
            }

            if (room.Exits.NorthWest != null)
            {
                var clickEvent = "window.dispatchEvent(new CustomEvent(\"post-to-server\", {\"detail\":\"nw\"}))";
                exits.Add(verbose
                    ? $"<tr class='verbose-exit-wrapper'><td class='verbose-exit'>{Helpers.DisplayDoor(room.Exits.NorthWest)}  </td> <td style='text-align:center; color:#fff'> - </td><td class='verbose-exit-name'><a href='javascript:void(0)' onclick='{clickEvent}'>{GetRoom(room.Exits.NorthWest)}</a></td></tr>"
                    : Helpers.DisplayDoor(room.Exits.NorthWest));
            }

            if (room.Exits.Down != null)
            {
                var clickEvent = "window.dispatchEvent(new CustomEvent(\"post-to-server\", {\"detail\":\"d\"}))";
                exits.Add(verbose
                    ? $"<tr class='verbose-exit-wrapper'><td class='verbose-exit'>{Helpers.DisplayDoor(room.Exits.Down)}  </td> <td style='text-align:center; color:#fff'> - </td><td class='verbose-exit-name'><a href='javascript:void(0)' onclick='{clickEvent}'>{GetRoom(room.Exits.Down)}</a></td></tr>"
                    : Helpers.DisplayDoor(room.Exits.Down));
            }

            if (room.Exits.Up != null)
            {
                var clickEvent = "window.dispatchEvent(new CustomEvent(\"post-to-server\", {\"detail\":\"u\"}))";
                exits.Add(verbose
                    ? $"<tr class='verbose-exit-wrapper'><td class='verbose-exit'>{Helpers.DisplayDoor(room.Exits.Up)}  </td> <td style='text-align:center; color:#fff'> - </td><td class='verbose-exit-name'><a href='javascript:void(0)' onclick='{clickEvent}'>{GetRoom(room.Exits.Up)}</a></td></tr>"
                    : Helpers.DisplayDoor(room.Exits.Up));
            }



            if (exits.Count <= 0)
            {
                exits.Add("None");
            }


            foreach (var exit in exits)
            {
                if (!verbose)
                {
                    exitList += exit + ", ";
                }
                else
                {
                    exitList += exit;
                }

            }
            if (!verbose)
            {
                exitList = exitList.Remove(exitList.Length - 2);

            }
            return exitList;

        }


    }
}
