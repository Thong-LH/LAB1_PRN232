USE master;
GO

IF EXISTS (SELECT * FROM sys.databases WHERE name = 'HotelBookingDB')
BEGIN
    ALTER DATABASE HotelBookingDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE HotelBookingDB;
END
GO

CREATE DATABASE HotelBookingDB;
GO

USE HotelBookingDB;
GO

CREATE TABLE RoomTypes (
    RoomTypeID INT IDENTITY(1,1) PRIMARY KEY,
    TypeName NVARCHAR(100) NOT NULL,
    BasePrice DECIMAL(18,2) NOT NULL
);

CREATE TABLE Rooms (
    RoomID INT IDENTITY(1,1) PRIMARY KEY,
    RoomNumber NVARCHAR(50) NOT NULL,
    RoomTypeID INT NOT NULL FOREIGN KEY REFERENCES RoomTypes(RoomTypeID),
    Status NVARCHAR(50) NOT NULL
);

CREATE TABLE Services (
    ServiceID INT IDENTITY(1,1) PRIMARY KEY,
    ServiceName NVARCHAR(100) NOT NULL,
    ServicePrice DECIMAL(18,2) NOT NULL
);

CREATE TABLE RoomTypeServices (
    RoomTypeID INT NOT NULL FOREIGN KEY REFERENCES RoomTypes(RoomTypeID),
    ServiceID INT NOT NULL FOREIGN KEY REFERENCES Services(ServiceID),
    PRIMARY KEY (RoomTypeID, ServiceID)
);

CREATE TABLE Guests (
    GuestID INT IDENTITY(1,1) PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) NULL,
    Phone NVARCHAR(50) NULL
);

CREATE TABLE Bookings (
    BookingID INT IDENTITY(1,1) PRIMARY KEY,
    GuestID INT NOT NULL FOREIGN KEY REFERENCES Guests(GuestID),
    CheckInDate DATETIME NOT NULL,
    CheckOutDate DATETIME NOT NULL
);

CREATE TABLE BookingDetails (
    BookingID INT NOT NULL FOREIGN KEY REFERENCES Bookings(BookingID),
    RoomID INT NOT NULL FOREIGN KEY REFERENCES Rooms(RoomID),
    NightCount INT NOT NULL,
    PRIMARY KEY (BookingID, RoomID)
);

-- Seed Data

-- 1. Services
INSERT INTO Services (ServiceName, ServicePrice) VALUES 
('An Sang', 100000.00),
('Giat La', 50000.00),
('Buffet Toi', 300000.00),
('Dua Don San Bay', 500000.00),
('Su dung Gym/Pool', 150000.00),
('Minibar Free', 0.00),
('Spa va Massage', 400000.00);

-- 2. RoomTypes
INSERT INTO RoomTypes (TypeName, BasePrice) VALUES
('Standard Single', 450000.00),
('Standard Double', 800000.00),
('Deluxe King', 1500000.00),
('Suite Ocean', 3000000.00),
('Family Connect', 2200000.00),
('Executive Suite', 5000000.00);

-- 3. RoomTypeServices
INSERT INTO RoomTypeServices (RoomTypeID, ServiceID) VALUES
(1, 1),
(2, 1),
(3, 1), (3, 5),
(4, 1), (4, 3), (4, 4),
(5, 1), (5, 2),
(6, 1), (6, 7), (6, 4);

-- 4. Rooms
INSERT INTO Rooms (RoomNumber, RoomTypeID, Status) VALUES
('P101', 1, 'Trong'),
('P102', 1, 'Dang don'),
('P201', 2, 'Trong'),
('P301', 3, 'Dang o'),
('P401', 4, 'Trong'),
('P505', 6, 'Trong'),
('P606', 6, 'Trong');

-- 5. Guests
INSERT INTO Guests (FullName, Email, Phone) VALUES
('Hoang Gia Bao', 'giabao@gmail.com', '0987654321'),
('Nguyen Van A', 'vana@gmail.com', '0123456789');

-- 6. Bookings
INSERT INTO Bookings (GuestID, CheckInDate, CheckOutDate) VALUES
(1, '2026-07-15', '2026-07-19'),
(2, '2026-07-10', '2026-07-12');

-- 7. BookingDetails
INSERT INTO BookingDetails (BookingID, RoomID, NightCount) VALUES
(1, 5, 4),
(2, 4, 2);
