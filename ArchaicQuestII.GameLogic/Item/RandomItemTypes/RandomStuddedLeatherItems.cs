﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ArchaicQuestII.GameLogic.Character;
using ArchaicQuestII.GameLogic.Character.Equipment;
using ArchaicQuestII.GameLogic.Core;

namespace ArchaicQuestII.GameLogic.Item.RandomItemTypes
{
    public class RandomStuddedLeatherItems : IRandomStuddedLeatherArmour
    {

        private IDice _dice;

        public RandomStuddedLeatherItems(IDice dice)
        {
            _dice = dice;
        }
        public List<PrefixItemMods> Prefix = new List<PrefixItemMods>()
        {
            new PrefixItemMods()
            {
                Name = "Bronze studded leather",
                MinArmour = 3,
                MaxArmour = 3,
            },
            new PrefixItemMods()
            {
                Name = "Iron studded leather",
                MinArmour = 3,
                MaxArmour = 6
            },
            new PrefixItemMods()
            {
                Name = "Steel studded leather",
                MinArmour = 3,
                MaxArmour = 7
            },
            new PrefixItemMods()
            {
                Name = "Alloy studded leather",
                MinArmour = 4,
                MaxArmour = 8
            },
            new PrefixItemMods()
            {
                Name = "Fine alloy studded leather",
                MinArmour = 4,
                MaxArmour = 9
            },
            new PrefixItemMods()
            {
                Name = "Mithril studded leather",
                MinArmour = 5,
                MaxArmour = 10
            },
            new PrefixItemMods()
            {
                Name = "Adamantium studded leather",
                MinArmour = 5,
                MaxArmour = 11
            },
            new PrefixItemMods()
            {
                Name = "Asterite studded leather",
                MinArmour = 6,
                MaxArmour = 12
            },
            new PrefixItemMods()
            {
                Name = "Netherium studded leather",
                MinArmour = 6,
                MaxArmour = 13
            },
            new PrefixItemMods()
            {
                Name = "Arcanium studded leather",
                MinArmour = 7,
                MaxArmour = 14
            },
            new PrefixItemMods()
            {
                Name = "Dragonskin studded leather",
                MinArmour = 7,
                MaxArmour = 15
            },
            new PrefixItemMods()
            {
                Name = "Studded Leather",
                MinArmour = 3,
                MaxArmour = 5
            }
        };

