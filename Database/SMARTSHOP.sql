CREATE DATABASE IF NOT EXISTS SMARTSHOP CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE SMARTSHOP;

CREATE TABLE Product (
    ProductId INT PRIMARY KEY,
    ProductName VARCHAR(128) NOT NULL,
    Category VARCHAR(64) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    Features VARCHAR(512) NOT NULL,
    Description TEXT NULL
);

CREATE TABLE Review (
    ReviewId INT AUTO_INCREMENT PRIMARY KEY,
    ProductId INT NOT NULL,
    CustomerName VARCHAR(64) NOT NULL,
    Content VARCHAR(1000) NOT NULL,
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    Sentiment VARCHAR(16) NULL,
    Score DOUBLE NULL,
    Summary VARCHAR(256) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT FK_Review_Product FOREIGN KEY (ProductId) REFERENCES Product(ProductId)
);

INSERT INTO Product (ProductId, ProductName, Category, Price, Features, Description) VALUES
(1, 'Galaxy S25', 'Phone', 22990000, '6.2 inch AMOLED, Snapdragon 8 Elite, 50MP camera', NULL),
(2, 'MacBook Air M4', 'Laptop', 28490000, '13.6 inch, 16GB RAM, 18 hours battery', NULL),
(3, 'AirPods Pro 3', 'Accessory', 6790000, 'Active noise cancellation, USB-C, IP57', NULL),
(4, 'Xiaomi Redmi Note 15', 'Phone', 6490000, '120Hz display, 5500mAh battery, 108MP camera', NULL);