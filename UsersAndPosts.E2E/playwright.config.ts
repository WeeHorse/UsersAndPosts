import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./tests",
  fullyParallel: true,
  retries: process.env.CI ? 2 : 0,
  reporter: "list",
  use: {
    baseURL: "http://127.0.0.1:5099",
    trace: "on-first-retry"
  },
  projects: [
    {
      name: "chromium",
      use: { ...devices["Desktop Chrome"] }
    }
  ],
  webServer: {
    command: "dotnet run --project ../UsersAndPosts/UsersAndPosts.csproj --urls http://127.0.0.1:5099",
    cwd: "/Users/b/Desktop/Projects/SYS9/UsersAndPosts/UsersAndPosts.E2E",
    url: "http://127.0.0.1:5099",
    reuseExistingServer: !process.env.CI,
    timeout: 120000
  }
});