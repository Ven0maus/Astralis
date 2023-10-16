namespace Astralis.GameCode.Npcs.Config
{
    /* TODO: Adjust to only 1 character setup, auto calculated per row. So we can have diverse characters */
    internal enum NpcConfiguration
    {
        /* Male SPRITE SETUP */
        Male_Forward = 1,
        Male_Sideways = 2,
        Male_Backwards = 3,

        Male_Shirt_Forward = 4,
        Male_Shirt_Sideways = 5,
        Male_Shirt_Backwards = 6,

        Male_Pants_Forward = 7,
        Male_Pants_Sideways = 8,
        Male_Pants_Backwards = 9,

        Male_Hair_Forward = 10,
        Male_Hair_Sideways = 11,
        Male_Hair_Backwards = 12,

        /* FEMALE SPRITE SETUP */
        Female_Forward = 17,
        Female_Sideways = 18,
        Female_Backwards = 19,

        Female_Shirt_Forward = 20,
        Female_Shirt_Sideways = 21,
        Female_Shirt_Backwards = 22,

        Female_Pants_Forward = 23,
        Female_Pants_Sideways = 24,
        Female_Pants_Backwards = 25,

        Female_Hair_Forward = 26,
        Female_Hair_Sideways = 27,
        Female_Hair_Backwards = 28,
    }

    internal enum Facing
    {
        Forward,
        Left,
        Backwards,
        Right,
    }

    internal enum Gender
    {
        Male,
        Female
    }

    internal enum Race
    {
        Human,
        Elf,
        Orc,
        Dwarf
    }

    internal enum Class
    {
        Warrior,
        Ranger,
        Wizard,
        Cleric,
        Rogue,
        Necromancer,
        Alchemist,
        Shapeshifter
    }
}
