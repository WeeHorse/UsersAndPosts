import { Form, useLoaderData, useParams } from "react-router-dom";
import type { ActionFunctionArgs, LoaderFunctionArgs } from "react-router-dom";
import { getUser, getUserPosts } from "../api/users";
import { createPost } from "../api/posts";
import type { PostDto, UserDto } from "../generated/dtos";

export async function userDetailLoader({ params }: LoaderFunctionArgs) {
  const userId = Number(params.userId);
  const user = await getUser(userId);
  const posts = await getUserPosts(userId);
  return { user, posts };
}

export async function userDetailAction({ request, params }: ActionFunctionArgs) {
  const userId = Number(params.userId);
  const data = await request.formData();
  const content = String(data.get("content") ?? "");
  await createPost({ userId, content });
  return null;
}

export function UserDetail() {
  const { user, posts } = useLoaderData() as { user: UserDto; posts: PostDto[]; };
  const { userId } = useParams();

  return (
    <div>
      <h1>User #{userId}</h1>

      <div className="card">
        <strong>{user.displayName}</strong> <small>@{user.username}</small>
        <div><small>Created: {new Date(user.createdAtUtc).toLocaleString()}</small></div>
      </div>

      <div className="card">
        <h3>Create post</h3>
        <Form method="post">
          <label>
            Content
            <textarea name="content" rows={3} placeholder="Write something..." />
          </label>
          <div style={{ marginTop: 8 }}>
            <button type="submit">Post</button>
          </div>
        </Form>
      </div>

      <h3>Posts</h3>
      {posts.length === 0 && <p><small>No posts yet.</small></p>}
      {posts.map((p) => (
        <div className="card" key={p.id}>
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <div>
              <strong>@{p.authorUsername}</strong> <small>({p.userId})</small>
            </div>
            <small>{new Date(p.createdAtUtc).toLocaleString()}</small>
          </div>
          <div style={{ marginTop: 8 }}>{p.content}</div>
        </div>
      ))}
    </div>
  );
}
