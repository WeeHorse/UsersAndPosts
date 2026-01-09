import { useEffect, useState } from "react";

export function Home() {
  const [dtos, setDtos] = useState<string>("");

  useEffect(() => {
    fetch("/api/dtos")
      .then((r) => r.text())
      .then(setDtos)
      .catch(() => setDtos("Could not load /dtos (is the API running on :5000?)"));
  }, []);

  return (
    <div>
      <h1>UsersAndPosts</h1>
      <p>Minimal Vite + React + Data Router-klient mot Minimal API.</p>

      <div className="card">
        <h3>DTO contract (server: /dtos)</h3>
        <pre style={{ whiteSpace: "pre-wrap" }}>{dtos}</pre>
      </div>
    </div>
  );
}
