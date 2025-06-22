-- Adminer 5.3.0 MariaDB 11.3.2-MariaDB dump

SET NAMES utf8;
SET time_zone = '+00:00';
SET foreign_key_checks = 0;

SET NAMES utf8mb4;

DROP DATABASE IF EXISTS `diabet`;
CREATE DATABASE `diabet` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci */;
USE `diabet`;

DROP TABLE IF EXISTS `callbacks`;
CREATE TABLE `callbacks` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` longtext NOT NULL,
  `Phone` longtext NOT NULL,
  `Date` datetime(6) NOT NULL,
  `PlaceId` int(11) NOT NULL,
  PRIMARY KEY (`Id`),
  KEY `IX_Callbacks_PlaceId` (`PlaceId`),
  CONSTRAINT `FK_Callbacks_Places_PlaceId` FOREIGN KEY (`PlaceId`) REFERENCES `places` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;


DROP TABLE IF EXISTS `eventphotos`;
CREATE TABLE `eventphotos` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` longtext NOT NULL,
  `ImageId` longtext NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;


DROP TABLE IF EXISTS `htmlbodies`;
CREATE TABLE `htmlbodies` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Text` longtext NOT NULL,
  `Name` longtext NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;


DROP TABLE IF EXISTS `places`;
CREATE TABLE `places` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` longtext NOT NULL,
  `ImageId` longtext NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;


DROP TABLE IF EXISTS `questions`;
CREATE TABLE `questions` (
  `Id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` longtext NOT NULL,
  `Phone` longtext NOT NULL,
  `Question` longtext NOT NULL,
  `Answered` tinyint(1) NOT NULL,
  `Date` datetime(6) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;


-- 2025-06-22 15:36:04 UTC
