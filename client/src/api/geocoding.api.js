import axios from "axios";

export async function searchLocation(query) {
  const { data } = await axios.get(
    "https://nominatim.openstreetmap.org/search",
    {
      params: {
        q: query,
        format: "json",
        limit: 1,
      },
      headers: {
        Accept: "application/json",
      },
    },
  );

  return data;
}
