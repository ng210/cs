## Webapp – ötletgyűjtemény

### 1. Alapvető cél
- Dinamikusan bővíthető webalkalmazás létrehozása.
- A frontend központi eleme a `<web-app>` webkomponens.
- A webapp a backenddel együttműködve tölti be a szükséges webkomponenseket és azok erőforrásait.
- A frontend és a .NET Core Web API szorosan együttműködik, különösen a hitelesítés és a jogosultságkezelés területén.

### 2. Webkomponensek kezelése
- A betölthető webkomponensek előre ismertek a szerver számára.
- A szerver csak az alkalmazásban regisztrált komponenseket engedélyezheti betölteni.
- A webapp az API válasza alapján választja ki és tölti be a megfelelő komponenseket.
- A komponensekhez tartozhat JavaScript, HTML-sablon, CSS, kép és más erőforrás.
- A komponenseket a webapp a konfigurációjából beolvasva hozza létre.
- A már inicializált webapp később is betölthet új komponenseket.
- A komponensek közös ősosztálya (`WebComponent`) kezeli az általános működést, például a sablonok és stílusok betöltését, a DOM létrehozását és frissítését.

### 3. Erőforrások betöltése és inicializálás
- A `<web-app>` központilag koordinálja a szükséges erőforrások betöltését.
- Az erőforrások leírása konfigurációs objektumban történhet, például `resources` szótárral.
- A párhuzamos betöltéshez használható a `Promise.all()`.
- A már betöltött erőforrások gyorsítótárazhatók, így ugyanazt nem kell többször letölteni.
- Legyen egyértelmű pont, amikor minden szükséges erőforrás betöltődött, és megkezdődhet az inicializálás.
- A gyermekkomponensek ne kezdjék meg önállóan a működésüket a webapp központi inicializálása előtt.

### 4. Hitelesítés és jogosultságkezelés
- A felhasználó először egy minimális, nyilvános felületet lát: például fejlécet, logót és bejelentkezési lehetőséget.
- Sikeres hitelesítés után a webapp lekéri az adott felhasználó számára elérhető komponenseket.
- A backend határozza meg, hogy a felhasználó milyen komponenseket és API-műveleteket érhet el.
- A frontend csak a jogosultságoknak megfelelő komponenseket tölti be és jeleníti meg.
- A frontend oldali elrejtés önmagában nem jogosultságvédelem: a backendnek minden véAdett API-műveletnél ellenőriznie kell a jogosultságot.
- A hitelesítési és API-kommunikációs logika külön az `ApiClient` és az általa használt hitelesítő komponensek segítségével kezelhető.

### 5. Biztonság
- Az API nem küldhet tetszőleges, végrehajtandó JavaScript-kódot.
- A backend komponensazonosítókat és konfigurációt adjon vissza, a frontend pedig ezeket egy előre meghatározott komponensjegyzék alapján oldja fel.
- A frontend a konfigurációjában megadott komponenseket a szerverről tölti le.
- A backend a szerveren tárolt komponensek fájljait nem közvetlenül, hanem karakter stream formában, esetleg módosítva küldi a kliensnek.
- Csak megbízható, előre regisztrált és így a szerveren tárolt JavaScript-modulok tölthetők be.
- A dinamikus HTML-kezelésnél kerülni kell a nem megbízható tartalom közvetlen, ellenőrizetlen `innerHTML`-be helyezését.
- A frontend kizárólag a működéséhez szükséges és elkerülhetetlen információkat ismerheti meg. A szerver belső fájljainak nevei, elérési útjai, könyvtárszerkezete és belső adatai nem fedhetők fel, kivéve, ha valamely információ közlése feltétlenül szükséges a működéshez.
- A Content Security Policy (CSP) további védelmi rétegként alkalmazható.

### 6. Központi szolgáltatások
- A webapp felügyeli a gyermekkomponensek életciklusát.
- Központi szolgáltatásokat biztosíthat, például az `ApiClient` példányát, vagy eszközkezelőket mint pl. hang lejátszás, kép megjelenítés.
- A komponensek egységes módon kommunikálhatnak a backenddel.
- A közös szolgáltatásokat nem szükséges minden komponensben külön létrehozni.

### 7. További tervezési ötletek
- Egységes komponensregiszter, amely meghatározza a komponensek azonosítóját, modulját és erőforrásait.
- Egységes komponenskonfiguráció, amely meghatározza a betöltéshez szükséges adatokat.
- Elkülönített hitelesítési stratégia, hogy a Google-alapú bejelentkezés később más módszerre is cserélhető legyen.
- Olyan architektúra, amely később támogatja a tokenalapú hitelesítésről a HttpOnly cookie-alapú munkamenetre való áttérést.
- A nyilvános felület és a bejelentkezés utáni, jogosultságfüggő felület elkülönítése.

### 8. Tervezési szempontok
- Inkrementális fejlesztés: az első verzió önmagában is működő alkalmazás legyen, amelyet fokozatosan bővítünk.
- Tesztelhetőség: minden fejlesztési lépés eredménye legyen önállóan kipróbálható és ellenőrizhető.
- Könnyű implementálhatóság: csak olyan részletességű terveket készítsünk, amelyekből közvetlenül megkezdhető a fejlesztés.
- Fokozatosság: a későbbi igényeket vegyük figyelembe, de ne építsük be előre azokat a funkciókat, amelyekre az aktuális verziónak még nincs szüksége.
