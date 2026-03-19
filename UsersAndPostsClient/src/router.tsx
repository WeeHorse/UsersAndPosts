import { createBrowserRouter } from "react-router-dom";
import { Root, rootLoader, rootAction } from "./routes/Root";
import { Home } from "./routes/Home";
import { Users, usersLoader, usersAction } from "./routes/Users";
import { UserDetail, userDetailLoader, userDetailAction } from "./routes/UserDetail";
import { Posts, postsLoader, postsAction } from "./routes/Posts";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <Root />,
    loader: rootLoader,
    action: rootAction,
    children: [
      { index: true, element: <Home /> },
      {
        path: "users",
        element: <Users />,
        loader: usersLoader,
        action: usersAction
      },
      {
        path: "users/:userId",
        element: <UserDetail />,
        loader: userDetailLoader,
        action: userDetailAction
      },
      {
        path: "posts",
        element: <Posts />,
        loader: postsLoader,
        action: postsAction
      }
    ]
  }
]);
