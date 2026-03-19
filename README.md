# UsersAndPosts

Ett minimalt fullstack-exempel med:

- **ASP.NET Minimal API** (SQLite, raw SQL)
- **React + Vite** (Data Router)
- **Kontraktskedja för DTO:er**: C# DTO-records → `dtos.json` → genererade TypeScript-typer

## Struktur

```

UsersAndPosts.sln
UsersAndPosts/                # API (serverar även statiska filer i wwwroot)
UsersAndPostsClient/          # React/Vite-klient
UsersAndPosts.DtoContractGen/ # Tool som genererar dtos.json från C# DTOs

```

- Klientens routes ligger på `/` (t.ex. `/posts`)
- API ligger under `/api` (t.ex. `/api/posts`)
- DTO-kontrakt exponeras på `/api/dtos`

---

## Förutsättningar

- .NET SDK 8
- Node.js 20+ (npm)

---

## Dev-läge (snabb iteration)

### 1) Starta API (port 5000)

Från repo-roten:

```bash
dotnet run --project UsersAndPosts
````

API-bas: `http://localhost:5000/api`

### Swagger / OpenAPI

Swagger är aktiverat i API:t och finns tillgängligt när servern kör.

* Swagger UI: `http://localhost:5000/swagger`
* OpenAPI JSON: `http://localhost:5000/swagger/v1/swagger.json`

**Tips för auth i Swagger:**

* Börja med att köra `POST /api/auth/login` i Swagger (med username/password).
* Session-cookien (`usersandposts.session`) sätts av svaret.
* Därefter kan du testa skyddade endpoints som `POST /api/posts` i samma browser-session.

### Full REST-route referens

| Method | Route | Auth | Success | Vanliga fel |
|---|---|---|---|---|
| POST | `/api/auth/login` | Nej | `200` (`SessionUserDto`) | `400` |
| POST | `/api/auth/logout` | Nej | `204` | - |
| GET | `/api/auth/me` | Session-cookie | `200` (`SessionUserDto`) | `401` |
| GET | `/api/users` | Nej | `200` (`UserDto[]`) | - |
| GET | `/api/users/{id}` | Nej | `200` (`UserDto`) | `404` |
| POST | `/api/users` | Nej | `201` (`UserDto`) | `400` |
| GET | `/api/posts` | Nej | `200` (`PostDto[]`) | - |
| GET | `/api/users/{userId}/posts` | Nej | `200` (`PostDto[]`) | `404` |
| POST | `/api/posts` | Session-cookie | `201` (`{ id }`) | `400`, `401` |
| GET | `/api/dtos` | Nej | `200` (`dtos.json`) | - |

#### Auth

**POST `/api/auth/login`**

* Auth: Nej
* Body: `{"username":"string","password":"string"}`
* 200: `SessionUserDto`
* 400: valideringsfel eller felaktiga credentials

**POST `/api/auth/logout`**

* Auth: Nej (idempotent, tömmer cookie om den finns)
* Body: ingen
* 204: utloggad

**GET `/api/auth/me`**

* Auth: Session-cookie (om inloggad)
* Body: ingen
* 200: `SessionUserDto`
* 401: ej inloggad/ogiltig session

#### Users

**GET `/api/users`**

* Auth: Nej
* Body: ingen
* 200: `UserDto[]`

**GET `/api/users/{id}`**

* Auth: Nej
* Path-param: `id` (int)
* Body: ingen
* 200: `UserDto`
* 404: user finns inte

**POST `/api/users`**

* Auth: Nej
* Body: `{"username":"string","password":"string","displayName":"string"}`
* 201: skapad `UserDto`
* 400: valideringsfel (t.ex. tomma fält, duplicate username)

#### Posts

**GET `/api/posts`**

* Auth: Nej
* Body: ingen
* 200: `PostDto[]`

**GET `/api/users/{userId}/posts`**

* Auth: Nej
* Path-param: `userId` (int)
* Body: ingen
* 200: `PostDto[]`
* 404: user finns inte

**POST `/api/posts`**

* Auth: Ja (session-cookie krävs)
* Body: `{"content":"string"}`
* 201: `{ "id": number }`
* 400: valideringsfel (t.ex. tom content)
* 401: saknad/ogiltig session

> `POST /api/posts` ignorerar klient-identitet och sätter alltid författare från inloggad session.

#### Contract

**GET `/api/dtos`**

* Auth: Nej
* Body: ingen
* 200: rå JSON för DTO-kontraktet (`dtos.json`)

### Auth (session/cookie)

API:t använder cookie-baserad session-auth för skrivning av inlägg.

**Regler:**

* `GET /api/posts` och andra läs-endpoints är öppna utan inloggning.
* `POST /api/posts` kräver inloggning (session-cookie).
* Författare på nya inlägg tas alltid från inloggad session (inte från request-body).

**Logga in (exempel):**

```bash
curl -i -c cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"username":"alice","password":"alice"}' \
  http://localhost:5000/api/auth/login
```

**Skapa user (exempel):**

```bash
curl -i \
  -H "Content-Type: application/json" \
  -d '{"username":"charlie","password":"charlie","displayName":"Charlie"}' \
  http://localhost:5000/api/users
```

