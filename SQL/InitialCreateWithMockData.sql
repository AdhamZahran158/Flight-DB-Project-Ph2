CREATE DATABASE AirlineDB;
GO
USE AirlineDB;
GO

CREATE TABLE Airport (
    AirportID   INT PRIMARY KEY,
    Name        VARCHAR(100),
    City        VARCHAR(50),
    Country     VARCHAR(50),
    IATACode    CHAR(3) UNIQUE,
    Longitude   DECIMAL(9,6),
    Latitude    DECIMAL(9,6)
);

CREATE TABLE Aircraft (
    AircraftID        INT PRIMARY KEY,
    Model             VARCHAR(50),
    PassengerCapacity INT,
    CargoCapacity     DECIMAL(10,2),
    MaxTakeOffWeight  DECIMAL(10,2)
);

CREATE TABLE Flight (
    FlightID           INT PRIMARY KEY,
    FlightNumber       VARCHAR(10),
    DistanceKM         DECIMAL(10,2),
    Status             VARCHAR(20),
    AircraftID         INT REFERENCES Aircraft(AircraftID),
    DepartureAirportID INT REFERENCES Airport(AirportID),
    DepartureTime      DATETIME,
    ArrivalAirportID   INT REFERENCES Airport(AirportID),
    ArrivalTime        DATETIME
);

CREATE TABLE Tenant (
    PassportNum VARCHAR(20) PRIMARY KEY,
    Fname       VARCHAR(50),
    Mname       VARCHAR(50),
    Lname       VARCHAR(50),
    Email       VARCHAR(100),
    NationalID  VARCHAR(20),
    Nationality VARCHAR(50)
);

CREATE TABLE TenantPhone (
    PassportNum VARCHAR(20),
    PhoneNumber VARCHAR(20),
    PRIMARY KEY (PassportNum, PhoneNumber),

    CONSTRAINT FK_TenantPhone_Tenant
    FOREIGN KEY (PassportNum)
    REFERENCES Tenant(PassportNum)
    ON DELETE CASCADE
);

CREATE TABLE Booking (
    BookingID     INT PRIMARY KEY,
    BookingDate   DATE,
    BookingStatus VARCHAR(20),
    TotalPrice    DECIMAL(10,2),
    PassportNum   VARCHAR(20),

    CONSTRAINT FK_Booking_Tenant
    FOREIGN KEY (PassportNum)
    REFERENCES Tenant(PassportNum)
    ON DELETE CASCADE
);

CREATE TABLE Payment (
    PaymentID     INT PRIMARY KEY,
    PaymentMethod VARCHAR(30),
    PaymentStatus VARCHAR(20),
    BookingID     INT,
    PaymentDate   DATE,

    CONSTRAINT FK_Payment_Booking
    FOREIGN KEY (BookingID)
    REFERENCES Booking(BookingID)
    ON DELETE CASCADE
);

CREATE TABLE Trip (
    TripID   INT PRIMARY KEY,
    TripType VARCHAR(20)
);

CREATE TABLE FlightTrip (
    FlightID INT REFERENCES Flight(FlightID),
    TripID   INT REFERENCES Trip(TripID),
    PRIMARY KEY (FlightID, TripID)
);

CREATE TABLE Seat (
    AircraftID INT,
    SeatNumber VARCHAR(5),
    ClassType  VARCHAR(20),

    PRIMARY KEY (AircraftID, SeatNumber),

    CONSTRAINT FK_Seat_Aircraft
    FOREIGN KEY (AircraftID)
    REFERENCES Aircraft(AircraftID)
    ON DELETE CASCADE
);

CREATE TABLE Ticket (
    TicketID    INT PRIMARY KEY,
    TicketPrice DECIMAL(10,2),
    BookingID   INT,
    TripID      INT,
    AircraftID  INT,
    SeatNumber  VARCHAR(5),

    CONSTRAINT FK_Ticket_Booking
    FOREIGN KEY (BookingID)
    REFERENCES Booking(BookingID)
    ON DELETE CASCADE,

    FOREIGN KEY (AircraftID, SeatNumber)
    REFERENCES Seat(AircraftID, SeatNumber)
);

CREATE TABLE Baggage (
    TicketID   INT REFERENCES Ticket(TicketID),
    BaggageID  INT,
    Weight     DECIMAL(6,2),
    ExtraFee   DECIMAL(8,2),
    PRIMARY KEY (TicketID, BaggageID)
);

--seed sample data

INSERT INTO Airport VALUES
(1, 'Cairo International',        'Cairo',   'Egypt',  'CAI',  31.405600, 30.121900),
(2, 'Dubai International',        'Dubai',   'UAE',    'DXB',  55.364400, 25.252800),
(3, 'Heathrow Airport',           'London',  'UK',     'LHR',  -0.461389, 51.477500);

INSERT INTO Aircraft VALUES
(1, 'Boeing 737',    189, 20000.00, 79016.00),
(2, 'Airbus A320',   180, 18000.00, 73500.00),
(3, 'Boeing 777',    396, 50000.00, 247200.00);

INSERT INTO Flight VALUES
(1, 'MS701', 2165.00, 'Scheduled', 1, 1, '2026-06-01 08:00', 2, '2026-06-01 12:30'),
(2, 'EK201', 5510.00, 'Scheduled', 3, 2, '2026-06-02 14:00', 3, '2026-06-02 19:45'),
(3, 'MS801', 3500.00, 'Delayed',   2, 1, '2026-06-03 09:00', 3, '2026-06-03 15:00');

INSERT INTO Tenant VALUES
('A12345678', 'Ahmed',  'Mohamed', 'Ali',    'ahmed@email.com',  '29901011234', 'Egyptian'),
('B98765432', 'Sara',   NULL,      'Hassan', 'sara@email.com',   '30005152345', 'Egyptian'),
('C11223344', 'James',  'Robert',  'Smith',  'james@email.com',  '88011013456', 'British');

INSERT INTO TenantPhone VALUES
('A12345678', '+201001234567'),
('A12345678', '+201119876543'),
('B98765432', '+201211112222'),
('C11223344', '+447911123456');

INSERT INTO Booking VALUES
(1, '2026-05-01', 'Confirmed', 3500.00, 'A12345678'),
(2, '2026-05-02', 'Confirmed', 7200.00, 'B98765432'),
(3, '2026-05-03', 'Pending',   1800.00, 'C11223344');

INSERT INTO Payment VALUES
(1, 'Credit Card', 'Paid',    1, '2026-05-01'),
(2, 'PayPal',      'Paid',    2, '2026-05-02'),
(3, 'Cash',        'Pending', 3, '2026-05-03');

INSERT INTO Trip VALUES
(1, 'OneWay'),
(2, 'RoundTrip'),
(3, 'OneWay');

INSERT INTO FlightTrip VALUES
(1, 1),
(2, 2),
(3, 3);

INSERT INTO Seat VALUES
(1, '1A',  'Business'),
(1, '10B', 'Economy'),
(2, '2C',  'Business'),
(2, '15D', 'Economy'),
(3, '1A',  'First'),
(3, '20E', 'Economy');

INSERT INTO Ticket VALUES
(1, 1500.00, 1, 1, 1, '1A'),
(2, 3200.00, 2, 2, 3, '1A'),
(3,  900.00, 3, 3, 2, '15D');

INSERT INTO Baggage VALUES
(1, 1, 23.00,  0.00),
(2, 1, 32.00, 50.00),
(3, 1, 10.00,  0.00);
