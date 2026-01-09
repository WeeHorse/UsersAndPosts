import { Form, Link, useLoaderData } from "react-router-dom";
import type { ActionFunctionArgs, LoaderFunctionArgs } from "react-router-dom";
import { getUsers, createUser } from "../api/users";
import type { UserDto } from "../generated/dtos";

export async function usersLoader(_: LoaderFunctionArgs) {
  const users = await getUsers();
  return { users };
}

export async function usersAction({ request }: ActionFunctionArgs) {
  const data = await request.formData();
  const username = String(data.get("username") ?? "");
  const displayName = String(data.get("displayName") ?? "");

  await createUser({ username, displayName });
  return null;
}

export function Users() {
  const { users } = useLoaderData() as { users: UserDto[]; };

  return (
    <div>
      <h1>Users</h1>

      <div className="card">
        <h3>Create user</h3>
        <Form method="post" className="row">
          <div>
            <label>
              Username
              <input name="username" placeholder="alice" />
            </label>
          </div>
          <div>
            <label>
              Display name
              <input name="displayName" placeholder="Alice" />
            </label>
          </div>
          <div>
            <button type="submit">Create</button>
          </div>
        </Form>
      </div>

      {users.map((u) => (
        <div className="card" key={u.id}>
          <div style={{ display: "flex", justifyContent: "space-between", gap: 12 }}>
            <div>
              <strong>{u.displayName}</strong> <small>@{u.username}</small>
              <div><small>Created: {new Date(u.createdAtUtc).toLocaleString()}</small></div>
            </div>
            <Link to={`/users/${u.id}`}>Open</Link>
          </div>
        </div>
      ))}
    </div>
  );
}
