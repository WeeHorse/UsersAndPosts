import { api, API_BASE } from "./http";
import type { LoginDto, SessionUserDto } from "../generated/dtos";

export function login(dto: LoginDto) {
  return api<SessionUserDto>("/auth/login", {
    method: "POST",
    body: JSON.stringify(dto)
  });
}

export function logout() {
  return api<void>("/auth/logout", {
    method: "POST"
  });
}

export async function getSessionUser() {
  const res = await fetch(`${API_BASE}/auth/me`, {
    credentials: "include",
    headers: { "Content-Type": "application/json" }
  });

  if (res.status === 401) return null;
  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(`API ${res.status} ${res.statusText}: ${text}`);
  }

  return (await res.json()) as SessionUserDto;
}
