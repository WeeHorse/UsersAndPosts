import { api } from "./http";
import type { UserCreateDto, UserDto, PostDto } from "../generated/dtos";

export function getUsers() {
  return api<UserDto[]>("/users");
}

export function getUser(id: number) {
  return api<UserDto>(`/users/${id}`);
}

export function createUser(dto: UserCreateDto) {
  return api<UserDto>("/users", {
    method: "POST",
    body: JSON.stringify(dto)
  });
}

export function getUserPosts(userId: number) {
  return api<PostDto[]>(`/users/${userId}/posts`);
}
