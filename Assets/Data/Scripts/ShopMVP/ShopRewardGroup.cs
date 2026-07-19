namespace Shop
{
    /// <summary>
    /// Groups a paid offer by the resource the player receives, so offers can be
    /// placed into separate slots (crystals / decor / soft / chests) instead of
    /// being grouped by how they are paid for.
    /// </summary>
    public enum ShopRewardGroup
    {
        Soft = 0,
        Decor = 1,
        Hard = 2,
        Chest = 3,
        Experience = 4
    }
}
