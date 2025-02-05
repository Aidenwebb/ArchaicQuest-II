﻿using ArchaicQuestII.GameLogic.Core;
using System.Collections.Generic;
using ArchaicQuestII.GameLogic.Character.Model;
using ArchaicQuestII.GameLogic.Effect;
using ArchaicQuestII.GameLogic.Item;

namespace ArchaicQuestII.GameLogic.Character.Class
{
    public class SkillList
    {
        public int SkillId { get; set; }
        public string SkillName { get; set; }
        public int Level { get; set; }
        public int? Proficiency { get; set; } = 1;

        public bool IsSpell { get; set; }
    }


    public class Class : OptionDescriptive
    {
        public List<SkillList> Skills { get; set; } = new List<SkillList>();
        public Dice HitDice { get; set; } = new Dice();
        public int ExperiencePointsCost { get; set; } = 0;
        public Attributes AttributeBonus { get; set; } = new Attributes();
        public string PreferredWeapon { get; set; }
    }
}