**Kolla aktiv session:**

```bash
curl -i -b cookies.txt http://localhost:5000/api/auth/me
```

**Skapa inlägg som inloggad user:**

```bash
curl -i -b cookies.txt \
  -H "Content-Type: application/json" \
  -d '{"content":"Hej från session-auth"}' \
  http://localhost:5000/api/posts
```

**Logga ut:**

```bash
curl -i -b cookies.txt -X POST http://localhost:5000/api/auth/logout
```

### 2) Starta klienten i dev (port 3000)

I en ny terminal:

```bash
cd UsersAndPostsClient
npm install
npm run dev
```

Öppna:

* `http://localhost:3000/posts`

I dev-läge använder Vite en proxy, så `/api/*` på `:3000` skickas vidare till `http://localhost:5000`.

### DTO-flöde i dev

* Klientens `npm run dev` kör `npm run gen:dtos` och genererar `src/generated/dtos.ts`
* `gen:dtos` läser från `../UsersAndPosts/dtos.json`
* `dtos.json` genereras av tool-projektet (se nedan)

Om du ändrar C# DTO-records och vill uppdatera kontraktet:

```bash
dotnet run --project UsersAndPosts.DtoContractGen
```

---

## “Produktionsläge” lokalt (servera klient från API)

I produktionsläge byggs klienten och läggs i `UsersAndPosts/wwwroot`, och Minimal API serverar den på `/`.

### 1) Generera kontrakt (dtos.json)

```bash
dotnet run --project UsersAndPosts.DtoContractGen
```

### 2) Bygg klienten till API:ets wwwroot

```bash
cd UsersAndPostsClient
npm install
npm run build
```

Detta skriver ut statiska filer till:

```
UsersAndPosts/wwwroot/
```

### 3) Kör API och öppna klienten på port 5000

```bash
cd ../UsersAndPosts
dotnet run
```

Öppna:

* `http://localhost:5000/posts`

---

## Vanliga kommandon

**Generera DTO-kontrakt från C#**

```bash
dotnet run --project UsersAndPosts.DtoContractGen
```

**Bygg klienten till wwwroot**

```bash
cd UsersAndPostsClient
npm run build
```

**Kör allt i dev (två terminaler)**

```bash
dotnet run --project UsersAndPosts
cd UsersAndPostsClient && npm run dev
```

---

## Noteringar

* SQLite-databasen skapas/seedas automatiskt av API:t.
* Password lagras i klartext (plain text) i databasen för detta exempel.
* Inlägg kan läsas utan inloggning, men att skapa inlägg kräver inloggning via session-cookie (`/api/auth/login`).
* `VITE_API_BASE` kan sättas om du vill peka klienten mot annat API än `/api` (default är `/api`).
* `dtos.json` används av `/api/dtos` och som input till genereringen av TypeScript-typer.

---

## CI (Continuous Integration)

Projektet innehåller en GitHub Actions-pipeline som automatiskt verifierar att **backend, kontrakt och frontend hänger ihop** vid varje push och pull request mot `main`.

CI-flödet är medvetet uppdelat i tydliga steg som speglar hur projektet är uppbyggt lokalt.

### Vad CI gör

**1. Restore & säkerhetskontroll**

* Återställer alla .NET-beroenden
* Kör en sårbarhetskontroll av NuGet-paket (`dotnet list package --vulnerable`)
* Failar bygget om kända sårbarheter hittas

**2. Build av backend**

* Bygger hela solutionen i `Release`
* Säkerställer att C#-koden (inkl. DTO-records) är korrekt

**3. Generering av DTO-kontrakt**

* Kör `UsersAndPosts.DtoContractGen`
* Genererar `UsersAndPosts/dtos.json` **vid build-time**
* Validerar att filen finns och innehåller giltig JSON

> `dtos.json` versionsstyrs inte – den ses som ett genererat kontrakt baserat på C#-koden.

**4. Build av klient**

* Installerar npm-beroenden
* Genererar TypeScript-typer (`dtos.ts`) från `dtos.json`
* Kör `npm run build`, vilket:

  * bygger React-klienten
  * skriver statiska filer direkt till API-projektets `wwwroot`

**5. Tester**

* Kör alla .NET-tester (`dotnet test`)

**6. Konsistens-kontroll**

* Kontrollerar att inga versionsstyrda filer har ändrats av build-stegen
* Failar CI om bygget genererar filer som borde vara committade (eller ignorerade)

**7. Publish-artefakt**

* Kör `dotnet publish`
* Kopierar `dtos.json` till publish-root (krävs för `/api/dtos`)
* Laddar upp resultatet som en GitHub-artefakt

### Varför detta CI-upplägg?

* **Kontrakt först**
  DTO-kedjan (C# → JSON → TypeScript) verifieras automatiskt.
* **Samma beteende lokalt och i CI**
  CI kör samma kommandon som en utvecklare kör manuellt.
* **Tydliga fel tidigt**
  Problem med kontrakt, typer, klientbuild eller sårbarheter stoppas innan merge.
* **Deploy-agnostiskt**
  CI producerar ett färdigt publish-artefakt utan att anta var eller hur det deployas.