        public List<Item> HeadItemName = new List<Item>()
        {

            new Item()
            {
                Name = "Helmet",
               ArmourType = Item.ArmourTypes.Cloth,
               Slot = Equipment.EqSlot.Head,
               Description = new Description()
               {
                   Look = "A fitted #prefix# helmet.",
                   Exam = "A fitted #prefix# helmet."
               }
            },
            new Item()
            {
                Name = "Hat",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Head,
                Description = new Description()
                {
                    Look = "A simple #prefix# hat.",
                    Exam = "A simple #prefix# hat."
                }
            },
            new Item()
            {
                Name = "Skull Cap",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Head,
                Description = new Description()
                {
                    Look = "A fitted #prefix# skull cap.",
                    Exam = "A fitted #prefix# skull cap."
                }
            },
            new Item()
            {
                Name = "Helm",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Head,
                Description = new Description()
                {
                    Look = "A fitted #prefix# helm.",
                    Exam = "A fitted #prefix# helm."
                }
            }
        };
        public List<Item> LegItemName = new List<Item>()
        {

            new Item()
            {
                Name = "Leggings",
               ArmourType = Item.ArmourTypes.Cloth,
               Slot = Equipment.EqSlot.Legs,
               Description = new Description()
               {
                   Look = "A pair of #prefix# leggings",
                   Exam = "A pair of #prefix# leggings",
               }
            },
            new Item()
            {
                Name = "Trousers",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Legs,
                Description = new Description()
                {
                    Look = "some #prefix# trousers.",
                    Exam = "some #prefix# trousers."
                }
            },
            new Item()
            {
                Name = "Skirt",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Legs,
                Description = new Description()
                {
                    Look = "A protective #prefix# skirt.",
                    Exam = "A protective #prefix# skirt."
                }
            }

        };
        public List<Item> ArmItemName = new List<Item>()
        {

            new Item()
            {
                Name = "Sleeves",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Arms,
                Description = new Description()
                {
                    Look = "A pair of #prefix# sleeves",
                    Exam = "A pair of #prefix# sleeves",
                }
            },
            new Item()
            {
                Name = "armbands",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Arms,
                Description = new Description()
                {
                    Look = "A pair #prefix# armbands.",
                    Exam = "A pair  #prefix# armbands."
                }
            }

        };
        public List<Item> HandItemName = new List<Item>()
        {

            new Item()
            {
                Name = "Gloves",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Hands,
                Description = new Description()
                {
                    Look = "A pair of #prefix# gloves",
                    Exam = "A pair of #prefix# gloves",
                }
            },
            new Item()
            {
                Name = "Gauntlets",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Hands,
                Description = new Description()
                {
                    Look = "A pair of #prefix# gauntlets",
                    Exam = "A pair of #prefix# gauntlets",
                }
            },
        };
        public List<Item> FeetItemName = new List<Item>()
        {

            new Item()
            {
                Name = "Boots",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Feet,
                Description = new Description()
                {
                    Look = "A pair of #prefix# boots",
                    Exam = "A pair of #prefix# boots",
                }
            },
            new Item()
            {
                Name = "shoes",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Feet,
                Description = new Description()
                {
                    Look = "A pair #prefix# shoes.",
                    Exam = "A pair #prefix# shoes."
                }
            },
            new Item()
            {
                Name = "Knee-high boots",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Feet,
                Description = new Description()
                {
                    Look = "A pair #prefix# Knee-high boots.",
                    Exam = "A pair #prefix# Knee-high boots."
                }
            },
            new Item()
            {
                Name = "Moccasins",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Feet,
                Description = new Description()
                {
                    Look = "A pair #prefix# Moccasins.",
                    Exam = "A pair #prefix# Moccasins."
                }
            }

        };
        public List<Item> BodyItemName = new List<Item>()
        {


            new Item()
            {
                Name = "Jerkin",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Body,
                Description = new Description()
                {
                    Look = "A #prefix# jerkin.",
                    Exam = "A #prefix# jerkin."
                }
            },
            new Item()
            {
                Name = "Armour",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Body,
                Description = new Description()
                {
                    Look = "A #prefix# armour.",
                    Exam = "A #prefix# armour."
                }
            },
            new Item()
            {
                Name = "Tunic",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Body,
                Description = new Description()
                {
                    Look = "A #prefix# tunic.",
                    Exam = "A #prefix# tunic."
                }
            },
            new Item()
            {
                Name = "Vest",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Body,
                Description = new Description()
                {
                    Look = "A #prefix# vest.",
                    Exam = "A #prefix# vest."
                }
            },
            new Item()
            {
                Name = "Jacket",
                ArmourType = Item.ArmourTypes.Cloth,
                Slot = Equipment.EqSlot.Body,
                Description = new Description()
                {
                    Look = "A #prefix# jacket.",
                    Exam = "A #prefix# jacket."
                }
            },

        };


        public Item CreateRandomItem(Player player, bool legendary)
        {
            var items = HeadItemName.Concat(LegItemName).Concat(ArmItemName).Concat(HandItemName).Concat(FeetItemName)
                .Concat(BodyItemName).ToList();
            var prefix = Prefix[_dice.Roll(1, 0, Prefix.Count)];
            var choice = items[_dice.Roll(1, 0, items.Count)];


            var item = new Item()
            {
                Name = "a " + prefix.Name + " " + choice.Name,
                ItemType = Item.ItemTypes.Armour,
                Level = player.Level,
                Value = player.Level * 75,
                Condition = _dice.Roll(1, 75, 100),
                Weight = 2,
                ArmourRating = new ArmourRating()
                {
                    Armour = _dice.Roll(1, prefix.MinArmour, prefix.MaxArmour),
                    Magic = prefix.MaxArmour / prefix.MinArmour
                },
                Gold = player.Level * 75,
                Description = new Description()
                {
                    Look = $"{choice.Description.Look}",
                    Room = $"a {prefix.Name} {choice.Name} has been left here.",
                    Exam = $"{choice.Description.Exam}",
                },
                Slot = choice.Slot,
            };

            if (legendary)
            {
                item.ArmourRating.Armour += _dice.Roll(1, (int)(prefix.MinArmour * 1.5), prefix.MaxArmour * 2);
                item.ArmourRating.Magic += prefix.MaxArmour * 2 / prefix.MinArmour;
                item.Condition = 100;

                item.Name += " <span class='legendary'>(Legendary)</span>";
            }

            return item;

        }
    }
}
