﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArchaicQuestII.GameLogic.Core;
using ArchaicQuestII.GameLogic.World.Room;

namespace ArchaicQuestII.GameLogic.Character.Equipment
{
   public class Equip: IEquip
    {
        private readonly IWriteToClient _writer;
        private readonly IUpdateClientUI _clientUi;

        public Equip(IWriteToClient writer, IUpdateClientUI clientUi)
        {
            _writer = writer;
            _clientUi = clientUi;
        }


        public string ShowEquipmentUI(Player player)
        {
            var displayEquipment = new StringBuilder();
            displayEquipment.Append("<p>You are using:</p>")
                .Append("<table>")
               .Append("<tr><td style='width:175px;' title='Worn as light'>").Append("&lt;used as light&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Light?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td title='Worn on finger'>").Append(" &lt;worn on finger&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Finger?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Worn on finger'>").Append(" &lt;worn on finger&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Finger2?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Worn around neck'>").Append(" &lt;worn around neck&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Neck?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td title='Worn around neck'>").Append(" &lt;worn around neck&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Neck2?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Worn on face'>").Append(" &lt;worn on face&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Face?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Worn on head'>").Append(" &lt;worn on head&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Head?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Worn on torso'>").Append(" &lt;worn on torso&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Torso?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Worn on legs'>").Append(" &lt;worn on legs&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Legs?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Worn on feet'>").Append(" &lt;worn on feet&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Feet?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Worn on hands'>").Append(" &lt;worn on hands&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Hands?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Worn on arms'>").Append(" &lt;worn on arms&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Arms?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Worn about body'>").Append(" &lt;worn about body&gt;").Append("</td>").Append("<td>").Append(player.Equipped.AboutBody?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Worn on waist'>").Append(" &lt;worn about waist&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Waist?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Worn on wrist'>").Append(" &lt;worn around wrist&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Wrist?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Worn on wrist'>").Append(" &lt;worn around wrist&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Wrist2?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='worn as weapon'>").Append(" &lt;wielded&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Wielded?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Worn as shield'>").Append(" &lt;worn as shield&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Shield?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Held'>").Append(" &lt;Held&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Held?.Name ?? "(nothing)").Append("</td></tr>")
                .Append("<tr><td  title='Floating Nearby'>").Append(" &lt;Floating nearby&gt;").Append("</td>").Append("<td>").Append(player.Equipped.Floating?.Name ?? "(nothing)").Append("</td></tr>").Append("</table");


            return displayEquipment.ToString();
        }

        private void EmitRemoveActionToRoom(Item.Item item, Room room, Player player)
        {
            foreach (var pc in room.Players)
            {
                if (pc.ConnectionId == player.ConnectionId)
                {
                    continue;
                }

                _writer.WriteLine($"<p>{pc.Name} stops using {item.Name.ToLower()}.</p>", pc.ConnectionId);
            }
        }

        private void EmitWearActionToRoom(string action, Room room, Player player)
        {
            foreach (var pc in room.Players)
            {
                if (pc.ConnectionId == player.ConnectionId)
                {
                    continue;
                }

                _writer.WriteLine($"<p>{pc.Name} equips {action}</p>", pc.ConnectionId);
            }
        }


        public void Remove(string item, Room room, Player player)
        {
            var itemToRemove = player.Inventory.FirstOrDefault(x => x.Name.Contains(item, StringComparison.CurrentCultureIgnoreCase) && x.Equipped);

            if (itemToRemove == null)
            {
                _writer.WriteLine("<p>You are not wearing that item.</p>", player.ConnectionId);
                return;
            }

            itemToRemove.Equipped = false;

            switch (itemToRemove.Slot)
            {
                case Equipment.EqSlot.Arms:
                    player.Equipped.Arms = null;
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Body:
                    player.Equipped.AboutBody = null;
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Face:
                    player.Equipped.Face = null;
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Feet:
                    player.Equipped.Feet = null;
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Finger:
                    player.Equipped.Finger = null; // TODO: slot 2
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Floating:
                    player.Equipped.Floating = null;
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Hands:
                    player.Equipped.Hands = null;
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Head:
                    player.Equipped.Head = null;
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Held:
                    player.Equipped.Held = null; // TODO: handle when wield and shield or 2hand item are equipped
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Legs:
                    player.Equipped.Legs = null;
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Light:
                    player.Equipped.Light = null;
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Neck:
                    player.Equipped.Neck = null; // TODO: slot 2
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Shield:
                    player.Equipped.Shield = null;
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Torso:
                    player.Equipped.Torso = null;
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Waist:
                    player.Equipped.Waist = null;
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Wielded:
                    player.Equipped.Wielded = null;
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                case Equipment.EqSlot.Wrist:
                    player.Equipped.Wrist = null; // TODO: slot 2
                    _writer.WriteLine($"<p>You stop using {itemToRemove.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitRemoveActionToRoom(itemToRemove, room, player);
                    break;
                default:
                    itemToRemove.Equipped = false;
                    _writer.WriteLine("<p>You don't know how to remove this.</p>", player.ConnectionId);
                    break;
            }

            _clientUi.UpdateEquipment(player);
            _clientUi.UpdateInventory(player);
        }

        public void ShowEquipment(Player player)
        {

            var displayEquipment = ShowEquipmentUI(player);
            _writer.WriteLine(displayEquipment, player.ConnectionId);
        }


        public void Wear(string item, Room room, Player player)
        {

            var itemToWear = player.Inventory.FirstOrDefault(x => x.Name.Contains(item, StringComparison.CurrentCultureIgnoreCase));

            if (itemToWear == null)
            {
                _writer.WriteLine("<p>You don't have that item.</p>", player.ConnectionId);
                return;
            }

            itemToWear.Equipped = true;
            switch (itemToWear.Slot)
            {
                case Equipment.EqSlot.Arms:
                    player.Equipped.Arms = itemToWear;
                    _writer.WriteLine($"<p>You wear {itemToWear.Name.ToLower()} on your arms.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} on {Helpers.GetPronoun(player.Gender)} arms.", room, player);
                    break;
                case Equipment.EqSlot.Body:
                    player.Equipped.AboutBody = itemToWear;
                    _writer.WriteLine($"<p>You wear {itemToWear.Name.ToLower()} about your body.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} about {Helpers.GetPronoun(player.Gender)} body.", room, player);
                    break;
                case Equipment.EqSlot.Face:
                    player.Equipped.Face = itemToWear;
                    _writer.WriteLine($"<p>You wear {itemToWear.Name.ToLower()} on your face.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} on {Helpers.GetPronoun(player.Gender)} face.", room, player);
                    break;
                case Equipment.EqSlot.Feet:
                    player.Equipped.Feet = itemToWear;
                    _writer.WriteLine($"<p>You wear {itemToWear.Name.ToLower()} on your feet.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} on {Helpers.GetPronoun(player.Gender)} feet.", room, player);
                    break;
                case Equipment.EqSlot.Finger:
                    player.Equipped.Finger = itemToWear; // TODO: slot 2
                    _writer.WriteLine($"<p>You wear {itemToWear.Name.ToLower()} on your finger.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} on {Helpers.GetPronoun(player.Gender)} finger.", room, player);
                    break;
                case Equipment.EqSlot.Floating:
                    player.Equipped.Floating = itemToWear;
                    _writer.WriteLine($"<p>You release {itemToWear.Name.ToLower()} to float around you.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} to float around {Helpers.GetPronoun(player.Gender)}.", room, player);
                    break;
                case Equipment.EqSlot.Hands:
                    player.Equipped.Hands = itemToWear;
                    _writer.WriteLine($"<p>You wear {itemToWear.Name.ToLower()} on your hands.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} on {Helpers.GetPronoun(player.Gender)} hands.", room, player);
                    break;
                case Equipment.EqSlot.Head:
                    player.Equipped.Head = itemToWear;
                    _writer.WriteLine($"<p>You wear {itemToWear.Name.ToLower()} on your head.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} on {Helpers.GetPronoun(player.Gender)} head.", room, player);
                    break;
                case Equipment.EqSlot.Held:
                    player.Equipped.Held = itemToWear; // TODO: handle when wield and shield or 2hand item are equipped
                    _writer.WriteLine($"<p>You hold {itemToWear.Name.ToLower()} in your hands.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} in {Helpers.GetPronoun(player.Gender)} hands.", room, player);
                    break;
                case Equipment.EqSlot.Legs:
                    player.Equipped.Legs = itemToWear;
                    _writer.WriteLine($"<p>You wear {itemToWear.Name.ToLower()} on your legs.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} on {Helpers.GetPronoun(player.Gender)} legs.", room, player);
                    break;
                case Equipment.EqSlot.Light:
                    player.Equipped.Light = itemToWear;
                    _writer.WriteLine($"<p>You equip {itemToWear.Name.ToLower()} as your light.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} as {Helpers.GetPronoun(player.Gender)} light.", room, player);
                    break;
                case Equipment.EqSlot.Neck:
                    player.Equipped.Neck = itemToWear; // TODO: slot 2
                    _writer.WriteLine($"<p>You wear {itemToWear.Name.ToLower()} around your neck.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} around {Helpers.GetPronoun(player.Gender)} neck.", room, player);
                    break;
                case Equipment.EqSlot.Shield:
                    player.Equipped.Shield = itemToWear;
                    _writer.WriteLine($"<p>You equip {itemToWear.Name.ToLower()} as your shield.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} as {Helpers.GetPronoun(player.Gender)} shield.", room, player);
                    break;
                case Equipment.EqSlot.Torso:
                    player.Equipped.Torso = itemToWear;
                    _writer.WriteLine($"<p>You wear {itemToWear.Name.ToLower()} around your torso.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} around {Helpers.GetPronoun(player.Gender)} torso.", room, player);
                    break;
                case Equipment.EqSlot.Waist:
                    player.Equipped.Waist = itemToWear;
                    _writer.WriteLine($"<p>You wear {itemToWear.Name.ToLower()} around your waist.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} around {Helpers.GetPronoun(player.Gender)} waist.", room, player);
                    break;
                case Equipment.EqSlot.Wielded:
                    player.Equipped.Wielded = itemToWear;
                    _writer.WriteLine($"<p>You wield {itemToWear.Name.ToLower()}.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()}.", room, player);
                    break;
                case Equipment.EqSlot.Wrist:
                    player.Equipped.Wrist = itemToWear; // TODO: slot 2
                    _writer.WriteLine($"<p>You wear {itemToWear.Name.ToLower()} on your wrist.</p>", player.ConnectionId);
                    EmitWearActionToRoom($"{itemToWear.Name.ToLower()} on {Helpers.GetPronoun(player.Gender)} wrist.", room, player);
                    break;
                default:
                    itemToWear.Equipped = false;
                    _writer.WriteLine("<p>You don't know how to wear this.</p>", player.ConnectionId);
                    break;
            }

            _clientUi.UpdateEquipment(player);
            _clientUi.UpdateInventory(player);
        }

    }
}
