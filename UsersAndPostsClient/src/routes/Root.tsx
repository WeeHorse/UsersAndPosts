import { NavLink, Outlet } from "react-router-dom";

export function Root() {
  return (
    <>
      <nav>
        <NavLink to="/">Home</NavLink>
        <NavLink to="/users">Users</NavLink>
        <NavLink to="/posts">Posts</NavLink>
      </nav>
      <main>
        <Outlet />
      </main>
    </>
  );
}
