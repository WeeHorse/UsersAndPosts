import { Form, useLoaderData } from "react-router-dom";
import type { ActionFunctionArgs, LoaderFunctionArgs } from "react-router-dom";
import { getPosts, createPost } from "../api/posts";
import type { PostDto } from "../generated/dtos";

export async function postsLoader(_: LoaderFunctionArgs) {
  const posts = await getPosts();
  return { posts };
}

export async function postsAction({ request }: ActionFunctionArgs) {
  const data = await request.formData();
  const content = String(data.get("content") ?? "");
  await createPost({ content });
  return null;
}

export function Posts() {
  const { posts } = useLoaderData() as { posts: PostDto[]; };

  if (!Array.isArray(posts)) {
    return <pre>Expected posts to be an array, got: {JSON.stringify(posts, null, 2)}</pre>;
  }

  return (
    <div>
      <h1>Posts</h1>

      <div className="card">
        <h3>Create post</h3>
        <Form method="post" className="row">
          <div>
            <label>
              Content
              <input name="content" placeholder="Hello..." />
            </label>
          </div>
          <div>
            <button type="submit">Create</button>
          </div>
        </Form>
        <p><small>Du måste vara inloggad för att skapa inlägg.</small></p>
      </div>

      {posts.map((p) => (
        <div className="card" key={p.id}>
          <div style={{ display: "flex", justifyContent: "space-between" }}>
            <div>
              <strong>@{p.authorUsername}</strong>{" "}
              <small>(userId: {p.userId})</small>
            </div>
            <small>{new Date(p.createdAtUtc).toLocaleString()}</small>
          </div>
          <div style={{ marginTop: 8 }}>{p.content}</div>
        </div>
      ))}
    </div>
  );
}
