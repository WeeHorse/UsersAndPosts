import { api, API_BASE } from "./http";
import type { AuthLoginResponseDto, LoginDto, SessionUserDto } from "../generated/dtos";
import { clearAccessToken, getAccessToken, setAccessToken } from "./token";

export function login(dto: LoginDto) {
  return api<AuthLoginResponseDto>("/auth/login", {
    method: "POST",
    body: JSON.stringify(dto)
  });
}

export async function logout() {
  clearAccessToken();

  return api<void>("/auth/logout", {
    method: "POST"
  });
}

export async function getSessionUser() {
  const token = getAccessToken();
  if (!token) return null;

  const res = await fetch(`${API_BASE}/auth/me`, {
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`
    }
  });

  if (res.status === 401) {
    clearAccessToken();
    return null;
  }
  if (!res.ok) {
    const text = await res.text().catch(() => "");
    throw new Error(`API ${res.status} ${res.statusText}: ${text}`);
  }

  return (await res.json()) as SessionUserDto;
}

export function persistLogin(response: AuthLoginResponseDto) {
  setAccessToken(response.accessToken);
  return response.user;
}
