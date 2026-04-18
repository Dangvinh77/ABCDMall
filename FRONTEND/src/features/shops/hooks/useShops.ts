import { useEffect, useState } from "react";
import { getShops, type Shop } from "../api/shopApi";

export function useShops() {
  const [shops, setShops] = useState<Shop[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let active = true;

    getShops()
      .then((data) => {
        if (!active) {
          return;
        }

        setShops(data);
        setError(null);
      })
      .catch((requestError: unknown) => {
        if (!active) {
          return;
        }

        setShops([]);
        setError(requestError instanceof Error ? requestError.message : "Unable to load shops.");
      })
      .finally(() => {
        if (active) {
          setLoading(false);
        }
      });

    return () => {
      active = false;
    };
  }, []);

  return { shops, loading, error };
}
