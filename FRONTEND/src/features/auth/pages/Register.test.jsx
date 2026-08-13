import React from "react";
import { fireEvent, render, screen, waitFor } from "@testing-library/react";
import { MemoryRouter } from "react-router-dom";
import { beforeEach, describe, expect, it, vi } from "vitest";
import Register from "./Register";

const { postMock } = vi.hoisted(() => ({
  postMock: vi.fn(),
}));

vi.mock("../../../core/api/api", () => ({
  default: {
    post: postMock,
    get: vi.fn(),
    put: vi.fn(),
    delete: vi.fn(),
  },
}));

describe("Register", () => {
  beforeEach(() => {
    postMock.mockReset();
  });

  it("submits manager onboarding as multipart form data", async () => {
    postMock.mockResolvedValueOnce({
      emailSent: true,
    });

    render(
      <MemoryRouter>
        <Register />
      </MemoryRouter>,
    );

    fireEvent.change(screen.getByLabelText("Full Name"), { target: { value: "New Manager" } });
    fireEvent.change(screen.getByLabelText("Email"), { target: { value: "manager@example.com" } });
    fireEvent.change(screen.getByLabelText("Shop Name"), { target: { value: "Shop Alpha" } });
    fireEvent.change(screen.getByLabelText("CCCD"), { target: { value: "123456789" } });
    fireEvent.change(screen.getByLabelText("Floor"), { target: { value: "L2" } });
    fireEvent.change(screen.getByLabelText("Location Slot"), { target: { value: "A-12" } });
    fireEvent.change(screen.getByLabelText("Lease Start Date"), { target: { value: "2026-05-01" } });
    fireEvent.change(screen.getByLabelText("Lease Term Days"), { target: { value: "30" } });
    fireEvent.change(screen.getByLabelText("Electricity Fee"), { target: { value: "100" } });
    fireEvent.change(screen.getByLabelText("Water Fee"), { target: { value: "50" } });
    fireEvent.change(screen.getByLabelText("Service Fee"), { target: { value: "25" } });

    fireEvent.click(screen.getByRole("button", { name: "Create User" }));

    await waitFor(() => {
      expect(postMock).toHaveBeenCalledTimes(1);
    });

    const [url, formData] = postMock.mock.calls[0];
    expect(url).toBe("/Auth/register");
    expect(formData).toBeInstanceOf(FormData);
    expect(formData.get("email")).toBe("manager@example.com");
    expect(formData.get("locationSlot")).toBe("A-12");
    expect(formData.get("serviceFee")).toBe("25");
  });
});
