using PatternBase.Models;

namespace TestsProj.TestModels;

[TestClass]
public sealed class CloneTests
{
    private Food? bread;
    private Food? drinkingWhater;
    private Food? apple;
    private Food? condies;

    private Manufacturer? manufacturer1;

    private Furniture? table;
    private Furniture? closet;
    private Furniture? bed;

    private Delivery? delivery1;
    private Delivery? delivery2;
    private Installation? installation1;
    private Repair? repair1;

    [TestInitialize]
    public void Startup()
    {
        bread = new Food(1, "White bread", 50);
        drinkingWhater = new(2, "Whater", 200);
        apple = new Food(3, "Green apple", 9);
        condies = new Food(4, "Cholate condies", 230);

        manufacturer1 = new Manufacturer(1, "Furniture manufacturer");

        table = new Furniture(1, "Oak table", 15_000, 1_000, 5_000, manufacturer1);
        closet = new Furniture(2, "Ceder closet", 18_000, 4_000, 10_000, manufacturer1);
        bed = new Furniture(3, "Steel bed", 120_000, 24_000, 45_000, manufacturer1);

        delivery1 = new Delivery(1, "Deliver to client 1", 1_500, "Moskov, ul. Kirova, d. 20");
        delivery1.Products.Add((bread, 2));
        delivery1.Products.Add((drinkingWhater, 1));
        delivery1.Products.Add((apple, 1.5d));
        delivery1.Products.Add((condies, 1.2d));

        delivery2 = new Delivery(2, "Deliver to client 2", 1_500, "Moskov, ul. Chernishevskogo, d. 125");
        delivery2.Products.Add((table, 1));
        delivery2.Products.Add((bed, 1));
        installation1 = new Installation(3, "Installation to client 2");
        installation1.Furnitures.Add(table);
        installation1.Furnitures.Add(bed);

        repair1 = new Repair(4, "Repair furniture for client 3");
        repair1.Furnitures.Add(closet);
    }

    /// <summary>
    /// Тестирование поверхностного клонирования интерфейса IMyCloneable
    /// </summary>
    [TestMethod]
    public void SuperficialIMyCloneableTest()
    {
        Assert.IsNotNull(apple);
        var clone = apple.Clone();
        Assert.IsNotNull(clone);
        Assert.IsTrue(clone.Id == apple.Id);
        Assert.IsTrue(clone.Name == apple.Name);
        Assert.IsTrue(clone.Price == apple.Price);
    }
    /// <summary>
    /// Тестирование глубокого клонирования интерфейса IMyCloneable
    /// </summary>
    [TestMethod]
    public void DeepIMyCloneableTest()
    {
        Assert.IsNotNull(table);
        var clone = table.Clone();
        Assert.IsNotNull(clone);
        Assert.IsTrue(clone.Id == table.Id);
        Assert.IsTrue(clone.Name == table.Name);
        Assert.IsTrue(clone.Price == table.Price);
        Assert.IsTrue(clone.Manufacturer != table.Manufacturer);
    }
    /// <summary>
    /// Тестирование поверхностного клонирования интерфейса ICloneable
    /// </summary>
    [TestMethod]
    public void SuperficialICloneableTest()
    {
        Assert.IsNotNull(installation1);
        var clone = installation1.Clone() as Installation;
        Assert.IsNotNull(clone);
        Assert.IsTrue(clone.Id == installation1.Id);
        Assert.IsTrue(clone.Name == installation1.Name);
        Assert.IsTrue(clone.Furnitures.Count > 0);
        Assert.IsTrue(clone.Furnitures.Count == installation1.Furnitures.Count);
        Assert.IsTrue(clone.Price == installation1.Price);
        Assert.IsTrue(clone.Furnitures[0] == installation1.Furnitures[0] && clone.Furnitures[0].Id == installation1.Furnitures[0].Id);
    }
    /// <summary>
    /// Тестирование глубокого клонирования интерфейса ICloneable
    /// </summary>
    [TestMethod]
    public void DeepICloneableTest()
    {
        Assert.IsNotNull(delivery1);
        var clone = delivery1.Clone() as Delivery;
        Assert.IsNotNull(clone);
        Assert.IsTrue(clone.Id == delivery1.Id);
        Assert.IsTrue(clone.Name == delivery1.Name);
        Assert.IsTrue(clone.Products.Count > 0);
        Assert.IsTrue(clone.Products.Count == delivery1.Products.Count);
        Assert.IsTrue(clone.Price == delivery1.Price);
        Assert.IsTrue(clone.Products[0] != delivery1.Products[0] && clone.Products[0].product.Id == delivery1.Products[0].product.Id);
    }
}
