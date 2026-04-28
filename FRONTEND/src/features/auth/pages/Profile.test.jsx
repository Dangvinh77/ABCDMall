import React from "react";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import Profile from "./Profile";

const { getMock, putMock } = vi.hoisted(() => ({
  getMock: vi.fn(),
  putMock: vi.fn(),
}));

vi.mock("../services/auth", () => ({
  logoutUser: vi.fn(),
}));

vi.mock("../../../core/api/api", () => ({
  default: {
    get: getMock,
    put: putMock,
    post: vi.fn(),
    delete: vi.fn(),
  },
}));

describe("Profile", () => {
  beforeEach(() => {
    getMock.mockReset();
    putMock.mockReset();
  });

  it("submits CCCD image files as a profile approval request payload", async () => {
    getMock
      .mockResolvedValueOnce({
        email: "manager@example.com",
        role: "Manager",
        fullName: "Current Manager",
        address: "Current Address",
        cccd: "111111111",
      })
      .mockResolvedValueOnce([])
      .mockResolvedValueOnce([]);

    putMock.mockResolvedValueOnce({
      message: "Profile update request submitted for admin approval",
      profile: {
        email: "manager@example.com",
        role: "Manager",
        fullName: "Current Manager",
        address: "Current Address",
        cccd: "111111111",
      },
    });

    render(
      <MemoryRouter>
        <Profile />
      </MemoryRouter>,
    );

    expect(await screen.findByText("Current Manager")).toBeInTheDocument();

    fireEvent.click(screen.getByRole("button", { name: "Edit Profile" }));
    fireEvent.change(screen.getByPlaceholderText("Full name"), {
      target: { value: "Updated Manager" },
    });

    const frontFile = new File(["front"], "front.png", { type: "image/png" });
    const backFile = new File(["back"], "back.png", { type: "image/png" });
    fireEvent.change(screen.getByLabelText("CCCD Front Image"), {
      target: { files: [frontFile] },
    });
    fireEvent.change(screen.getByLabelText("CCCD Back Image"), {
      target: { files: [backFile] },
    });

    fireEvent.click(screen.getByRole("button", { name: "Submit Request" }));

    await waitFor(() => {
      expect(putMock).toHaveBeenCalledTimes(1);
    });

    const [url, formData] = putMock.mock.calls[0];
    expect(url).toBe("/Auth/updateprofile");
    expect(formData).toBeInstanceOf(FormData);
    expect(formData.get("fullName")).toBe("Updated Manager");
    expect(formData.get("cccdFrontImage")).toBe(frontFile);
    expect(formData.get("cccdBackImage")).toBe(backFile);
  });
});
