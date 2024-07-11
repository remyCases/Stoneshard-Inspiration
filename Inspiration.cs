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
        UndertaleSprite s_weapon_dealer = Msl.GetSprite("s_weapon_dealer");
        s_weapon_dealer.OriginX = 12;
        s_weapon_dealer.OriginY = 12;
        UndertaleSprite s_corruption = Msl.GetSprite("s_corruption");
        s_corruption.OriginX = 12;
        s_corruption.OriginY = 12;  
        UndertaleSprite s_fearsome = Msl.GetSprite("s_fearsome");
        s_fearsome.OriginX = 12;
        s_fearsome.OriginY = 12;

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

        Msl.AddNewEvent(pray, @"
            event_inherited()
            ds_map_replace(text_map, ""CRT"", 30 + (0.5 * owner.WIL))
            ds_map_replace(text_map, ""Miracle_Chance"", 30 + (0.5 * owner.WIL))
        ", EventType.Other, 17);
        
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
                ds_map_add(other.data, ""CRT"", 40)
                ds_map_add(other.data, ""Miracle_Chance"", 40)
                scr_atr_calc(id)
            }
        ", EventType.Alarm, 2);

        Msl.AddFunction(@"
        function scr_mod_inspiration_check_buff_pray(argument0)
        {
            if ((attack_result != ""crit"") && argument0)
            {
                if scr_instance_exists_in_list(o_b_pray)
                {
                    with (o_b_pray)
                        instance_destroy()
                }
            }
        }  
        ", "scr_mod_inspiration_check_buff_pray");
        
        Msl.LoadAssemblyAsString("gml_GlobalScript_scr_attack")
            .Apply(AttackIterator)
            .Save();

        Msl.LoadGML("gml_Object_o_skill_Other_13")
            .MatchAll()
            .InsertBelow(@"
if(asset_get_index(spell) != o_b_pray && !is_crit && ((damage > 0) || (duration > 0))) 
{
    if (scr_instance_exists_in_list(o_b_pray, owner.buffs))
    {
        with(o_b_pray)
        {
            instance_destroy()
        }
    }
}")
            .Save();

        UndertaleGameObject weapon_dealer = Msl.AddObject($"o_pass_skill_weapon_dealer", $"s_weapon_dealer", "o_skill_passive", true, false, true, CollisionShapeFlags.Circle);

        // ds_list_find_index(category, "weapon")
        // gml_GlobalScript_scr_trade_item (price is already computed)
        // Buying_Prices affected
        // sell weapons : buyer_inventory.object_index == o_inventory
        // to modify gml_Object_o_inv_slot_Step_0
        // 100 ?
        // add :
        // var bonus_weapon_price = 0 (ligne 70)
        // if ((ds_list_find_index(category, "weapon") >= 0)) { bonus_weapon_price = 1 or 1.5 } (ligne 80-81)
        // durable_price * bonus_weapon_price (ligne 101)
        UndertaleGameObject corruption = Msl.AddObject($"o_pass_skill_corruption", $"s_corruption", "o_skill_passive", true, false, true, CollisionShapeFlags.Circle);

        Msl.LoadAssemblyAsString("gml_Object_o_inv_slot_Step_0")
            .MatchFrom("pop.v.i local._curse_price")
            .InsertBelow("pushi.e 1\npop.v.i local.bonus_weapon_price")
            .MatchFrom("push.s \"is_cursed\"")
            .InsertAbove(@"
push.s ""weapon""
conv.s.v
push.v self.category
call.i ds_list_find_index(argc=2)
pushi.e 0
cmp.i.v GTE
bf [801]

:[800]
push.d 0.5
pop.v.d local.bonus_weapon_price

:[801]
")
            .Save();
        
        UndertaleGameObject fearsome = Msl.AddObject($"o_pass_skill_fearsome", $"s_fearsome", "o_skill_passive", true, false, true, CollisionShapeFlags.Circle);

        Msl.AddNewEvent(weapon_dealer, @"
            event_inherited()
            scr_skill_atr(""weapon_dealer"")
            passive = 1
        ", EventType.Create, 0);
        
        Msl.AddNewEvent(corruption, @"
            event_inherited()
            scr_skill_atr(""corruption"")
            passive = 1
        ", EventType.Create, 0);

        Msl.AddNewEvent(fearsome, @"
            event_inherited()
            scr_skill_atr(""fearsome"")
            passive = 1
        ", EventType.Create, 0);

        Msl.AddSkillTree("Inspiration", MetaCaterory.Utilities, "s_branch_modded", 
            new SkillLocation("o_skill_pray_ico", 55, 24),
            new SkillLocation("o_pass_skill_weapon_dealer", 55, 81),
            new SkillLocation("o_pass_skill_corruption", 55, 138),
            new SkillLocation("o_pass_skill_fearsome", 128, 24)
        );
        
        Msl.LoadGML("gml_GlobalScript_scr_skill_tier_init")
            .MatchFrom("}")
            .InsertBelow("global.inspiration_tier1 = [\"Inspiration\", o_skill_pray_ico, o_pass_skill_weapon_dealer, o_pass_skill_corruption, o_pass_skill_fearsome]")
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
            Class : Msl.SkillsStatClass.skill, 
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
            Maneuver: true, 
            Spell: false);

        Msl.LoadAssemblyAsString("gml_Object_o_skill_fast_panel_Alarm_0")
            .MatchFrom("pushi.e 3622")
            .InsertAbove("pushglb.v global.inspiration_tier1\ncall.i gml_Script_scr_skill_open_from_array1d(argc=1)\npopz.v")
            .Save();
    }
    private static IEnumerable<string> AttackIterator(IEnumerable<string> input)
    {
        string insert = @"
pushloc.v local._isPlayerAttacker
call.i gml_Script_scr_mod_inspiration_check_buff_pray(argc=1)
popz.v";

        bool findHit = false;
        bool insertDone = false;

        foreach (string item in input)
        {
            if (!findHit && item.Contains("pushloc.v local.hit"))
            {
                findHit = true;
            }
            else if (findHit && item.Contains("ret.v"))
            {
                insertDone = true;
                yield return insert;
            }
            else if (!insertDone && findHit)
            {
                findHit = false;
            }
            yield return item;
        }   
    }
    private static IEnumerable<string> TextIterator(IEnumerable<string> input)
    {
        string tier = $"\"{string.Concat(Enumerable.Repeat($"Inspiration;", 12))}\", ";

        Dictionary<ModLanguage, string> rarity_dict = Localization.SetDictionary(new Dictionary<ModLanguage, string>() {
            {ModLanguage.Russian, "Inspiration / Inspiration / Inspiration / Inspiration"},
            {ModLanguage.English, "Inspiration"},
            {ModLanguage.Chinese, "Inspiration"},
            {ModLanguage.German, "Inspiration / Inspiration / Inspiration / Inspiration"},
            {ModLanguage.Spanish, "Inspiration / Inspiration / Inspiration / Inspiration"},
            {ModLanguage.French, "Inspiration / Inspiration / Inspiration / Inspiration"},
            {ModLanguage.Italian, "Inspiration / Inspiration / Inspiration / Inspiration"},
            {ModLanguage.Portuguese, "Inspiration / Inspiration / Inspiration / Inspiration"},
            {ModLanguage.Polish, "Inspiration / Inspiration / Inspiration / Inspiration"},
            {ModLanguage.Turkish, "Inspiration"},
            {ModLanguage.Japanese, "Inspiration"},
            {ModLanguage.Korean, "Inspiration"},
        });
        string rarity = $"\"10;{string.Join(";", rarity_dict.Values)}\",";

        string hover_en = "Manipulate others and yourself by being charismatic.";
        Dictionary<ModLanguage, string> hover_dict = Localization.SetDictionary(new Dictionary<ModLanguage, string>() {
           {ModLanguage.English, hover_en},
        });
        string hover = $"\"Inspiration;{string.Join(";", hover_dict.Values)}\",";
        
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
        string desc_pray_en = "You believe a powerful being is guiding you, granting ~lg~40~/~% critical chance and ~lg~40~/~% miracle chance for ~lg~5~/~ turns. Landing a non-critical attack or a non-miracle spell will remove this buff.";

        Dictionary<ModLanguage, string> name_pray = Localization.SetDictionary(new Dictionary<ModLanguage, string>() {{ModLanguage.English, name_pray_en},});
        Dictionary<ModLanguage, string> desc_pray = Localization.SetDictionary(new Dictionary<ModLanguage, string>() {{ModLanguage.English, desc_pray_en},});

        string id_weapon_dealer = "weapon_dealer";
        string name_weapon_dealer_en = "Weapon dealer";
        string desc_weapon_dealer_en = "Sell value of weapons is increased by ~lg~50~/~%. You can also give weapons with more than ~lg~80~/~% durability to a settlement elder or quatermaster to improve your reputation.";

        Dictionary<ModLanguage, string> name_weapon_dealer = Localization.SetDictionary(new Dictionary<ModLanguage, string>() {{ModLanguage.English, name_weapon_dealer_en},});
        Dictionary<ModLanguage, string> des_weapon_dealer = Localization.SetDictionary(new Dictionary<ModLanguage, string>() {{ModLanguage.English, desc_weapon_dealer_en},});
        
        string id_corruption = "corruption";
        string name_corruption_en = "Corruption";
        string desc_corruption_en = "Once a day, guards can now increase your reputation, for a little fee. You can also ask them to follow and help you for ~lg~5~/~ tiles around their settlement.";

        Dictionary<ModLanguage, string> name_corruption = Localization.SetDictionary(new Dictionary<ModLanguage, string>() {{ModLanguage.English, name_corruption_en},});
        Dictionary<ModLanguage, string> des_corruption = Localization.SetDictionary(new Dictionary<ModLanguage, string>() {{ModLanguage.English, desc_corruption_en},});
        
        string id_fearsome = "fearsome";
        string name_fearsome_en = "Fearsome";
        string desc_fearsome_en = "You're so imposing during combat, your foes are more likely to flee while hit by your attack.";

        Dictionary<ModLanguage, string> name_fearsome = Localization.SetDictionary(new Dictionary<ModLanguage, string>() {{ModLanguage.English, name_fearsome_en},});
        Dictionary<ModLanguage, string> des_fearsome = Localization.SetDictionary(new Dictionary<ModLanguage, string>() {{ModLanguage.English, desc_fearsome_en},});

        string name = @$"""{id_pray};{string.Join(";", name_pray.Values)}"",
            ""{id_weapon_dealer};{string.Join(";", name_weapon_dealer.Values)}"",
            ""{id_corruption};{string.Join(";", name_corruption.Values)}"",
            ""{id_fearsome};{string.Join(";", name_fearsome.Values)}"",";
        string desc = @$"""{id_pray};{string.Join(";", desc_pray.Values)}"",
            ""{id_weapon_dealer};{string.Join(";", des_weapon_dealer.Values)}"",
            ""{id_corruption};{string.Join(";", des_corruption.Values)}"",
            ""{id_fearsome};{string.Join(";", des_fearsome.Values)}"",";

        string undead = "\";;///// UNDEAD;///// UNDEAD;;;;;///// UNDEAD;///// UNDEAD;;;;\",";

        foreach(string item in input)
        {
            if (item.Contains("\";skill_name_end") && item.Contains(undead))
            {
                string newItem = item;
                newItem = newItem.Insert(newItem.IndexOf("\";skill_name_end"), name.Replace('\n', ' '));
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
        string pray_speech = string.Concat(Enumerable.Repeat("Volunt... En!;", 15));
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
