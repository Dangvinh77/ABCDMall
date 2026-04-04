

const BASE_URL = "http://localhost:5184/api";

export const api = {
  get: async (url: string) => {
    const res = await fetch(`${BASE_URL}${url}`);
    return res.json();
  },

  post: async (url: string, data: any) => {
    const res = await fetch(`${BASE_URL}${url}`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(data),
    });
    return res.json();
  },

  
};

