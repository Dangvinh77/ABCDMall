import { beforeEach, describe, expect, it, vi } from "vitest";

const { createMock, postMock, getMock, putMock, deleteMock, refreshPostMock } = vi.hoisted(() => ({
  createMock: vi.fn(),
  postMock: vi.fn(),
  getMock: vi.fn(),
  putMock: vi.fn(),
  deleteMock: vi.fn(),
  refreshPostMock: vi.fn(),
}));

vi.mock("axios", () => {
  const instance = {
    get: getMock,
    post: postMock,
    put: putMock,
    delete: deleteMock,
    interceptors: {
      request: { use: vi.fn() },
      response: { use: vi.fn() },
    },
  };

  createMock.mockReturnValue(instance);

  const axiosModule = {
    create: createMock,
    isAxiosError: (error: unknown) => Boolean((error as { isAxiosError?: boolean })?.isAxiosError),
    post: refreshPostMock,
  };

  return {
    __esModule: true,
    default: axiosModule,
    ...axiosModule,
  };
});

import { api } from "./api";

describe("api error mapping", () => {
  beforeEach(() => {
    postMock.mockReset();
    getMock.mockReset();
    putMock.mockReset();
    deleteMock.mockReset();
    refreshPostMock.mockReset();
  });

  it("returns the backend string error message instead of a generic axios status message", async () => {
    postMock.mockRejectedValueOnce({
      isAxiosError: true,
      message: "Request failed with status code 404",
      response: {
        status: 404,
        data: "Email does not exist",
      },
    });

    await expect(api.post("/Auth/forgotpassword/request-otp", {})).rejects.toMatchObject({
      message: "Email does not exist",
      status: 404,
      data: "Email does not exist",
    });
  });
});
