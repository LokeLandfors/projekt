// Anything that can take damage (enemies, the player, breakable barrels...) implements this.
// Your player's health script and your mage spells should use it too.
public interface IDamageable
{
    void TakeDamage(int amount);
}
