# SQL
### Add Products Table - [1] Таблица "Products" (Продукты)  

```Sql
CREATE TABLE Products (
    ProductID bigserial primary key,
    ProductName varchar(100) NOT NULL,
    Description text default NULL,
	Price real NOT NULL,
    QuantityInStock int NOT NULL
);
```  

### Add Users Table - [2] Таблица "Users" (Пользователи)  

```Sql
CREATE TABLE Users (
    UserID bigserial primary key,
    UserName varchar(100) NOT NULL,
    Email varchar(50) default NULL,
	RegistrationDate date NOT NULL DEFAULT CURRENT_DATE
);
```  

### Add Orders Table - [3] Таблица "Orders" (Заказы)  

```Sql
CREATE TABLE Orders (
    OrderID bigserial primary key,
    UserID bigserial NOT NULL references Users(UserID),
    OrderDate timestamp NOT NULL,
	Status OrderStatus NOT NULL
);
```  

### Add OrderDetails Table - [4] Таблица "OrderDetails" (Детали заказа)  

```Sql
CREATE TABLE OrderDetails (
    OrderDetailID bigserial primary key,
    OrderID bigserial NOT NULL references Orders(OrderID),
    ProductID bigserial NOT NULL references Products(ProductID),
	Quantity int NOT NULL,
	TotalCost real NOT NULL
);
```  

### Add Product - Добавление нового продукта  

```Sql
INSERT INTO Products (ProductName, Description, Price, QuantityInStock) 
VALUES 
	('Dino TRex (Toy)', 'Green dinosaur toy.', 100, 10),
	('Bear (Toy)', 'White bear toy.', 150, 4),
    ('Rabbit (Toy)', 'White rabbit toy.', 130, 7),
    ('Red car (Toy)', 'Red car toy.', 90, 3),
    ('Black Pistol (Toy)', 'Black pistol toy.', 120, 12);
```

### Update Price - Обновление цены продукта  

```Sql
UPDATE Products SET Price = 88 WHERE ProductID = 1;
```

### Get All Orders by User - Выбор всех заказов определенного пользователя  

```Sql
SELECT OrderID, UserID, OrderDate, Status FROM Orders WHERE UserID = 1;
```

### Get SUM TotalCost by order - Расчет общей стоимости заказа  

```Sql
SELECT SUM(totalcost) AS total FROM OrderDetails WHERE OrderID = 2;
```

### Get SUM Quantity Stock - Подсчет количества товаров на складе  

```Sql
SELECT SUM(QuantityinStock) AS total FROM Products;
```

### Get Top 5 price products - Получение 5 самых дорогих товаров  

```Sql
SELECT Productid, ProductName, Description, Price, QuantityinStock FROM Products ORDER BY Price DESC LIMIT 5;
```

### Get Top products where count less 5 - Список товаров с низким запасом (менее 5 штук)  

```Sql
SELECT Productid, ProductName, Description, Price, QuantityinStock FROM Products WHERE QuantityinStock < 5 ORDER BY QuantityinStock;
```

# Add Other Data  

## Add Order Status Type - Добавление типа перечисления статуса заказа  

```Sql
CREATE TYPE OrderStatus AS ENUM ('Created', 'Deliving', 'Close');
```

## Add User - Добавление пользователя  

```Sql
INSERT INTO Users (UserName, Email, RegistrationDate) 
VALUES 
	('Sergey', 'sergey77@mail.ru', DEFAULT);
```

## Add Orders - Добавление заказов  

```Sql
INSERT INTO Orders (UserID, OrderDate, Status)
	VALUES (1, '2025-12-05', 'Created');
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, TotalCost)
	VALUES (1, 1, 4, 400);
UPDATE Products SET QuantityInStock = (SELECT QuantityInStock FROM Products WHERE ProductID = 1 LIMIT 1) - 4 WHERE ProductID = 1;

INSERT INTO Orders (UserID, OrderDate, Status)
	VALUES (1, '2025-12-04', 'Created');
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, TotalCost)
	VALUES (2, 2, 3, 450);
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, TotalCost)
	VALUES (2, 1, 5, 500);
UPDATE Products SET QuantityInStock = (SELECT QuantityInStock FROM Products WHERE ProductID = 2 LIMIT 1) - 3 WHERE ProductID = 2;
UPDATE Products SET QuantityInStock = (SELECT QuantityInStock FROM Products WHERE ProductID = 1 LIMIT 1) - 5 WHERE ProductID = 1;
```

