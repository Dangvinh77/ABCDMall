import { render, screen } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { describe, expect, it, vi } from "vitest";

vi.mock("../features/auth/pages/ChangeInitialPassword", () => ({
  default: () => <div>Change Initial Password</div>,
}));

import { AppRoutes } from "./AppRoutes";

describe("AppRoutes", () => {
  it("renders the initial password change route", async () => {
    render(
      <MemoryRouter initialEntries={["/change-initial-password?token=test-token"]}>
        <AppRoutes />
      </MemoryRouter>,
    );

    expect(await screen.findByText("Change Initial Password")).toBeInTheDocument();
  });
});
