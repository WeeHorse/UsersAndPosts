import { expect, test } from "@playwright/test";

test("login stores jwt, allows posting, and logout clears auth", async ({ page }) => {
  const postContent = `Playwright auth post ${Date.now()}`;

  await page.goto("/posts");

  await page.getByPlaceholder("username").fill("alice");
  await page.getByPlaceholder("password").fill("abc123");
  await page.getByRole("button", { name: "Logga in" }).click();

  await expect(page.getByText("Inloggad som @alice")).toBeVisible();

  const tokenAfterLogin = await page.evaluate(() => window.localStorage.getItem("usersandposts.jwt"));
  expect(tokenAfterLogin).toBeTruthy();

  await page.getByPlaceholder("Hello...").fill(postContent);
  await page.getByRole("button", { name: "Create" }).click();

  await expect(page.getByText(postContent)).toBeVisible();
  await expect(page.getByText("@alice").first()).toBeVisible();

  await page.getByRole("button", { name: "Logga ut" }).click();

  await expect(page.getByRole("button", { name: "Logga in" })).toBeVisible();

  const tokenAfterLogout = await page.evaluate(() => window.localStorage.getItem("usersandposts.jwt"));
  expect(tokenAfterLogout).toBeNull();
});