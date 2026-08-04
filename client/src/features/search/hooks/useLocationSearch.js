import { toast } from "react-toastify";

import { searchLocation } from "../../../api/geocoding.api";

import { useSearch } from "../context/SearchContext";

function useLocationSearch() {
  const { searchText, setCoordinates, radius, setSearchParams } = useSearch();

  const searchByLocation = async () => {
    const query = searchText.trim();

    if (!query) {
      toast.warning("Please enter a location.");
      return;
    }

    try {
      const locations = await searchLocation(query);

      if (!locations.length) {
        toast.error("Location not found.");
        return;
      }

      const location = locations[0];

      const latitude = Number(location.lat);
      const longitude = Number(location.lon);

      setCoordinates({
        latitude,
        longitude,
      });
      console.log("Search Result:", {
        latitude,
        longitude,
      });
      setSearchParams({
        lat: latitude,
        lng: longitude,
        radius,
      });
    } catch (error) {
      console.error(error);

      toast.error("Unable to search location.");
    }
  };

  return {
    searchByLocation,
  };
}

export default useLocationSearch;
