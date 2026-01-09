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

API:

* `GET http://localhost:5000/api/users`
* `GET http://localhost:5000/api/posts`
* `GET http://localhost:5000/api/dtos`

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
* `VITE_API_BASE` kan sättas om du vill peka klienten mot annat API än `/api` (default är `/api`).
* `dtos.json` används av `/api/dtos` och som input till genereringen av TypeScript-typer.


