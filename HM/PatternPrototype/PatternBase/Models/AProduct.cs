using PatternBase.Interfaces;

namespace PatternBase.Models;

public abstract class AProduct : IEntity, IMyCloneable<AProduct>
{
    /// <inheritdoc/>
    public long Id { get; protected set; }
    protected readonly string _name;
    /// <inheritdoc/>
    public string Name => _name;
    /// <inheritdoc/>
    public decimal Price { get; protected set; }
    public AProduct(long id, string name, decimal price)
    {
        Id = id;
        _name = name;
        Price = price;
    }

    public AProduct Clone()
    {
        return MemberwiseClone() as AProduct ?? throw new Exception();
    }
}
