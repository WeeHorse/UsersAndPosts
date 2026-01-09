import { Form, Link, useLoaderData } from "react-router-dom";
import type { ActionFunctionArgs, LoaderFunctionArgs } from "react-router-dom";
import { getPosts, createPost } from "../api/posts";
import type { PostDto } from "../generated/dtos";

export async function postsLoader(_: LoaderFunctionArgs) {
  const posts = await getPosts();
  return { posts };
}

export async function postsAction({ request }: ActionFunctionArgs) {
  const data = await request.formData();
  const userId = Number(data.get("userId"));
  const content = String(data.get("content") ?? "");
  await createPost({ userId, content });
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
              UserId
              <input name="userId" type="number" min={1} placeholder="1" />
            </label>
          </div>
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
        <p><small>Tips: gå via <Link to="/users">Users</Link> och öppna en user för att posta “rätt”.</small></p>
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
