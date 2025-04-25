public interface IDespawnable
{
    /// <returns>Amount of time in seconds to delay before destroying the object</returns>
    public float OnDespawn();
}