CREATE DATABASE PROG2B_PART1;

USE PROG2B_PART1;

-- Create Users Table

CREATE TABLE Users (
    user_id INT PRIMARY KEY AUTO_INCREMENT,
    username VARCHAR(50) NOT NULL,
    password VARCHAR(255) NOT NULL,
    email VARCHAR(100) NOT NULL,
    role ENUM('Organiser', 'Participant') NOT NULL
);

-- Create Events Table
CREATE TABLE Events (
    event_id INT PRIMARY KEY AUTO_INCREMENT,
    event_name VARCHAR(100) NOT NULL,
    event_date DATE NOT NULL,
    location VARCHAR(100) NOT NULL,
    organiser_id INT,
    FOREIGN KEY (organiser_id) REFERENCES Users(user_id)
);

-- Create Categories Table
CREATE TABLE Categories (
    category_id INT PRIMARY KEY AUTO_INCREMENT,
    category_name VARCHAR(50) NOT NULL,
    event_id INT,
    FOREIGN KEY (event_id) REFERENCES Events(event_id)
);

-- Create Enrolments Table
CREATE TABLE Enrolments (
    enrolment_id INT PRIMARY KEY AUTO_INCREMENT,
    participant_id INT,
    event_id INT,
    category_id INT,
    FOREIGN KEY (participant_id) REFERENCES Users(user_id),
    FOREIGN KEY (event_id) REFERENCES Events(event_id),
    FOREIGN KEY (category_id) REFERENCES Categories(category_id)
);

-- Create Results Table
CREATE TABLE Results (
    result_id INT PRIMARY KEY AUTO_INCREMENT,
    enrolment_id INT,
    finish_time TIME NOT NULL,
    finishing_position INT NOT NULL,
    FOREIGN KEY (enrolment_id) REFERENCES Enrolments(enrolment_id)
);

-- Seed Data
INSERT INTO Users (username, password, email, role) VALUES
('organiser1', 'hashed_password1', 'organiser1@example.com', 'Organiser'),
('organiser2', 'hashed_password2', 'organiser2@example.com', 'Organiser'),
('participant1', 'hashed_password3', 'participant1@example.com', 'Participant'),
('participant2', 'hashed_password4', 'participant2@example.com', 'Participant');

INSERT INTO Events (event_name, event_date, location, organiser_id) VALUES
('Cape Town Marathon', '2023-09-17', 'Cape Town', 1),
('Durban Cycle Race', '2023-10-01', 'Durban', 1),
('Johannesburg Fun Run', '2023-11-05', 'Johannesburg', 2);

INSERT INTO Categories (category_name, event_id) VALUES
('10km', 1),
('21km', 1),
('5km', 2),
('Cycling', 2),
('Fun Run', 3);

INSERT INTO Enrolments (participant_id, event_id, category_id) VALUES
(3, 1, 1),
(3, 2, 3),
(4, 1, 2),
(4, 3, 5);

INSERT INTO Results (enrolment_id, finish_time, finishing_position) VALUES
(1, '00:45:30', 1),
(2, '00:30:15', 2);
