using System;
using Amude.Global;
namespace Amude.Domain.Attribute
{
    internal enum SpecialAbilityType
    {
        Passive,
        Attack
    }

    [Serializable]
    internal class SpecialAbility
    {
        public SpecialAbilityType Type { get; set; }

        public bool Friendly { get; set; }

        public Affect Affect { get; set; }

        public String Name { get; set; }

        public String RootName { get; set; }

        public int Range { get; set; }

        public float Percentage { get; set; }

        public Affect Process(Character actor, Character target)
        {
            if (RandomMath.Percentage(Percentage))
            {
                if (Type == SpecialAbilityType.Attack)
                {
                    return Affect;
                }
                else
                {
                    int x_gap = Math.Abs(actor.MapLocation.X - target.MapLocation.X);
                    int y_gap = Math.Abs(actor.MapLocation.Y - target.MapLocation.Y);
                    if (x_gap + y_gap <= Range)
                    {
                        return Affect;
                    }
                }
            }

            return null;
        }
    }
}
