using PatternBase.Interfaces;

namespace PatternBase.Models;

public abstract class Product : IEntity
{
    public long Id { get; protected set; }
    protected readonly string _name;
    public string Name => _name;
    public virtual decimal Price { get; protected set; }
    public virtual double Weight { get; protected set; }
    public virtual decimal DeliveryPrice { get; protected set; }
    public Product(string name)
    {
        _name = name;
    }
}
