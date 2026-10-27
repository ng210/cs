# .NET 8 Web API – Tervezési összefoglaló

## 1. Tervezési alapelvek

A cél egy egyszerű, világos, precízen működő és később bővíthető Web API kialakítása.

- Az API REST-szemléletű, az erőforrásokat URL-eken keresztül teszi elérhetővé.
- A CRUD-műveleteket lehetőség szerint egy univerzális végpont kezeli.
- Az erőforrások, a jogosultságok és az üzleti logika külön felelősségi körökbe tartoznak.
- A belső működéshez szükséges metaadatok nem kerülnek automatikusan a frontendhez.
- A megvalósítás fokozatosan történik; a még nyitott kérdéseket az implementáció előtt vagy közben döntjük el.

## 2. Mi tekinthető erőforrásnak?

Erőforrás lehet minden olyan adat vagy objektum, amelyet az API-n keresztül elérhetővé, lekérdezhetővé vagy módosíthatóvá szeretnénk tenni.

Példák:

- **Entitások:** felhasználók (`User`), csoportok (`Group`), dokumentumok (`Document`).
- **Relációk:** felhasználó csoporttagsága (`Member`), csoportok közötti alárendeltség (`Subgroup`).
- **Összetettebb objektumok:** olyan kapcsolatok, amelyeknek saját attribútumaik vagy önálló üzleti jelentésük van.

A tervezés során kell eldönteni, hogy egy kapcsolat egyszerű reláció maradjon-e, vagy inkább önálló entitásként kezeljük.

## 3. Az univerzális végpont

Az univerzális végpont feladata, hogy a kérés alapján meghatározza:

- az érintett erőforrást vagy erőforrásokat;
- a művelet típusát;
- a művelethez tartozó modellosztályokat;
- a szükséges jogosultságokat;
- a végrehajtás módját.

A HTTP-metódus határozza meg az alapvető műveletet:

- `GET` – lekérdezés;
- `POST` – létrehozás;
- `PUT` – módosítás vagy teljes csere;
- `DELETE` – törlés.

A ténylegesen támogatott műveleteket az adott erőforrás tulajdonságai határozzák meg. Nem szükséges minden erőforráson minden CRUD-műveletet biztosítani.

Speciális üzleti műveletekhez később külön végpontok is kialakíthatók.

## 4. Erőforrások és URL-ek

Az URL-ek erőforrásokat azonosítanak. Az útvonalakban a többes számú főnevek jelölik az erőforrástípust, a C#-modellosztályok nevei viszont egyes számúak.

Példák:

| URL | Jelentés |
|---|---|
| `/api/users` | A felhasználók gyűjteménye |
| `/api/users/22` | A 22-es azonosítójú felhasználó |
| `/api/groups` | A csoportok gyűjteménye |
| `/api/groups/2` | A 2-es azonosítójú csoport |

Az erőforrások stabil, numerikus azonosítót kapnak. A logikai útvonal külön adat, ezért az útvonal megváltoztatása nem módosítja az erőforrás azonosítóját.

A hierarchikus erőforrásoknál a `ParentId` jelöli a szülőkapcsolatot. A logikai útvonal nem feltétlenül egyezik meg a fizikai fájlrendszer útvonalával.

## 5. Relációs erőforrások

Az entitások közötti kapcsolatok szintén erőforrásként kezelhetők.

A reláció URL-jében valamennyi érintett entitásnak azonosíthatónak kell lennie. A reláció nevét is fel kell tüntetni, hogy ugyanazon entitások között többféle kapcsolat is megkülönböztethető legyen.

Példák:

| URL | Reláció neve | Modellosztály |
|---|---|---|
| `/api/groups/2/users/22/member` | `member` | `Member` |
| `/api/groups/2/groups/22/subgroup` | `subgroup` | `Subgroup` |

A relációk feloldásához explicit regiszter készül. Ez határozza meg, hogy az adott útvonal, relációnév és entitáskombináció mely modellosztályhoz tartozik.

A regiszter nem tölthet be tetszőleges, kliens által megadott C#-típusokat.

### Relációazonosítás és összetett kulcs

A reláció önálló azonosító helyett az érintett entitások idegen kulcsaiból képzett összetett kulccsal azonosítható.

Például a `Member` reláció kulcsa:

`(GroupId, UserId)`

A reláció tervezésekor kell eldönteni, hogy önálló reláció marad-e, vagy saját attribútumai és viselkedése miatt inkább önálló entitássá válik.

**Nyitott tervezési kérdés:** egy reláció mindig pontosan két entitás között létezzen-e. Az egyik lehetőség, hogy a több entitást összekapcsoló kapcsolatokat is önálló entitásként modellezzük. Ezzel a relációk kezelése egységesen két entitás közötti kapcsolatokra korlátozható.

## 6. A fő szolgáltatások felelősségei

### UserService

Feladata jelenleg a hitelesített felhasználó adatainak előállítása a JWT token claimjeiből.

A `User` modell alapadatai:

- `Id`
- `FirstName`
- `LastName`
- `Email`
- `ProfileImageUrl`

