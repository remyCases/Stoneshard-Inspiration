// Copyright (C)
// See LICENSE file for extended copyright information.
// This file is part of the repository from .

using ModShardLauncher;
using ModShardLauncher.Mods;
using UndertaleModLib.Models;
using System.Linq;
using System.Collections.Generic;

namespace Inspiration;
public class Inspiration : Mod
{
    public override string Author => "author";
    public override string Name => "Inspiration";
    public override string Description => "mod_description";
    public override string Version => "0.1.0";
    public override string TargetVersion => "0.8.2.10";

    public override void PatchMod()
    {
		UndertaleSprite s_pray = Msl.GetSprite("s_pray");
        s_pray.OriginX = 12;
        s_pray.OriginY = 12;
		UndertaleSprite s_b_pray = Msl.GetSprite("s_b_pray");
        s_b_pray.OriginX = 12;
        s_b_pray.OriginY = 12;

        UndertaleGameObject pray_ico = Msl.AddObject("o_skill_pray_ico", "s_pray", "o_skill_ico", true, false, true, CollisionShapeFlags.Circle);
        UndertaleGameObject pray = Msl.AddObject("o_skill_pray", "s_pray", "o_skill", true, false, true, CollisionShapeFlags.Circle);
        UndertaleGameObject b_pray = Msl.AddObject("o_b_pray", "s_b_pray", "o_buff_maneuver", true, false, true, CollisionShapeFlags.Circle);

        Msl.AddNewEvent(pray_ico, @"
            event_inherited()
            child_skill = o_skill_pray
            event_perform_object(child_skill, ev_create, 0)
        ", EventType.Create, 0);
        
        Msl.AddNewEvent(pray, @"
            event_inherited()
            skill = ""pray""
            scr_skill_atr()
        ", EventType.Create, 0);
        
        Msl.AddNewEvent(b_pray, @"
            event_inherited()
            scr_buff_atr()
            stack = 0
            first_turn = 1
            buff_snd = snd_brynn_church_bell_1hit
        ", EventType.Create, 0);

        Msl.AddNewEvent(b_pray, @"
            event_inherited()
            with (target)
            {
                ds_map_clear(other.data)
                ds_map_add(other.data, ""CRT"", 50)
                ds_map_add(other.data, ""CRTD"", 50)
                scr_atr_calc(id)
            }
        ", EventType.Alarm, 2);

        Msl.AddSkillTree("Inspiration", MetaCaterory.Utilities, "s_branch_modded", new SkillLocation("o_skill_pray_ico", 55, 24));
        
        Msl.LoadGML("gml_GlobalScript_scr_skill_tier_init")
            .MatchFrom("}")
            .InsertBelow("global.inspiration_tier1 = [\"Inspiration\", o_skill_pray_ico]")
            .Save();
			
        Msl.LoadGML("gml_GlobalScript_table_Modifiers")
            .Apply(ModifiersIterator)
            .Save();

        Msl.LoadGML("gml_GlobalScript_table_skills")
            .Apply(SkillsIterator)
            .Save();
        
        Msl.LoadGML("gml_GlobalScript_table_text")
            .Apply(TextIterator)
            .Save();

        Msl.LoadGML("gml_GlobalScript_table_speech")
            .Apply(SpeechIterator)
            .Save();

        Msl.InjectTableSkillsStat(
            metaGroup: Msl.SkillsStatMetaGroup.MAGICMASTERY,
            id: "pray",
            Object: "o_b_pray", 
            Target: Msl.SkillsStatTarget.NoTarget, 
            Range: "0", 
            KD: 20, 
            MP: 10, 
            Reserv: 0, 
            Duration : 5, 
            AOE_Lenght: 0, 
            AOE_Width : 0, 
            is_movement: false, 
            Pattern: Msl.SkillsStatPattern.normal, 
            Class : Msl.SkillsStatClass.spell, 
            Bonus_Range: false, 
            Starcast: "", 
            Branch: Msl.SkillsStatBranch.none, 
            is_knockback: false, 
            Crime: false, 
            metacategory: Msl.SkillsStatMetacategory.none, 
            FMB: 0, 
            AP: "x", 
            Attack: false, 
            Stance: false, 
            Charge: false, 
            Maneuver: false, 
            Spell: true);

        Msl.LoadAssemblyAsString("gml_Object_o_skill_fast_panel_Alarm_0")
            .MatchFrom("pushi.e 3622")
            .InsertAbove("pushglb.v global.inspiration_tier1\ncall.i gml_Script_scr_skill_open_from_array1d(argc=1)\npopz.v")
            .Save();
    }
    private static IEnumerable<string> TextIterator(IEnumerable<string> input)
    {
        string tier = $"\"{string.Concat(Enumerable.Repeat($"Inspiration;", 12))}\", ";

        string rarity_ru = "Inspiration / Inspiration / Inspiration / Inspiration";
        string rarity_en = "Inspiration";
        string rarity_ch = "Inspiration";
        string rarity_ge = "Inspiration / Inspiration / Inspiration / Inspiration";
        string rarity_sp = "Inspiration / Inspiration / Inspiration / Inspiration";
        string rarity_fr = "Inspiration / Inspiration / Inspiration / Inspiration";
        string rarity_it = "Inspiration / Inspiration / Inspiration / Inspiration";
        string rarity_po = "Inspiration / Inspiration / Inspiration / Inspiration";
        string rarity_pl = "Inspiration / Inspiration / Inspiration / Inspiration";
        string rarity_tu = "Inspiration";
        string rarity_jp = "Inspiration";
        string rarity_kr = "Inspiration";
        string rarity = $"\"10;{rarity_ru};{rarity_en};{rarity_ch};{rarity_ge};{rarity_sp};{rarity_fr};{rarity_it};{rarity_po};{rarity_pl};{rarity_tu};{rarity_jp};{rarity_kr};\",";
        string hover = $"\"Inspiration;{string.Concat(Enumerable.Repeat($"Inspired you are;", 12))}\",";
        
        foreach(string item in input)
        {
            if (item.Contains("Tier_name_end"))
            {
                string newItem = item;
                newItem = newItem.Insert(newItem.IndexOf("\";Tier_name_end"), tier);
                newItem = newItem.Insert(newItem.IndexOf("\";rarity_end"), rarity);
                newItem = newItem.Insert(newItem.IndexOf("\";skilltree_hover_end"), hover);
                yield return newItem;
            }
            else
            {
                yield return item;
            }
        }
    }
    private static IEnumerable<string> ModifiersIterator(IEnumerable<string> input)
    {
        string buff_name = "buff_name;\",";
        string buff_desc = "buff_desc;\",";

        string id_pray = "o_b_pray";
        string name_pray_en = "Pray";
        string name_pray = $"{id_pray};" + string.Concat(Enumerable.Repeat($"{name_pray_en};", 12));
        string desc_pray_en = "I prayed.";
        string desc_pray = $"{id_pray};" + string.Concat(Enumerable.Repeat($"{desc_pray_en};", 12));

        string name = $"\"{name_pray}\",";
        string desc = $"\"{desc_pray}\",";
        
        foreach(string item in input)
        {
            if (item.Contains(buff_name))
            {
                string newItem = item;
                newItem = newItem.Insert(newItem.IndexOf(buff_name) + buff_name.Length, name);
                newItem = newItem.Insert(newItem.IndexOf(buff_desc) + buff_desc.Length, desc);
                yield return newItem;
            }
            else
            {
                yield return item;
            }
        }
    }
    private static IEnumerable<string> SkillsIterator(IEnumerable<string> input)
    {
        string id_pray = "pray";
        string name_pray_en = "Pray";
        string name_pray = $"{id_pray};" + string.Concat(Enumerable.Repeat($"{name_pray_en};", 12));
        string desc_pray_en = "Pray a lot.";
        string desc_pray = $"{id_pray};" + string.Concat(Enumerable.Repeat($"{desc_pray_en};", 12));

        string name = $"\"{name_pray}\",";
        string desc = $"\"{desc_pray}\",";

        string undead = "\";;///// UNDEAD;///// UNDEAD;;;;;///// UNDEAD;///// UNDEAD;;;;\",";

        foreach(string item in input)
        {
            if (item.Contains("\";skill_name_end") && item.Contains(undead))
            {
                string newItem = item;
                newItem = newItem.Insert(newItem.IndexOf("\";skill_name_end"), name);
                newItem = newItem.Insert(newItem.IndexOf(undead) + undead.Length, desc.Replace('\n', ' '));
                yield return newItem;
            }
            else
            {
                yield return item;
            }
        }
    }
    private static IEnumerable<string> SpeechIterator(IEnumerable<string> input)
    {
        string forbidden = "\";;// FORBIDDEN MAGIC;// FORBIDDEN MAGIC;;;;;;// FORBIDDEN MAGIC;;;;\",";
        
        string pray_start = string.Concat(Enumerable.Repeat("pray;", 15));
        string pray_speech = string.Concat(Enumerable.Repeat("Volunt En!;", 15));
        string pray_end = string.Concat(Enumerable.Repeat("pray_end;", 15));
        
        string speech = 
            $"\"{pray_start}\",\"{pray_speech}\",\"{pray_end}\",";
        
        foreach(string item in input)
        {
            if (item.Contains(forbidden))
            {
                string newItem = item.Insert(item.IndexOf(forbidden) + forbidden.Length, speech);
                yield return newItem;
            }
            else
            {
                yield return item;
            }
        }
    }
}
