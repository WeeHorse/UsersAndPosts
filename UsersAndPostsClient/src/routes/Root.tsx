import { Form, NavLink, Outlet, redirect, useLoaderData, useLocation } from "react-router-dom";
import type { ActionFunctionArgs, LoaderFunctionArgs } from "react-router-dom";
import { getSessionUser, login, logout, persistLogin } from "../api/auth";
import type { SessionUserDto } from "../generated/dtos";

export async function rootLoader(_: LoaderFunctionArgs) {
  const sessionUser = await getSessionUser();
  return { sessionUser };
}

export async function rootAction({ request }: ActionFunctionArgs) {
  const data = await request.formData();
  const intent = String(data.get("intent") ?? "");
  const redirectTo = String(data.get("redirectTo") ?? "/");

  if (intent === "login") {
    const username = String(data.get("username") ?? "");
    const password = String(data.get("password") ?? "");
    const loginResponse = await login({ username, password });
    persistLogin(loginResponse);
  }

  if (intent === "logout") {
    await logout();
  }

  return redirect(redirectTo);
}

export function Root() {
  const { sessionUser } = useLoaderData() as { sessionUser: SessionUserDto | null; };
  const location = useLocation();
  const redirectTo = `${location.pathname}${location.search}`;

  return (
    <>
      <nav>
        <NavLink to="/">Home</NavLink>
        <NavLink to="/users">Users</NavLink>
        <NavLink to="/posts">Posts</NavLink>
        {sessionUser ? (
          <Form method="post" style={{ display: "inline-flex", gap: 8, marginLeft: 12, alignItems: "center" }}>
            <input type="hidden" name="intent" value="logout" />
            <input type="hidden" name="redirectTo" value={redirectTo} />
            <small>Inloggad som @{sessionUser.username}</small>
            <button type="submit">Logga ut</button>
          </Form>
        ) : (
          <Form method="post" style={{ display: "inline-flex", gap: 8, marginLeft: 12, alignItems: "center" }}>
            <input type="hidden" name="intent" value="login" />
            <input type="hidden" name="redirectTo" value={redirectTo} />
            <input name="username" placeholder="username" />
            <input name="password" type="password" placeholder="password" />
            <button type="submit">Logga in</button>
          </Form>
        )}
      </nav>
      <main>
        <Outlet />
      </main>
    </>
  );
}
