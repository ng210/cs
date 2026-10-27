-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Gép: 127.0.0.1:3307
-- Létrehozás ideje: 2026. Okt 27. 07:45
-- Kiszolgáló verziója: 10.4.32-MariaDB
-- PHP verzió: 8.0.30

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Adatbázis: `advent`
--

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `players`
--

CREATE TABLE `players` (
  `id` int(6) NOT NULL,
  `email` varchar(200) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- A tábla adatainak kiíratása `players`
--

INSERT INTO `players` (`id`, `email`) VALUES
(1, 'nemeth.gabor@hbsz.edu.hu');

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `questions`
--

CREATE TABLE `questions` (
  `id` int(6) NOT NULL,
  `valid_on` date NOT NULL,
  `type` int(2) NOT NULL,
  `text` varchar(500) NOT NULL,
  `options` varchar(128) DEFAULT NULL,
  `correct` varchar(100) NOT NULL,
  `image_url` varchar(256) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- A tábla adatainak kiíratása `questions`
--

INSERT INTO `questions` (`id`, `valid_on`, `type`, `text`, `options`, `correct`, `image_url`) VALUES
(1, '2026-10-27', 1, 'Milyen színű a Grincs?', 'kék;zöld;piros;fekete', 'zöld', NULL);

-- --------------------------------------------------------

--
-- Tábla szerkezet ehhez a táblához `response`
--

CREATE TABLE `response` (
  `player_id` int(6) NOT NULL,
  `question_id` int(6) NOT NULL,
  `submitted` datetime(3) NOT NULL,
  `text` varchar(128) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- A tábla adatainak kiíratása `response`
--

INSERT INTO `response` (`player_id`, `question_id`, `submitted`, `text`) VALUES
(1, 1, '2026-10-25 20:17:52.000', 'zöld');

--
-- Indexek a kiírt táblákhoz
--

--
-- A tábla indexei `players`
--
ALTER TABLE `players`
  ADD PRIMARY KEY (`id`);

--
-- A tábla indexei `questions`
--
ALTER TABLE `questions`
  ADD PRIMARY KEY (`id`);

--
-- A kiírt táblák AUTO_INCREMENT értéke
--

--
-- AUTO_INCREMENT a táblához `players`
--
ALTER TABLE `players`
  MODIFY `id` int(6) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- AUTO_INCREMENT a táblához `questions`
--
ALTER TABLE `questions`
  MODIFY `id` int(6) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
