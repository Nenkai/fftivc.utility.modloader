using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace fftivc.utility.modloader.Interfaces.Tables.Structures;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
/// <summary>
/// ABILITY_COMMON_DATA
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct ABILITY_COMMON_DATA // As per mobile symbols
{
    public ushort JPCost { get; set; }
    public byte ChanceToLearn { get; set; }
    public byte Flags { get; set; }
    // 4 bytes, but we combined it
    public AIBehaviorFlags AIBehaviorFlags { get; set; }
}

[Flags]
public enum AbilityFlags : byte
{
    Unk_0 = 1 << 0,
    LearnOnHit = 1 << 1,
    DisplayAbilityName = 1 << 2,
    LearnWithJP = 1 << 3,
}

public enum AbilityType : byte
{
    None = 0,
    Normal = 1,
    Item = 2,
    Throwing = 3,
    Jumping = 4,
    Aim = 5,
    Math = 6,
    Reaction = 7,
    Support = 8,
    Movement = 9,
}

[Flags]
public enum AIBehaviorFlags : int
{
    TargetAllies = 1 << 0,
    TargetEnemies = 1 << 1,
    Unequip = 1 << 2,
    Stats = 1 << 3,
    AddStatus = 1 << 4,
    CancelStatus = 1 << 5,
    MP = 1 << 6,
    HP = 1 << 7,
    Silence = 1 << 8,
    Evadeable = 1 << 9,
    AffectedByFaith = 1 << 10,
    RandomHits = 1 << 11,
    CheckCT_Target = 1 << 12,
    UndeadReverse = 1 << 13,
    Reflectable = 1 << 14,
    TargetMap = 1 << 15,
    PhysicalAttack = 1 << 16,
    MagicalAttack = 1 << 17,
    Ranged3Directions = 1 << 18,
    Melee3Directions = 1 << 19,
    NonSpearAttack = 1 << 20,
    LinearAttack = 1 << 21,
    StopAtObstacle = 1 << 22,
    Arced = 1 << 23,
    EvadeWithMotion = 1 << 24,
    Unk_25 = 1 << 25,
    UseWeaponRange = 1 << 26,
    RequireMonsterSkill = 1 << 27,
    Unk_28 = 1 << 28,
    OnlyHitsEnemies = 1 << 29,
    OnlyHitsAlliesOrSelf = 1 << 30,
    UsableByAI = 1 << 31,
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member