A felhasználói modell nem tartalmaz szerepköröket. A csoporttagságok és jogosultságok kezelése külön történik.

#### A felhasználó érvényességének ellenőrzése

Később el kell döntenünk, hogy az API milyen gyakran és milyen módon ellenőrizze a felhasználó érvényességét a külső identitásszolgáltatónál, például a Google Identity Services (GIS) használatával.

Két szélsőséges megoldás:

- **Minden kérésnél külső ellenőrzés:** a felhasználó aktuális állapota pontosabban követhető, de a hálózati forgalom és a válaszidő jelentősen megnőhet.
- **Érvényesség a munkamenet alatt:** a felhasználót az érvényes token alapján elfogadjuk a munkamenet során. Ez gyorsabb, de a felhasználó törléséről vagy jogosultságainak megváltozásáról az API nem feltétlenül értesül azonnal.

A későbbi tervezés során olyan megoldást kell választani, amely megfelelő egyensúlyt teremt a teljesítmény és az aktuális felhasználói állapot követése között.

### ResourceService

Feladata az erőforrások kezelése, beleértve:

- az erőforrások CRUD-műveleteit;
- a logikai útvonalak feloldását;
- a relációk és entitástípusok regiszter alapján történő azonosítását;
- az erőforrások belső metaadatainak és támogatott műveleteinek szolgáltatását.

A metaadatokat egyelőre kizárólag az API belső működése használja. A frontend csak az erőforrásokat kapja meg.

### PermissionService

Feladata a hozzáférési jogosultságok ellenőrzése.

A ResourceService által szolgáltatott metaadatokat az univerzális végpont használja a művelet meghatározásához és a jogosultságvizsgálat előkészítéséhez.

A jogosultságvizsgálat a felhasználó, az erőforrás és a kért művelet alapján történik.

## 7. Jogosultsági modell

A jogosultságok csoportokhoz és erőforrásokhoz kapcsolódó szabályokon alapulnak.

A kezdeti műveletek:

- `Read`
- `Create`
- `Update`
- `Delete`

Egy szabály `Allow` és `Deny` listát tartalmazhat.

A csoportok és az erőforrások hierarchikusak:

- a csoportok örökölhetnek jogosultságokat a szülőcsoportoktól;
- az erőforrások örökölhetnek szabályokat a szülőerőforrásoktól;
- a konkrétabb szabályok felülírhatják az örökölt szabályokat.

A csoporthierarchia az elsődleges: az általánosabb csoport szabályai után a specifikusabb csoport szabályai kerülnek feldolgozásra. Egy adott csoporton belül az erőforrás-hierarchia határozza meg a szabályok feldolgozási sorrendjét.

Az azonos szinten jelentkező `Allow`–`Deny` ütközés konfigurációs hibának számít; futás közben a `Deny` élvez elsőbbséget.

A jogosultságok tárolási módja – például JSON-fájl vagy adatbázis – külön döntés.

## 8. Kérések feldolgozása és hibaválaszok

A tervezett feldolgozási sorrend:

1. Az URL és a HTTP-metódus alapján az erőforrás és a művelet azonosítása.
2. Az erőforrás és a művelet támogatottságának ellenőrzése.
3. A hozzáférési jogosultság ellenőrzése.
4. A művelet végrehajtása.
5. A megfelelő HTTP-válasz visszaadása.

A `403 Forbidden` és a `405 Method Not Allowed` közötti választás még nyitott kérdés.

A biztonsági megfontolások alapján dönthetünk úgy, hogy a frontend nem kap információt az elutasítás pontos okáról, és az elutasított kérések egységes választ kapnak.

A metaadatok esetleges későbbi publikálása szintén külön döntés lesz.

### Naplózás és hibák jelzése

Az alsóbb rétegek exception dobással jelzik a hibákat, és a felelős felsőbb réteg naplózza azokat. Ugyanazt a hibát nem naplózzuk minden rétegben.
Logger átadása csak ott szükséges, ahol ténylegesen naplózni kell.

## 9. Nyitott kérdések

A következő kérdéseket a részletes tervezés és az implementáció során kell véglegesíteni:

- Az univerzális végpont pontos URL-feloldási és műveletvégrehajtási mechanizmusa.
- Az erőforrás- és relációregiszter konkrét adatszerkezete.
- A relációk két entitásra korlátozása, illetve a több entitást érintő kapcsolatok modellezése.
- A relációk támogatott műveleteinek meghatározása.
- A ResourceService és a PermissionService pontos együttműködése.
- A felhasználó érvényességének ellenőrzési gyakorisága és a külső identitásszolgáltatóval való együttműködés.
- A jogosultsági hibák HTTP-válaszai és információvédelmi szabályai.
- A jogosultsági konfiguráció betöltése és tárolása.
- Az erőforrás-metaadatok későbbi esetleges frontend-hozzáférése.

A cél az, hogy ezek a döntések ne vezessenek szükségtelenül általános keretrendszer kialakításához: csak annyi általánosítás készüljön, amennyit a tényleges feladat indokol.