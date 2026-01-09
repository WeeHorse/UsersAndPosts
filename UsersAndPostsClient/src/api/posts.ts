import { api } from "./http";
import type { PostCreateDto, PostDto } from "../generated/dtos";

export function getPosts() {
  return api<PostDto[]>("/posts");
}

export function createPost(dto: PostCreateDto) {
  return api<{ id: number; }>("/posts", {
    method: "POST",
    body: JSON.stringify(dto)
  });
}
